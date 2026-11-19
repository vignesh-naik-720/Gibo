/**
 * Action Card Component
 * Path: src/mobile/src/features/dailyPlan/components/ActionCard.tsx
 */

import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, I18nManager } from 'react-native';
import { TaskInstanceDto, EvidenceSource } from '../contracts/dailyPlan.types';

interface ActionCardProps {
  task: TaskInstanceDto;
  onComplete: (taskId: string, source: EvidenceSource) => void;
  onSwapPress?: (taskId: string) => void;
  onAdjustTargetPress?: (taskId: string) => void;
}

export const ActionCard: React.FC<ActionCardProps> = ({
  task,
  onComplete,
  onSwapPress,
  onAdjustTargetPress,
}) => {
  const [showRationale, setShowRationale] = useState(false);
  const isRTL = I18nManager.isRTL;
  const isCompleted = task.state === 'Completed';

  const getDomainIcon = () => {
    switch (task.domain) {
      case 'Movement':
        return '🚶';
      case 'SleepRoutine':
        return '🌙';
      case 'Nutrition':
        return '🥗';
      default:
        return '✨';
    }
  };

  return (
    <View style={[styles.card, isCompleted && styles.completedCard]}>
      {/* Top Header Row */}
      <View style={[styles.headerRow, isRTL && styles.rtlRow]}>
        <View style={styles.domainBadge}>
          <Text style={styles.domainIcon}>{getDomainIcon()}</Text>
          <Text style={styles.domainText}>{task.domain}</Text>
        </View>
        <View style={styles.minutesBadge}>
          <Text style={styles.minutesText}>⏱️ {task.estimatedMinutes}m</Text>
        </View>
      </View>

      {/* Title & Verb */}
      <Text style={[styles.title, isRTL && styles.rtlText]}>{task.title}</Text>

      {/* Target & Unit */}
      <View style={[styles.targetRow, isRTL && styles.rtlRow]}>
        <Text style={styles.targetLabel}>Target:</Text>
        <Text style={styles.targetValue}>
          {task.targetValue} {task.targetUnit}
        </Text>
      </View>

      {/* "Why this fits you" Rationale Accordion */}
      <TouchableOpacity
        style={styles.rationaleToggle}
        onPress={() => setShowRationale(!showRationale)}
        activeOpacity={0.7}
      >
        <Text style={styles.rationaleToggleText}>
          {showRationale ? '▼ Hide Rationale' : '▶ Why Gibo recommended this'}
        </Text>
      </TouchableOpacity>

      {showRationale && (
        <View style={styles.rationaleBox}>
          <Text style={[styles.rationaleText, isRTL && styles.rtlText]}>
            {task.rationale}
          </Text>
        </View>
      )}

      {/* Action Buttons */}
      <View style={styles.buttonContainer}>
        {isCompleted ? (
          <View style={styles.completedBadge}>
            <Text style={styles.completedText}>✓ Completed (+10 XP)</Text>
          </View>
        ) : (
          <TouchableOpacity
            style={styles.completeButton}
            onPress={() => onComplete(task.taskInstanceId, 'Manual')}
            activeOpacity={0.8}
          >
            <Text style={styles.completeButtonText}>✓ Mark Complete (+10 XP)</Text>
          </TouchableOpacity>
        )}

        {!isCompleted && (
          <View style={[styles.utilityRow, isRTL && styles.rtlRow]}>
            {onSwapPress && (
              <TouchableOpacity
                style={styles.utilityButton}
                onPress={() => onSwapPress(task.taskInstanceId)}
              >
                <Text style={styles.utilityButtonText}>🔄 Swap</Text>
              </TouchableOpacity>
            )}
            {onAdjustTargetPress && (
              <TouchableOpacity
                style={styles.utilityButton}
                onPress={() => onAdjustTargetPress(task.taskInstanceId)}
              >
                <Text style={styles.utilityButtonText}>📉 Reduce</Text>
              </TouchableOpacity>
            )}
          </View>
        )}
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  card: {
    backgroundColor: '#1E293B',
    borderRadius: 20,
    borderWidth: 1.5,
    borderColor: '#334155',
    padding: 20,
    marginBottom: 16,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.2,
    shadowRadius: 8,
    elevation: 4,
  },
  completedCard: {
    borderColor: '#10B981',
    backgroundColor: '#0F2922',
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  rtlRow: {
    flexDirection: 'row-reverse',
  },
  rtlText: {
    textAlign: 'right',
  },
  domainBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(6, 182, 212, 0.15)',
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
    gap: 6,
  },
  domainIcon: {
    fontSize: 14,
  },
  domainText: {
    color: '#06B6D4',
    fontSize: 12,
    fontWeight: '700',
  },
  minutesBadge: {
    backgroundColor: '#334155',
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 8,
  },
  minutesText: {
    color: '#94A3B8',
    fontSize: 12,
    fontWeight: '600',
  },
  title: {
    color: '#F8FAFC',
    fontSize: 18,
    fontWeight: '700',
    lineHeight: 24,
    marginBottom: 8,
  },
  targetRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    marginBottom: 12,
  },
  targetLabel: {
    color: '#94A3B8',
    fontSize: 14,
  },
  targetValue: {
    color: '#F8FAFC',
    fontSize: 15,
    fontWeight: '700',
  },
  rationaleToggle: {
    paddingVertical: 4,
    marginBottom: 8,
  },
  rationaleToggleText: {
    color: '#06B6D4',
    fontSize: 13,
    fontWeight: '600',
  },
  rationaleBox: {
    backgroundColor: 'rgba(15, 23, 42, 0.6)',
    padding: 12,
    borderRadius: 10,
    marginBottom: 14,
    borderLeftWidth: 3,
    borderLeftColor: '#06B6D4',
  },
  rationaleText: {
    color: '#CBD5E1',
    fontSize: 13,
    lineHeight: 18,
  },
  buttonContainer: {
    marginTop: 4,
  },
  completeButton: {
    backgroundColor: '#06B6D4',
    paddingVertical: 14,
    borderRadius: 14,
    alignItems: 'center',
  },
  completeButtonText: {
    color: '#0F172A',
    fontSize: 15,
    fontWeight: '700',
  },
  completedBadge: {
    backgroundColor: 'rgba(16, 185, 129, 0.2)',
    borderWidth: 1,
    borderColor: '#10B981',
    paddingVertical: 12,
    borderRadius: 14,
    alignItems: 'center',
  },
  completedText: {
    color: '#10B981',
    fontSize: 15,
    fontWeight: '700',
  },
  utilityRow: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    gap: 12,
    marginTop: 10,
  },
  utilityButton: {
    paddingVertical: 6,
    paddingHorizontal: 12,
    borderRadius: 8,
    backgroundColor: '#334155',
  },
  utilityButtonText: {
    color: '#CBD5E1',
    fontSize: 12,
    fontWeight: '600',
  },
});
