/**
 * Offline Sync Service & Deduplicating Event Queue
 * Path: src/mobile/src/features/dailyPlan/services/offlineSyncService.ts
 * Enforces Principle VI (Offline Resilience & Idempotency):
 * - Buffers completion events locally when network is disconnected.
 * - Flushes queued completions upon network reconnect via POST /api/v1/tasks/sync-offline.
 * - Retains client UUIDs to prevent double-crediting XP on the server.
 */

import { EvidenceSource, SyncOfflineEventsResultDto } from '../contracts/dailyPlan.types';
import { dailyPlanApi } from './dailyPlanApiService';

export interface QueuedOfflineEvent {
  taskInstanceId: string;
  evidenceSource: EvidenceSource;
  measuredValue?: number;
  clientEventId: string;
  recordedAtUtc: string;
}

class OfflineSyncService {
  private queue: QueuedOfflineEvent[] = [];
  private isSyncing = false;
  private isOnline = true;
  private listeners: Array<(queueLength: number) => void> = [];

  constructor() {
    this.initNetworkListener();
  }

  /**
   * Enqueues a task completion event locally.
   */
  public enqueueCompletion(
    taskInstanceId: string,
    evidenceSource: EvidenceSource,
    measuredValue?: number
  ): QueuedOfflineEvent {
    const event: QueuedOfflineEvent = {
      taskInstanceId,
      evidenceSource,
      measuredValue,
      clientEventId: 'evt-' + Date.now() + '-' + Math.random().toString(36).substring(2, 9),
      recordedAtUtc: new Date().toISOString(),
    };

    this.queue.push(event);
    this.notifyListeners();

    // If currently online, attempt immediate flush
    if (this.isOnline) {
      this.flushQueue().catch((err) => {
        console.warn('[OFFLINE_QUEUE] Immediate flush failed, will retry on reconnect:', err);
      });
    }

    return event;
  }

  /**
   * Flushes all pending completion events to the backend sync endpoint.
   */
  public async flushQueue(): Promise<SyncOfflineEventsResultDto | null> {
    if (this.isSyncing || this.queue.length === 0) {
      return null;
    }

    this.isSyncing = true;
    const batchToSync = [...this.queue];

    try {
      const payload = {
        events: batchToSync.map((e) => ({
          taskInstanceId: e.taskInstanceId,
          evidenceSource: e.evidenceSource,
          measuredValue: e.measuredValue,
          clientEventId: e.clientEventId,
          recordedAtUtc: e.recordedAtUtc,
        })),
      };

      const result = await dailyPlanApi.syncOfflineEvents(payload);

      // Remove synced items from queue
      const syncedClientIds = new Set(batchToSync.map((e) => e.clientEventId));
      this.queue = this.queue.filter((e) => !syncedClientIds.has(e.clientEventId));
      this.notifyListeners();

      console.log(`[OFFLINE_SYNC_SUCCESS] Flushed ${result.syncedCount} events (+${result.totalXpAwarded} XP)`);
      return result;
    } catch (error) {
      console.warn('[OFFLINE_SYNC_ERROR] Failed to flush offline queue:', error);
      throw error;
    } finally {
      this.isSyncing = false;
    }
  }

  public getPendingCount(): number {
    return this.queue.length;
  }

  public getPendingEvents(): ReadonlyArray<QueuedOfflineEvent> {
    return [...this.queue];
  }

  public subscribe(listener: (queueLength: number) => void): () => void {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  public setOnlineStatus(online: boolean) {
    const wasOffline = !this.isOnline;
    this.isOnline = online;

    if (wasOffline && online) {
      console.log('[OFFLINE_QUEUE] Network reconnected, triggering auto-flush...');
      this.flushQueue().catch((err) => {
        console.warn('[OFFLINE_QUEUE] Auto-flush failed on reconnect:', err);
      });
    }
  }

  private initNetworkListener() {
    // Basic ping/reconnect heartbeat interval for mobile without external native modules
    if (typeof setInterval !== 'undefined') {
      setInterval(() => {
        // Attempt flush if items exist
        if (this.queue.length > 0 && !this.isSyncing) {
          this.flushQueue().catch(() => {
            // Still offline, remain quiet
          });
        }
      }, 15000);
    }
  }

  private notifyListeners() {
    this.listeners.forEach((listener) => {
      try {
        listener(this.queue.length);
      } catch (err) {
        console.error('Error notifying queue listener:', err);
      }
    });
  }
}

export const offlineSyncService = new OfflineSyncService();
