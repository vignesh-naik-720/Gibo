/**
 * TypeScript Contracts: Adaptive Recovery Loop
 * Path: specs/003-adaptive-recovery-loop/contracts/recovery.types.ts
 */

export type BarrierCategory =
  | 'TimingConflict'
  | 'TooDifficult'
  | 'LowEnergy'
  | 'AccessOrWeather'
  | 'Forgot'
  | 'UnwellOrPain'
  | 'PreferNotToSay';

export type AdaptationType =
  | 'ReduceTargetSize'
  | 'RescheduleWindow'
  | 'ReplaceWithLowEnergyAlternative'
  | 'ConvertToIndoorAlternative'
  | 'TemporaryPause'
  | 'SafetyHalt';

export type AdaptationStatus = 'Proposed' | 'Accepted' | 'Declined';

export interface PlanAdaptationDto {
  adaptationId: string;
  adaptationType: AdaptationType;
  originalTarget: number;
  adaptedTarget: number;
  targetUnit: string;
  suggestedInstruction: string;
  status: AdaptationStatus;
}

export interface EmergencyGuidanceDto {
  message: string;
  ambulanceNumber: string;
  telehealthNumber: string;
  supportCenterNumber: string;
}

export interface SubmitBarrierRequestDto {
  taskInstanceId: string;
  category: BarrierCategory;
  userNote?: string;
}

export interface BarrierCheckInResponseDto {
  feedbackId: string;
  xpAwarded: number;
  isSafetyHaltActive: boolean;
  adaptation?: PlanAdaptationDto | null;
  emergencyGuidance?: EmergencyGuidanceDto | null;
}

export interface RespondAdaptationRequestDto {
  accept: boolean;
  customTarget?: number;
}

export interface ReEntryStatusDto {
  isReturningAfterGap: boolean;
  daysInactive: number;
  totalXpPreserved: number;
  welcomeMessage: string;
  suggestedRestartLevel: 'Gentle' | 'Standard';
}

export interface ResolveSafetyRequestDto {
  isClearedForActivity: boolean;
}

export interface SafetyResolutionResponseDto {
  success: boolean;
  message: string;
}
