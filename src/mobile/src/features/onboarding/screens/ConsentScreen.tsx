import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  Switch,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useOnboardingStore } from '../state/useOnboardingStore';
import { ConsentCategory } from '../contracts/onboarding.types';

interface ConsentScreenProps {
  onNext: () => void;
  onBack: () => void;
}

export const ConsentScreen: React.FC<ConsentScreenProps> = ({ onNext, onBack }) => {
  const { t } = useTranslation();
  const consents = useOnboardingStore((state) => state.consents);
  const setConsent = useOnboardingStore((state) => state.setConsent);

  const consentCategories: Array<{
    key: ConsentCategory;
    title: string;
    desc: string;
    required?: boolean;
  }> = [
    {
      key: 'CoreProfile',
      title: t('consent.coreProfile.title'),
      desc: t('consent.coreProfile.desc'),
      required: true,
    },
    {
      key: 'HealthConnectSync',
      title: t('consent.healthConnect.title'),
      desc: t('consent.healthConnect.desc'),
      required: false,
    },
    {
      key: 'AssistantChatContext',
      title: t('consent.chatContext.title'),
      desc: t('consent.chatContext.desc'),
      required: false,
    },
    {
      key: 'AnonymousResearch',
      title: t('consent.research.title'),
      desc: t('consent.research.desc'),
      required: false,
    },
  ];

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <ScrollView contentContainerStyle={styles.scrollContent}>
          <TouchableOpacity onPress={onBack} style={styles.backButton}>
            <Text style={styles.backButtonText}>← {t('common.back')}</Text>
          </TouchableOpacity>

          <Text style={styles.title}>{t('consent.title')}</Text>
          <Text style={styles.subtitle}>{t('consent.subtitle')}</Text>

          <View style={styles.itemsContainer}>
            {consentCategories.map((item) => (
              <View key={item.key} style={styles.consentCard}>
                <View style={styles.textContainer}>
                  <View style={styles.titleRow}>
                    <Text style={styles.cardTitle}>{item.title}</Text>
                    {item.required && (
                      <View style={styles.requiredBadge}>
                        <Text style={styles.requiredText}>Required</Text>
                      </View>
                    )}
                  </View>
                  <Text style={styles.cardDesc}>{item.desc}</Text>
                </View>

                <Switch
                  value={consents[item.key]}
                  onValueChange={(val) => {
                    if (item.required) return; // CoreProfile cannot be disabled
                    setConsent(item.key, val);
                  }}
                  disabled={item.required}
                  trackColor={{ false: '#D1D5DB', true: '#99F6E4' }}
                  thumbColor={consents[item.key] ? '#0F766E' : '#9CA3AF'}
                  accessibilityRole="switch"
                  accessibilityLabel={`${item.title} consent switch`}
                />
              </View>
            ))}
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
  itemsContainer: {
    gap: 12,
  },
  consentCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 14,
    padding: 16,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    borderWidth: 1,
    borderColor: '#E5E7EB',
  },
  textContainer: {
    flex: 1,
    paddingRight: 12,
  },
  titleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginBottom: 4,
  },
  cardTitle: {
    fontSize: 15,
    fontWeight: '700',
    color: '#1F2937',
  },
  requiredBadge: {
    backgroundColor: '#E0F2FE',
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  requiredText: {
    fontSize: 10,
    fontWeight: '600',
    color: '#0369A1',
  },
  cardDesc: {
    fontSize: 13,
    color: '#6B7280',
    lineHeight: 18,
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
