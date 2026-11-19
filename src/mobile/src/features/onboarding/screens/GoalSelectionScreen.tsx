import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useOnboardingStore } from '../state/useOnboardingStore';
import { GoalDomain } from '../contracts/onboarding.types';

interface GoalSelectionScreenProps {
  onNext: () => void;
  onBack: () => void;
}

export const GoalSelectionScreen: React.FC<GoalSelectionScreenProps> = ({
  onNext,
  onBack,
}) => {
  const { t } = useTranslation();
  const selectedGoal = useOnboardingStore((state) => state.primaryGoalDomain);
  const setPrimaryGoal = useOnboardingStore((state) => state.setPrimaryGoal);

  const goalOptions: Array<{
    key: GoalDomain;
    icon: string;
    name: string;
    desc: string;
  }> = [
    {
      key: 'Movement',
      icon: '🚶',
      name: t('goals.movement.name'),
      desc: t('goals.movement.desc'),
    },
    {
      key: 'SleepRoutine',
      icon: '🌙',
      name: t('goals.sleep.name'),
      desc: t('goals.sleep.desc'),
    },
    {
      key: 'Nutrition',
      icon: '🥗',
      name: t('goals.nutrition.name'),
      desc: t('goals.nutrition.desc'),
    },
  ];

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <ScrollView contentContainerStyle={styles.scrollContent}>
          <TouchableOpacity onPress={onBack} style={styles.backButton}>
            <Text style={styles.backButtonText}>← {t('common.back')}</Text>
          </TouchableOpacity>

          <Text style={styles.title}>{t('goals.title')}</Text>
          <Text style={styles.subtitle}>{t('goals.subtitle')}</Text>

          <View style={styles.cardsContainer}>
            {goalOptions.map((goal) => {
              const isSelected = selectedGoal === goal.key;
              return (
                <TouchableOpacity
                  key={goal.key}
                  style={[
                    styles.goalCard,
                    isSelected && styles.goalCardSelected,
                  ]}
                  onPress={() => setPrimaryGoal(goal.key)}
                  accessibilityRole="button"
                  accessibilityLabel={`Select goal ${goal.name}`}
                  accessibilityState={{ selected: isSelected }}
                >
                  <Text style={styles.goalIcon}>{goal.icon}</Text>
                  <View style={styles.goalTextContainer}>
                    <Text
                      style={[
                        styles.goalName,
                        isSelected && styles.goalNameSelected,
                      ]}
                    >
                      {goal.name}
                    </Text>
                    <Text style={styles.goalDesc}>{goal.desc}</Text>
                  </View>
                  <View
                    style={[
                      styles.radioButton,
                      isSelected && styles.radioButtonSelected,
                    ]}
                  >
                    {isSelected && <View style={styles.radioButtonInner} />}
                  </View>
                </TouchableOpacity>
              );
            })}
          </View>
        </ScrollView>

        <TouchableOpacity
          style={styles.primaryButton}
          onPress={onNext}
          accessibilityRole="button"
          accessibilityLabel={t('common.continue')}
        >
          <Text style={styles.primaryButtonText}>{t('common.continue')}</Text>
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#F8FAF9',
  },
  container: {
    flex: 1,
    padding: 24,
    justifyContent: 'space-between',
  },
  scrollContent: {
    paddingBottom: 24,
  },
  backButton: {
    paddingVertical: 8,
    marginBottom: 8,
  },
  backButtonText: {
    color: '#0F766E',
    fontSize: 16,
    fontWeight: '600',
  },
  title: {
    fontSize: 24,
    fontWeight: '800',
    color: '#111827',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 14,
    color: '#4B5563',
    lineHeight: 20,
    marginBottom: 24,
  },
  cardsContainer: {
    gap: 14,
  },
  goalCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 16,
    padding: 18,
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 2,
    borderColor: '#E5E7EB',
  },
  goalCardSelected: {
    borderColor: '#0F766E',
    backgroundColor: '#F0FDFA',
  },
  goalIcon: {
    fontSize: 28,
    marginRight: 14,
  },
  goalTextContainer: {
    flex: 1,
  },
  goalName: {
    fontSize: 17,
    fontWeight: '700',
    color: '#111827',
    marginBottom: 4,
  },
  goalNameSelected: {
    color: '#0F766E',
  },
  goalDesc: {
    fontSize: 13,
    color: '#6B7280',
    lineHeight: 18,
  },
  radioButton: {
    width: 22,
    height: 22,
    borderRadius: 11,
    borderWidth: 2,
    borderColor: '#D1D5DB',
    alignItems: 'center',
    justifyContent: 'center',
    marginLeft: 8,
  },
  radioButtonSelected: {
    borderColor: '#0F766E',
  },
  radioButtonInner: {
    width: 12,
    height: 12,
    borderRadius: 6,
    backgroundColor: '#0F766E',
  },
  primaryButton: {
    backgroundColor: '#0F766E',
    paddingVertical: 16,
    borderRadius: 12,
    alignItems: 'center',
    minHeight: 48,
  },
  primaryButtonText: {
    color: '#FFFFFF',
    fontSize: 16,
    fontWeight: '700',
  },
});
