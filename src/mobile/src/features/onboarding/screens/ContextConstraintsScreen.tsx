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
import { WorkSchedulePattern } from '../contracts/onboarding.types';

interface ContextConstraintsScreenProps {
  onNext: () => void;
  onBack: () => void;
}

export const ContextConstraintsScreen: React.FC<ContextConstraintsScreenProps> = ({
  onNext,
  onBack,
}) => {
  const { t } = useTranslation();
  const workPattern = useOnboardingStore((state) => state.workPattern);
  const setWorkPattern = useOnboardingStore((state) => state.setWorkPattern);
  const indoorPreferred = useOnboardingStore((state) => state.indoorPreferred);
  const setIndoorPreferred = useOnboardingStore((state) => state.setIndoorPreferred);
  const maxDailyActions = useOnboardingStore((state) => state.maxDailyActions);
  const setMaxDailyActions = useOnboardingStore((state) => state.setMaxDailyActions);

  const shiftOptions: Array<{ key: WorkSchedulePattern; label: string }> = [
    { key: 'StandardDaytime', label: t('constraints.standardDay') },
    { key: 'ShiftWorker', label: t('constraints.shiftWork') },
    { key: 'FlexibleOrIrregular', label: t('constraints.flexible') },
  ];

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <ScrollView contentContainerStyle={styles.scrollContent}>
          <TouchableOpacity onPress={onBack} style={styles.backButton}>
            <Text style={styles.backButtonText}>← {t('common.back')}</Text>
          </TouchableOpacity>

          <Text style={styles.title}>{t('constraints.title')}</Text>
          <Text style={styles.subtitle}>{t('constraints.subtitle')}</Text>

          {/* Work Schedule */}
          <Text style={styles.sectionHeader}>{t('constraints.workPattern')}</Text>
          <View style={styles.optionsRow}>
            {shiftOptions.map((opt) => (
              <TouchableOpacity
                key={opt.key}
                style={[
                  styles.optionChip,
                  workPattern === opt.key && styles.optionChipSelected,
                ]}
                onPress={() => setWorkPattern(opt.key)}
              >
                <Text
                  style={[
                    styles.optionChipText,
                    workPattern === opt.key && styles.optionChipTextSelected,
                  ]}
                >
                  {opt.label}
                </Text>
              </TouchableOpacity>
            ))}
          </View>

          {/* Indoor / Climate Preference */}
          <Text style={styles.sectionHeader}>{t('constraints.indoorOutdoor')}</Text>
          <View style={styles.toggleContainer}>
            <TouchableOpacity
              style={[
                styles.preferenceCard,
                indoorPreferred && styles.preferenceCardSelected,
              ]}
              onPress={() => setIndoorPreferred(true)}
            >
              <Text style={styles.prefIcon}>❄️</Text>
              <View style={styles.prefTextContainer}>
                <Text style={styles.prefTitle}>
                  {t('constraints.preferIndoor')}
                </Text>
              </View>
            </TouchableOpacity>

            <TouchableOpacity
              style={[
                styles.preferenceCard,
                !indoorPreferred && styles.preferenceCardSelected,
              ]}
              onPress={() => setIndoorPreferred(false)}
            >
              <Text style={styles.prefIcon}>🌅</Text>
              <View style={styles.prefTextContainer}>
                <Text style={styles.prefTitle}>
                  {t('constraints.outdoorOk')}
                </Text>
              </View>
            </TouchableOpacity>
          </View>

          {/* Daily Action Cap */}
          <Text style={styles.sectionHeader}>{t('constraints.dailyCap')}</Text>
          <View style={styles.capButtonsContainer}>
            {[1, 2, 3].map((cap) => (
              <TouchableOpacity
                key={cap}
                style={[
                  styles.capButton,
                  maxDailyActions === cap && styles.capButtonSelected,
                ]}
                onPress={() => setMaxDailyActions(cap)}
              >
                <Text
                  style={[
                    styles.capButtonNumber,
                    maxDailyActions === cap && styles.capButtonTextSelected,
                  ]}
                >
                  {cap}
                </Text>
                <Text
                  style={[
                    styles.capButtonLabel,
                    maxDailyActions === cap && styles.capButtonTextSelected,
                  ]}
                >
                  {cap === 1 ? 'Action/Day' : 'Actions/Day'}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </ScrollView>

        <View style={styles.footerRow}>
          <TouchableOpacity
            style={styles.skipButton}
            onPress={onNext}
            accessibilityRole="button"
            accessibilityLabel={t('common.skip')}
          >
            <Text style={styles.skipButtonText}>{t('common.skip')}</Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={styles.primaryButton}
            onPress={onNext}
            accessibilityRole="button"
            accessibilityLabel={t('common.continue')}
          >
            <Text style={styles.primaryButtonText}>{t('common.continue')}</Text>
          </TouchableOpacity>
        </View>
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
    marginBottom: 20,
  },
  sectionHeader: {
    fontSize: 15,
    fontWeight: '700',
    color: '#1F2937',
    marginTop: 16,
    marginBottom: 10,
  },
  optionsRow: {
    flexDirection: 'column',
    gap: 8,
  },
  optionChip: {
    paddingVertical: 12,
    paddingHorizontal: 16,
    borderRadius: 10,
    backgroundColor: '#FFFFFF',
    borderWidth: 1.5,
    borderColor: '#E5E7EB',
  },
  optionChipSelected: {
    borderColor: '#0F766E',
    backgroundColor: '#F0FDFA',
  },
  optionChipText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#374151',
  },
  optionChipTextSelected: {
    color: '#0F766E',
  },
  toggleContainer: {
    gap: 10,
  },
  preferenceCard: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 14,
    borderRadius: 12,
    backgroundColor: '#FFFFFF',
    borderWidth: 1.5,
    borderColor: '#E5E7EB',
  },
  preferenceCardSelected: {
    borderColor: '#0F766E',
    backgroundColor: '#F0FDFA',
  },
  prefIcon: {
    fontSize: 22,
    marginRight: 12,
  },
  prefTextContainer: {
    flex: 1,
  },
  prefTitle: {
    fontSize: 14,
    fontWeight: '600',
    color: '#1F2937',
  },
  capButtonsContainer: {
    flexDirection: 'row',
    gap: 10,
  },
  capButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 10,
    backgroundColor: '#FFFFFF',
    borderWidth: 1.5,
    borderColor: '#E5E7EB',
    alignItems: 'center',
  },
  capButtonSelected: {
    borderColor: '#0F766E',
    backgroundColor: '#0F766E',
  },
  capButtonNumber: {
    fontSize: 18,
    fontWeight: '800',
    color: '#1F2937',
  },
  capButtonLabel: {
    fontSize: 11,
    color: '#6B7280',
    marginTop: 2,
  },
  capButtonTextSelected: {
    color: '#FFFFFF',
  },
  footerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  skipButton: {
    paddingVertical: 16,
    paddingHorizontal: 16,
    borderRadius: 12,
  },
  skipButtonText: {
    color: '#6B7280',
    fontSize: 15,
    fontWeight: '600',
  },
  primaryButton: {
    flex: 1,
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
