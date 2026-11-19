/**
 * TypeScript Type Definitions for Gibo Onboarding & Consent Architecture
 * Compatible with React Native 0.88.x and strict TypeScript mode.
 */

export type LocaleCode = 'en' | 'ar' | 'hi';

export type GoalDomain = 'Movement' | 'SleepRoutine' | 'Nutrition';

export type WorkSchedulePattern = 'StandardDaytime' | 'ShiftWorker' | 'FlexibleOrIrregular';

export type BaselineFitnessLevel = 'Starter' | 'Intermediate' | 'Active';

export type ConsentCategory = 
  | 'CoreProfile' 
  | 'HealthConnectSync' 
  | 'AssistantChatContext' 
  | 'AnonymousResearch';

export interface ConsentItemInput {
  category: ConsentCategory;
  granted: boolean;
  policyVersion: string;
}

export interface OnboardingRegistrationRequest {
  isAdultConfirmed: boolean;
  preferredLocale: LocaleCode;
  timezoneId: string;
  primaryGoalDomain: GoalDomain;
  workPattern?: WorkSchedulePattern;
  indoorPreferred?: boolean;
  maxDailyActions?: number; // 1, 2, or 3 (default: 1)
  quietHoursStart?: string; // HH:mm:ss
  quietHoursEnd?: string;   // HH:mm:ss
  baselineFitnessLevel?: BaselineFitnessLevel;
  accessibilityTags?: string[];
  consents: ConsentItemInput[];
}

export interface FirstPlanPreviewResponse {
  taskTemplateId: string;
  instruction: string;
  verb: string;
  target: string;
  estimatedMinutes: number;
  evidenceMode: 'Manual' | 'HealthConnect' | 'Both';
  whyItFits: string;
}

export interface OnboardingRegistrationResponse {
  userId: string;
  status: 'Active' | 'Suspended';
  registeredAt: string; // ISO 8601 UTC
  firstPlan: FirstPlanPreviewResponse;
}

export interface FirstPlanPreviewRequest {
  goalDomain: GoalDomain;
  baselineFitnessLevel: BaselineFitnessLevel;
  indoorPreferred: boolean;
  locale?: LocaleCode;
}

export interface ApiErrorResponse {
  errorCode: string;
  message: string;
  details?: string[];
}
