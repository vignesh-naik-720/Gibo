/**
 * Zustand Store for Mobile Progressive Onboarding State
 * Manages trilingual locale, adult confirmation, granular consent selections,
 * chosen goals, and contextual feasibility constraints.
 */

import { create } from 'zustand';
import {
  ConsentCategory,
  ConsentItemInput,
  GoalDomain,
  LocaleCode,
  WorkSchedulePattern,
  BaselineFitnessLevel,
  FirstPlanPreviewResponse,
} from '../contracts/onboarding.types';

export interface OnboardingState {
  // Step 1: Language & Eligibility
  locale: LocaleCode;
  isAdultConfirmed: boolean;

  // Step 2: Granular Consent Selections (UAE PDPL Compliant)
  consents: Record<ConsentCategory, boolean>;

  // Step 3: Primary Goal & Constraints
  primaryGoalDomain: GoalDomain;
  workPattern: WorkSchedulePattern;
  indoorPreferred: boolean;
  maxDailyActions: number; // 1, 2, or 3
  quietHoursStart: string;
  quietHoursEnd: string;

  // Step 4: Baseline & Restrictions
  baselineFitnessLevel: BaselineFitnessLevel;
  accessibilityTags: string[];

  // Step 5: First Plan Preview
  firstPlanPreview: FirstPlanPreviewResponse | null;

  // Actions
  setLocale: (locale: LocaleCode) => void;
  confirmAdult: (confirmed: boolean) => void;
  setConsent: (category: ConsentCategory, granted: boolean) => void;
  setPrimaryGoal: (goal: GoalDomain) => void;
  setWorkPattern: (pattern: WorkSchedulePattern) => void;
  setIndoorPreferred: (indoor: boolean) => void;
  setMaxDailyActions: (cap: number) => void;
  setBaselineFitnessLevel: (level: BaselineFitnessLevel) => void;
  toggleAccessibilityTag: (tag: string) => void;
  setFirstPlanPreview: (preview: FirstPlanPreviewResponse | null) => void;
  resetOnboarding: () => void;
}

const initialConsents: Record<ConsentCategory, boolean> = {
  CoreProfile: true, // Required for app operation
  HealthConnectSync: false,
  AssistantChatContext: false,
  AnonymousResearch: false,
};

export const useOnboardingStore = create<OnboardingState>((set) => ({
  locale: 'en',
  isAdultConfirmed: false,
  consents: { ...initialConsents },
  primaryGoalDomain: 'Movement',
  workPattern: 'StandardDaytime',
  indoorPreferred: true,
  maxDailyActions: 1,
  quietHoursStart: '22:00:00',
  quietHoursEnd: '07:00:00',
  baselineFitnessLevel: 'Starter',
  accessibilityTags: [],
  firstPlanPreview: null,

  setLocale: (locale) => set({ locale }),
  confirmAdult: (confirmed) => set({ isAdultConfirmed: confirmed }),
  setConsent: (category, granted) =>
    set((state) => ({
      consents: {
        ...state.consents,
        [category]: granted,
      },
    })),
  setPrimaryGoal: (primaryGoalDomain) => set({ primaryGoalDomain }),
  setWorkPattern: (workPattern) => set({ workPattern }),
  setIndoorPreferred: (indoorPreferred) => set({ indoorPreferred }),
  setMaxDailyActions: (cap) => set({ maxDailyActions: Math.min(3, Math.max(1, cap)) }),
  setBaselineFitnessLevel: (baselineFitnessLevel) => set({ baselineFitnessLevel }),
  toggleAccessibilityTag: (tag) =>
    set((state) => {
      const exists = state.accessibilityTags.includes(tag);
      return {
        accessibilityTags: exists
          ? state.accessibilityTags.filter((t) => t !== tag)
          : [...state.accessibilityTags, tag],
      };
    }),
  setFirstPlanPreview: (firstPlanPreview) => set({ firstPlanPreview }),
  resetOnboarding: () =>
    set({
      locale: 'en',
      isAdultConfirmed: false,
      consents: { ...initialConsents },
      primaryGoalDomain: 'Movement',
      workPattern: 'StandardDaytime',
      indoorPreferred: true,
      maxDailyActions: 1,
      baselineFitnessLevel: 'Starter',
      accessibilityTags: [],
      firstPlanPreview: null,
    }),
}));
