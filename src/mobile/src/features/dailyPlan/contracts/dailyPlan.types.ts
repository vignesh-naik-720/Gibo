/**
 * TypeScript Contracts: Daily Plan Generation & Action Engine
 * Path: src/mobile/src/features/dailyPlan/contracts/dailyPlan.types.ts
 */

export type GoalDomain = 'Movement' | 'SleepRoutine' | 'Nutrition';

export type MeasurementType =
  | 'DurationMinutes'
  | 'StepCount'
  | 'Repetitions'
  | 'BooleanCheck'
  | 'ServingCount';

export type TaskDifficulty = 'Starter' | 'Standard' | 'Advanced';

export type TaskState =
  | 'Scheduled'
  | 'Completed'
  | 'Partial'
  | 'Skipped'
  | 'Expired'
  | 'Paused';

export type EvidenceSource = 'Manual' | 'HealthConnect';

export interface TaskInstanceDto {
  taskInstanceId: string;
  taskTemplateId: string;
  domain: GoalDomain;
  actionVerb: string;
  title: string;
  targetValue: number;
  targetUnit: string;
  estimatedMinutes: number;
  state: TaskState;
  evidenceSource: EvidenceSource | null;
  xpAwarded: number;
  rationale: string;
  isStarterFallback: boolean;
  displayOrder: number;
}

export interface DailyPlanDto {
  planId: string;
  planDate: string; // YYYY-MM-DD
  algorithmVersion: string;
  userEditsApplied: boolean;
  totalXpEarned: number;
  tasks: TaskInstanceDto[];
}

export interface CompleteTaskRequestDto {
  evidenceSource: EvidenceSource;
  measuredValue?: number;
  clientEventId: string;
  recordedAtUtc: string;
}

export interface TaskCompletionResultDto {
  taskInstanceId: string;
  state: 'Completed' | 'Partial';
  xpAwarded: number;
  isDuplicate: boolean;
  completedAtUtc: string;
}

export interface SwapTaskRequestDto {
  replacementTemplateId: string;
  reason?: string;
  locale?: string;
}

export interface AdjustTaskTargetRequestDto {
  newTargetValue: number;
}

export interface QueuedOfflineEvent {
  clientEventId: string;
  taskInstanceId: string;
  evidenceSource: EvidenceSource;
  measuredValue?: number;
  recordedAtUtc: string;
}

export interface SyncOfflineEventsRequestDto {
  events: QueuedOfflineEvent[];
}

export interface SyncOfflineEventsResultDto {
  syncedCount: number;
  duplicateCount: number;
  totalXpAwarded?: number;
}
