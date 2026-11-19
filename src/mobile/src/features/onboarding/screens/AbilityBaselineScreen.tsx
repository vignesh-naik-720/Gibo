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
import { BaselineFitnessLevel } from '../contracts/onboarding.types';

interface AbilityBaselineScreenProps {
  onNext: () => void;
  onBack: () => void;
}

export const AbilityBaselineScreen: React.FC<AbilityBaselineScreenProps> = ({
  onNext,
  onBack,
}) => {
  const { t } = useTranslation();
  const baseline = useOnboardingStore((state) => state.baselineFitnessLevel);
  const setBaseline = useOnboardingStore((state) => state.setBaselineFitnessLevel);
  const accessibilityTags = useOnboardingStore((state) => state.accessibilityTags);
  const toggleAccessibilityTag = useOnboardingStore(
    (state) => state.toggleAccessibilityTag
  );

  const baselineTiers: Array<{
    key: BaselineFitnessLevel;
    title: string;
    description: string;
  }> = [
    {
      key: 'Starter',
      title: 'Starter',
      description: t('baseline.starter'),
    },
    {
      key: 'Intermediate',
      title: 'Standard',
      description: t('baseline.intermediate'),
    },
    {
      key: 'Active',
      title: 'Active',
      description: t('baseline.active'),
    },
  ];

  const mobilityFilters = [
    { tag: 'LowImpact', label: 'Joint Sensitivity (Knees/Hips)' },
    { tag: 'SeatedOnly', label: 'Seated Movement Preferred' },
    { tag: 'NoBending', label: 'Lower Back / Spine Consideration' },
  ];

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <ScrollView contentContainerStyle={styles.scrollContent}>
          <TouchableOpacity onPress={onBack} style={styles.backButton}>
            <Text style={styles.backButtonText}>← {t('common.back')}</Text>
          </TouchableOpacity>

          <Text style={styles.title}>{t('baseline.title')}</Text>
          <Text style={styles.subtitle}>{t('baseline.subtitle')}</Text>

          {/* Baseline Tiers */}
          <View style={styles.tierContainer}>
            {baselineTiers.map((tier) => {
              const isSelected = baseline === tier.key;
              return (
                <TouchableOpacity
                  key={tier.key}
                  style={[
                    styles.tierCard,
                    isSelected && styles.tierCardSelected,
                  ]}
                  onPress={() => setBaseline(tier.key)}
                >
                  <Text
                    style={[
                      styles.tierTitle,
                      isSelected && styles.tierTitleSelected,
                    ]}
                  >
                    {tier.title}
                  </Text>
                  <Text style={styles.tierDescription}>{tier.description}</Text>
                </TouchableOpacity>
              );
            })}
          </View>

          {/* Physical Mobility Tags */}
          <Text style={styles.sectionHeader}>
            {t('baseline.restrictionsHeader')}
          </Text>
          <Text style={styles.restrictionsNotice}>
            {t('baseline.restrictionsNotice')}
          </Text>

          <View style={styles.tagGrid}>
            {mobilityFilters.map((item) => {
              const isChecked = accessibilityTags.includes(item.tag);
              return (
                <TouchableOpacity
                  key={item.tag}
                  style={[
                    styles.tagChip,
                    isChecked && styles.tagChipChecked,
                  ]}
                  onPress={() => toggleAccessibilityTag(item.tag)}
                >
                  <Text
                    style={[
                      styles.tagChipText,
                      isChecked && styles.tagChipTextChecked,
                    ]}
                  >
                    {isChecked ? '✓ ' : '+ '}
                    {item.label}
                  </Text>
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
    marginBottom: 20,
  },
  tierContainer: {
    gap: 10,
    marginBottom: 20,
  },
  tierCard: {
    backgroundColor: '#FFFFFF',
    padding: 16,
    borderRadius: 12,
    borderWidth: 1.5,
    borderColor: '#E5E7EB',
  },
  tierCardSelected: {
    borderColor: '#0F766E',
    backgroundColor: '#F0FDFA',
  },
  tierTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: '#1F2937',
    marginBottom: 4,
  },
  tierTitleSelected: {
    color: '#0F766E',
  },
  tierDescription: {
    fontSize: 13,
    color: '#6B7280',
    lineHeight: 18,
  },
  sectionHeader: {
    fontSize: 15,
    fontWeight: '700',
    color: '#1F2937',
    marginBottom: 6,
  },
  restrictionsNotice: {
    fontSize: 12,
    color: '#4B5563',
    lineHeight: 18,
    marginBottom: 12,
    backgroundColor: '#FEF3C7',
    padding: 10,
    borderRadius: 8,
  },
  tagGrid: {
    gap: 8,
  },
  tagChip: {
    paddingVertical: 12,
    paddingHorizontal: 14,
    borderRadius: 10,
    backgroundColor: '#FFFFFF',
    borderWidth: 1,
    borderColor: '#E5E7EB',
  },
  tagChipChecked: {
    borderColor: '#0F766E',
    backgroundColor: '#CCFBF1',
  },
  tagChipText: {
    fontSize: 13,
    fontWeight: '600',
    color: '#374151',
  },
  tagChipTextChecked: {
    color: '#0F766E',
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
