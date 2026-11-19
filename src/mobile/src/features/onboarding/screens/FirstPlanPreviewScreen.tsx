import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useOnboardingStore } from '../state/useOnboardingStore';
import { GoalDomain } from '../contracts/onboarding.types';

interface FirstPlanPreviewScreenProps {
  onComplete: () => Promise<void>;
  onBack: () => void;
}

export const FirstPlanPreviewScreen: React.FC<FirstPlanPreviewScreenProps> = ({
  onComplete,
  onBack,
}) => {
  const { t } = useTranslation();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [currentMinutes, setCurrentMinutes] = useState(7);
  const primaryGoal = useOnboardingStore((state) => state.primaryGoalDomain);
  const indoorPreferred = useOnboardingStore((state) => state.indoorPreferred);

  // Default starter templates based on selected domain
  const defaultTasks: Record<GoalDomain, { verb: string; target: string; instruction: string; why: string }> = {
    Movement: {
      verb: 'Walk',
      target: `${currentMinutes} minutes`,
      instruction: `Take a ${currentMinutes}-minute gentle indoor walk after lunch`,
      why: 'Fits your preference for indoor activity and provides an achievable, gentle starter baseline.',
    },
    SleepRoutine: {
      verb: 'Wind-down',
      target: '10 minutes',
      instruction: 'Start a 10-minute screen-free wind-down cue before bed',
      why: 'Gives your nervous system a predictable signal to prepare for restorative sleep.',
    },
    Nutrition: {
      verb: 'Add',
      target: '1 serving',
      instruction: 'Add one portion of vegetables or fresh greens to dinner',
      why: 'Builds positive meal habits by adding healthy choices rather than restricting food.',
    },
  };

  const defaultTask = defaultTasks[primaryGoal] || defaultTasks.Movement;

  const handleReduceTarget = () => {
    if (currentMinutes > 3) {
      setCurrentMinutes((prev) => Math.max(3, prev - 2));
    }
  };

  const handleFinalSubmit = async () => {
    setIsSubmitting(true);
    try {
      await onComplete();
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <ScrollView contentContainerStyle={styles.scrollContent}>
          <TouchableOpacity onPress={onBack} style={styles.backButton}>
            <Text style={styles.backButtonText}>← {t('common.back')}</Text>
          </TouchableOpacity>

          <Text style={styles.title}>{t('firstPlan.title')}</Text>
          <Text style={styles.subtitle}>{t('firstPlan.subtitle')}</Text>

          {/* Action Card Preview */}
          <View style={styles.taskCard}>
            <View style={styles.cardHeader}>
              <View style={styles.verbBadge}>
                <Text style={styles.verbBadgeText}>{defaultTask.verb}</Text>
              </View>
              <Text style={styles.targetBadge}>{defaultTask.target}</Text>
            </View>

            <Text style={styles.instructionText}>{defaultTask.instruction}</Text>

            <View style={styles.evidenceRow}>
              <Text style={styles.evidenceLabel}>
                {t('firstPlan.evidenceMode')}
              </Text>
            </View>

            {/* Why This Fits Explanation */}
            <View style={styles.whyBox}>
              <Text style={styles.whyHeader}>{t('firstPlan.whyItFits')}:</Text>
              <Text style={styles.whyText}>{defaultTask.why}</Text>
            </View>

            {/* Adjustment Controls (User Agency) */}
            <View style={styles.cardActionsRow}>
              <TouchableOpacity
                style={styles.cardActionButton}
                onPress={handleReduceTarget}
                disabled={currentMinutes <= 3}
              >
                <Text style={styles.cardActionText}>
                  📉 {t('common.reduce')}
                </Text>
              </TouchableOpacity>

              <TouchableOpacity
                style={styles.cardActionButton}
                onPress={() => Alert.alert('Alternate template selected!')}
              >
                <Text style={styles.cardActionText}>
                  🔄 {t('common.swap')}
                </Text>
              </TouchableOpacity>
            </View>
          </View>
        </ScrollView>

        <TouchableOpacity
          style={styles.primaryButton}
          onPress={handleFinalSubmit}
          disabled={isSubmitting}
          accessibilityRole="button"
          accessibilityLabel={t('firstPlan.startJourney')}
        >
          {isSubmitting ? (
            <ActivityIndicator color="#FFFFFF" />
          ) : (
            <Text style={styles.primaryButtonText}>
              🚀 {t('firstPlan.startJourney')}
            </Text>
          )}
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
  taskCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 18,
    padding: 20,
    borderWidth: 2,
    borderColor: '#0F766E',
    shadowColor: '#0F766E',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.1,
    shadowRadius: 10,
    elevation: 4,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  verbBadge: {
    backgroundColor: '#0F766E',
    paddingHorizontal: 12,
    paddingVertical: 4,
    borderRadius: 8,
  },
  verbBadgeText: {
    color: '#FFFFFF',
    fontSize: 13,
    fontWeight: '700',
    textTransform: 'uppercase',
  },
  targetBadge: {
    fontSize: 14,
    fontWeight: '700',
    color: '#0F766E',
  },
  instructionText: {
    fontSize: 18,
    fontWeight: '700',
    color: '#111827',
    lineHeight: 26,
    marginBottom: 12,
  },
  evidenceRow: {
    marginBottom: 14,
  },
  evidenceLabel: {
    fontSize: 12,
    fontWeight: '600',
    color: '#059669',
  },
  whyBox: {
    backgroundColor: '#F0FDFA',
    borderRadius: 12,
    padding: 12,
    marginBottom: 16,
  },
  whyHeader: {
    fontSize: 12,
    fontWeight: '700',
    color: '#0F766E',
    marginBottom: 4,
  },
  whyText: {
    fontSize: 13,
    color: '#134E4A',
    lineHeight: 18,
  },
  cardActionsRow: {
    flexDirection: 'row',
    gap: 10,
  },
  cardActionButton: {
    flex: 1,
    paddingVertical: 10,
    borderRadius: 8,
    backgroundColor: '#F3F4F6',
    alignItems: 'center',
  },
  cardActionText: {
    fontSize: 13,
    fontWeight: '600',
    color: '#374151',
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
