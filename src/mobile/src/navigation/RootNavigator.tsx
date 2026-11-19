import React, { useState } from 'react';
import { View, StyleSheet, Text } from 'react-native';
import { WelcomeScreen } from '../features/onboarding/screens/WelcomeScreen';
import { AgeVerificationScreen } from '../features/onboarding/screens/AgeVerificationScreen';
import { ConsentScreen } from '../features/onboarding/screens/ConsentScreen';
import { GoalSelectionScreen } from '../features/onboarding/screens/GoalSelectionScreen';
import { ContextConstraintsScreen } from '../features/onboarding/screens/ContextConstraintsScreen';
import { AbilityBaselineScreen } from '../features/onboarding/screens/AbilityBaselineScreen';
import { FirstPlanPreviewScreen } from '../features/onboarding/screens/FirstPlanPreviewScreen';
import { TodayPlanScreen } from '../features/dailyPlan/screens/TodayPlanScreen';
import { useOnboardingStore } from '../features/onboarding/state/useOnboardingStore';
import { onboardingApi } from '../features/onboarding/services/onboardingApiService';

type OnboardingStep =
  | 'Welcome'
  | 'AgeVerification'
  | 'Consent'
  | 'GoalSelection'
  | 'ContextConstraints'
  | 'AbilityBaseline'
  | 'FirstPlanPreview'
  | 'Dashboard';

export const RootNavigator: React.FC = () => {
  const [currentStep, setCurrentStep] = useState<OnboardingStep>('Welcome');
  const store = useOnboardingStore();

  const handleCompleteOnboarding = async () => {
    try {
      const consentPayload = Object.entries(store.consents).map(([category, granted]) => ({
        category: category as any,
        granted,
        policyVersion: '2026.1',
      }));

      await onboardingApi.registerOnboarding({
        isAdultConfirmed: store.isAdultConfirmed,
        preferredLocale: store.locale,
        timezoneId: 'Asia/Dubai',
        primaryGoalDomain: store.primaryGoalDomain,
        workPattern: store.workPattern,
        indoorPreferred: store.indoorPreferred,
        maxDailyActions: store.maxDailyActions,
        baselineFitnessLevel: store.baselineFitnessLevel,
        accessibilityTags: store.accessibilityTags,
        consents: consentPayload,
      });

      setCurrentStep('Dashboard');
    } catch (err: any) {
      console.warn('API sync warning: running in offline/demo mode:', err.message);
      // Seamless offline fallback per Principle VI
      setCurrentStep('Dashboard');
    }
  };

  switch (currentStep) {
    case 'Welcome':
      return <WelcomeScreen onNext={() => setCurrentStep('AgeVerification')} />;
    case 'AgeVerification':
      return (
        <AgeVerificationScreen
          onNext={() => setCurrentStep('Consent')}
          onBack={() => setCurrentStep('Welcome')}
        />
      );
    case 'Consent':
      return (
        <ConsentScreen
          onNext={() => setCurrentStep('GoalSelection')}
          onBack={() => setCurrentStep('AgeVerification')}
        />
      );
    case 'GoalSelection':
      return (
        <GoalSelectionScreen
          onNext={() => setCurrentStep('ContextConstraints')}
          onBack={() => setCurrentStep('Consent')}
        />
      );
    case 'ContextConstraints':
      return (
        <ContextConstraintsScreen
          onNext={() => setCurrentStep('AbilityBaseline')}
          onBack={() => setCurrentStep('GoalSelection')}
        />
      );
    case 'AbilityBaseline':
      return (
        <AbilityBaselineScreen
          onNext={() => setCurrentStep('FirstPlanPreview')}
          onBack={() => setCurrentStep('ContextConstraints')}
        />
      );
    case 'FirstPlanPreview':
      return (
        <FirstPlanPreviewScreen
          onComplete={handleCompleteOnboarding}
          onBack={() => setCurrentStep('AbilityBaseline')}
        />
      );
    case 'Dashboard':
      return <TodayPlanScreen />;
  }
};

const styles = StyleSheet.create({
  dashboardPlaceholder: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F0FDFA',
    padding: 24,
  },
  dashboardHeader: {
    fontSize: 28,
    fontWeight: '800',
    color: '#0F766E',
    marginBottom: 8,
  },
  dashboardSubtext: {
    fontSize: 16,
    color: '#374151',
    textAlign: 'center',
  },
});
