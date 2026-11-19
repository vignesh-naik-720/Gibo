/**
 * Today's Plan Screen (Main Dashboard Hub)
 * Path: src/mobile/src/features/dailyPlan/screens/TodayPlanScreen.tsx
 */

import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  ActivityIndicator,
  I18nManager,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useDailyPlanStore } from '../state/useDailyPlanStore';
import { ActionCard } from '../components/ActionCard';
import { TaskSwapSheet, SwapAlternative } from '../components/TaskSwapSheet';
import { EvidenceSource, TaskInstanceDto } from '../contracts/dailyPlan.types';
import { dailyPlanApi } from '../services/dailyPlanApiService';
import { offlineSyncService } from '../services/offlineSyncService';

export const TodayPlanScreen: React.FC = () => {
  const { t, i18n } = useTranslation();
  const isRTL = I18nManager.isRTL;
  const {
    plan,
    isLoading,
    totalXp,
    fetchTodayPlan,
    generateNewPlan,
    completeTaskLocally,
    swapTaskLocally,
    adjustTargetLocally,
  } = useDailyPlanStore();

  const [selectedTaskForSwap, setSelectedTaskForSwap] = useState<TaskInstanceDto | null>(null);

  useEffect(() => {
    fetchTodayPlan();
  }, []);

  const handleComplete = (taskId: string, source: EvidenceSource) => {
    completeTaskLocally(taskId, source);
    // Queue offline-resilient completion with client-generated event UUID
    offlineSyncService.enqueueCompletion(taskId, source);
  };

  const handleSwapPress = (taskId: string) => {
    const task = plan?.tasks.find((t) => t.taskInstanceId === taskId);
    if (task) {
      setSelectedTaskForSwap(task);
    }
  };

  const handleSelectAlternative = (alt: SwapAlternative) => {
    if (!selectedTaskForSwap) return;

    const replacement: TaskInstanceDto = {
      ...selectedTaskForSwap,
      taskTemplateId: alt.templateId,
      actionVerb: alt.actionVerb,
      title: i18n.language === 'ar' ? alt.titleAr : alt.titleEn,
      targetValue: alt.targetValue,
      targetUnit: alt.targetUnit,
      estimatedMinutes: alt.estimatedMinutes,
      rationale: i18n.language === 'ar' ? alt.rationaleAr : alt.rationaleEn,
      state: 'Scheduled',
    };

    swapTaskLocally(selectedTaskForSwap.taskInstanceId, replacement);

    dailyPlanApi.swapTask(selectedTaskForSwap.taskInstanceId, {
      replacementTemplateId: alt.templateId,
      locale: i18n.language,
    }).catch((err) => {
      console.warn('Swapped locally, server update will sync:', err);
    });

    setSelectedTaskForSwap(null);
  };

  const handleAdjustTarget = (taskId: string) => {
    const task = plan?.tasks.find((t) => t.taskInstanceId === taskId);
    if (task) {
      // Step-down target by 50% (feasibility before ambition)
      const newTarget = Math.max(1, Math.floor(task.targetValue / 2));
      adjustTargetLocally(taskId, newTarget);

      dailyPlanApi.adjustTarget(taskId, newTarget).catch((err) => {
        console.warn('Adjusted target locally, server update will sync:', err);
      });
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.container}>
        {/* Top Header */}
        <View style={[styles.header, isRTL && styles.rtlRow]}>
          <View>
            <Text style={styles.dateText}>
              {new Date().toLocaleDateString(i18n.language, {
                weekday: 'long',
                month: 'short',
                day: 'numeric',
              })}
            </Text>
            <Text style={[styles.headerTitle, isRTL && styles.rtlText]}>
              {i18n.language === 'ar' ? 'خطة اليوم' : 'Today’s Plan'}
            </Text>
          </View>
          <View style={styles.xpPill}>
            <Text style={styles.xpText}>⚡ {totalXp} XP</Text>
          </View>
        </View>

        {/* Loading Indicator */}
        {isLoading && (
          <View style={styles.loadingContainer}>
            <ActivityIndicator size="large" color="#06B6D4" />
            <Text style={styles.loadingText}>Curating your habit plan...</Text>
          </View>
        )}

        {/* Action Cards List */}
        {!isLoading && plan?.tasks && plan.tasks.length > 0 ? (
          plan.tasks.map((task) => (
            <ActionCard
              key={task.taskInstanceId}
              task={task}
              onComplete={handleComplete}
              onSwapPress={handleSwapPress}
              onAdjustTargetPress={handleAdjustTarget}
            />
          ))
        ) : (
          !isLoading && (
            <View style={styles.emptyCard}>
              <Text style={styles.emptyIcon}>🌅</Text>
              <Text style={styles.emptyTitle}>Ready for today?</Text>
              <Text style={styles.emptySubtitle}>
                Generate 1 to 3 micro-habits tailored to your schedule and the Abu Dhabi climate.
              </Text>
              <TouchableOpacity
                style={styles.generateButton}
                onPress={() => generateNewPlan(i18n.language)}
              >
                <Text style={styles.generateButtonText}>Generate Today’s Plan 🚀</Text>
              </TouchableOpacity>
            </View>
          )
        )}
      </ScrollView>

      {/* Task Swap Bottom Sheet */}
      <TaskSwapSheet
        visible={selectedTaskForSwap !== null}
        task={selectedTaskForSwap}
        locale={i18n.language}
        onClose={() => setSelectedTaskForSwap(null)}
        onSelectAlternative={handleSelectAlternative}
      />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#0F172A',
  },
  container: {
    padding: 20,
    paddingBottom: 40,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 24,
  },
  rtlRow: {
    flexDirection: 'row-reverse',
  },
  rtlText: {
    textAlign: 'right',
  },
  dateText: {
    color: '#06B6D4',
    fontSize: 13,
    fontWeight: '700',
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  headerTitle: {
    color: '#F8FAFC',
    fontSize: 28,
    fontWeight: '800',
    marginTop: 2,
  },
  xpPill: {
    backgroundColor: 'rgba(6, 182, 212, 0.15)',
    borderWidth: 1,
    borderColor: '#06B6D4',
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 20,
  },
  xpText: {
    color: '#06B6D4',
    fontSize: 14,
    fontWeight: '800',
  },
  loadingContainer: {
    paddingVertical: 60,
    alignItems: 'center',
  },
  loadingText: {
    color: '#94A3B8',
    marginTop: 12,
    fontSize: 14,
  },
  emptyCard: {
    backgroundColor: '#1E293B',
    borderRadius: 24,
    padding: 30,
    alignItems: 'center',
    marginTop: 40,
    borderWidth: 1,
    borderColor: '#334155',
  },
  emptyIcon: {
    fontSize: 48,
    marginBottom: 16,
  },
  emptyTitle: {
    color: '#F8FAFC',
    fontSize: 20,
    fontWeight: '700',
    marginBottom: 8,
  },
  emptySubtitle: {
    color: '#94A3B8',
    fontSize: 14,
    textAlign: 'center',
    lineHeight: 20,
    marginBottom: 20,
  },
  generateButton: {
    backgroundColor: '#06B6D4',
    paddingVertical: 14,
    paddingHorizontal: 24,
    borderRadius: 14,
  },
  generateButtonText: {
    color: '#0F172A',
    fontSize: 15,
    fontWeight: '700',
  },
});
