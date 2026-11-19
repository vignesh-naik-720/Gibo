/**
 * TypeScript Contracts: Privacy & Data Governance
 * Path: specs/006-privacy-data-governance/contracts/privacy.types.ts
 */

export type ConsentCategory =
  | 'CoreProfile'
  | 'HealthConnectSync'
  | 'AssistantChatContext'
  | 'AnonymousResearch';

export interface ConsentItemDto {
  category: ConsentCategory;
  purpose: string;
  policyVersion: string;
  isGranted: boolean;
  grantedAtUtc: string;
  revokedAtUtc?: string | null;
}

export interface UserDataExportDto {
  userId: string;
  exportTimestampUtc: string;
  profile: {
    preferredLocale: string;
    timezone: string;
    createdAtUtc: string;
  };
  consents: ConsentItemDto[];
  habitSummary: {
    totalActionsCompleted: number;
    totalBarriersLogged: number;
    primaryDomain: string;
  };
  totalXpEarned: number;
}
