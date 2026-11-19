/**
 * TypeScript Contracts: Gamification & Weekly Styles
 * Path: specs/004-gamification-and-weekly-styles/contracts/gamification.types.ts
 */

export type XpReason =
  | 'TaskCompleted'
  | 'PartialCompleted'
  | 'ReflectionReward'
  | 'WeeklyMilestone';

export type WeeklyStyleType =
  | 'ComebackChampion'
  | 'CuriousExplorer'
  | 'MindfulBalancer'
  | 'SteadyBuilder'
  | 'MomentumMaker';

export interface XpTransactionDto {
  transactionId: string;
  amount: number;
  reason: XpReason;
  referenceId?: string | null;
  createdAtUtc: string;
}

export interface XpBalanceDto {
  totalXp: number;
  recentTransactions: XpTransactionDto[];
}

export interface WeeklyInsightDto {
  insightId: string;
  weekStartDate: string;
  actionsCompleted: number;
  assignedStyle: WeeklyStyleType;
  title: string;
  encouragement: string;
  isHiddenByUser: boolean;
}

export interface DomainTrendDto {
  domain: 'Movement' | 'SleepRoutine' | 'Nutrition';
  sensorUnits?: number | null;
  selfReportedCount: number;
  daysActive: number;
  observationalSummary: string;
}

export interface MonthlyTrendsDto {
  monthYear: string;
  domains: DomainTrendDto[];
}
