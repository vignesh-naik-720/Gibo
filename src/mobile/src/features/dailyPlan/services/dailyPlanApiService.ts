/**
 * Daily Plan API Client Service
 * Path: src/mobile/src/features/dailyPlan/services/dailyPlanApiService.ts
 */

import {
  DailyPlanDto,
  CompleteTaskRequestDto,
  TaskCompletionResultDto,
  SwapTaskRequestDto,
  SyncOfflineEventsRequestDto,
  SyncOfflineEventsResultDto
} from '../contracts/dailyPlan.types';
import { API_BASE_URL as BASE } from '../../../config/api';

const API_BASE_URL = `${BASE}/api/v1`;

export const dailyPlanApi = {
  async getTodayPlan(): Promise<DailyPlanDto> {
    const res = await fetch(`${API_BASE_URL}/plans/today`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });
    if (!res.ok) {
      throw new Error(`Failed to fetch today's plan: ${res.status}`);
    }
    return res.json();
  },

  async generatePlan(forceRegenerate: boolean = false, locale: string = 'en'): Promise<DailyPlanDto> {
    const idempotencyKey = 'gen-' + Date.now();
    const res = await fetch(`${API_BASE_URL}/plans/generate?locale=${locale}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Idempotency-Key': idempotencyKey,
      },
      body: JSON.stringify({ forceRegenerate }),
    });
    if (!res.ok) {
      throw new Error(`Failed to generate plan: ${res.status}`);
    }
    return res.json();
  },

  async completeTask(taskId: string, payload: CompleteTaskRequestDto): Promise<TaskCompletionResultDto> {
    const res = await fetch(`${API_BASE_URL}/tasks/${taskId}/complete`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Idempotency-Key': payload.clientEventId,
      },
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      throw new Error(`Failed to complete task: ${res.status}`);
    }
    return res.json();
  },

  async swapTask(taskId: string, payload: SwapTaskRequestDto): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/tasks/${taskId}/swap`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      throw new Error(`Failed to swap task: ${res.status}`);
    }
    return res.json();
  },

  async adjustTarget(taskId: string, newTarget: number): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/tasks/${taskId}/target`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ newTarget }),
    });
    if (!res.ok) {
      throw new Error(`Failed to adjust target: ${res.status}`);
    }
    return res.json();
  },

  async syncOfflineEvents(payload: SyncOfflineEventsRequestDto): Promise<SyncOfflineEventsResultDto> {
    const res = await fetch(`${API_BASE_URL}/tasks/sync-offline`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      throw new Error(`Failed to sync offline events: ${res.status}`);
    }
    return res.json();
  }
};
