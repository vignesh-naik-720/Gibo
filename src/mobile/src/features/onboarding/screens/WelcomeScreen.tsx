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
import { changeLanguage, SUPPORTED_LANGUAGES, SupportedLanguageCode } from '../../../i18n/config';
import { useOnboardingStore } from '../state/useOnboardingStore';

interface WelcomeScreenProps {
  onNext: () => void;
}

export const WelcomeScreen: React.FC<WelcomeScreenProps> = ({ onNext }) => {
  const { t, i18n } = useTranslation();
  const setStoreLocale = useOnboardingStore((state) => state.setLocale);

  const handleLanguageChange = async (code: SupportedLanguageCode) => {
    await changeLanguage(code);
    setStoreLocale(code);
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.container}>
        {/* Brand Header */}
        <View style={styles.brandContainer}>
          <Text style={styles.logoText}>GIBO</Text>
          <Text style={styles.taglineText}>{t('common.tagline')}</Text>
        </View>

        {/* Welcome Narrative */}
        <View style={styles.card}>
          <Text style={styles.welcomeTitle}>{t('welcome.title')}</Text>
          <Text style={styles.welcomeSubtitle}>{t('welcome.subtitle')}</Text>

          {/* Legal / Clinical Disclaimer Banner (Mandatory Principle III) */}
          <View style={styles.disclaimerBox}>
            <Text style={styles.disclaimerText}>
              🛡️ {t('welcome.disclaimer')}
            </Text>
          </View>
        </View>

        {/* Language Selection Grid */}
        <View style={styles.languageSection}>
          <Text style={styles.sectionLabel}>{t('welcome.selectLanguage')}</Text>
          <View style={styles.languageRow}>
            {SUPPORTED_LANGUAGES.map((lang) => {
              const isSelected = i18n.language === lang.code;
              return (
                <TouchableOpacity
                  key={lang.code}
                  accessibilityRole="button"
                  accessibilityLabel={`Select language ${lang.nativeName}`}
                  accessibilityState={{ selected: isSelected }}
                  style={[
                    styles.langButton,
                    isSelected && styles.langButtonSelected,
                  ]}
                  onPress={() => handleLanguageChange(lang.code)}
                >
                  <Text
                    style={[
                      styles.langButtonText,
                      isSelected && styles.langButtonTextSelected,
                    ]}
                  >
                    {lang.nativeName}
                  </Text>
                  <Text style={styles.langSubtext}>{lang.name}</Text>
                </TouchableOpacity>
              );
            })}
          </View>
        </View>

        {/* Proceed Action Button */}
        <TouchableOpacity
          accessibilityRole="button"
          accessibilityLabel={t('common.continue')}
          style={styles.primaryButton}
          onPress={onNext}
        >
          <Text style={styles.primaryButtonText}>{t('common.continue')}</Text>
        </TouchableOpacity>
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#F8FAF9',
  },
  container: {
    padding: 24,
    flexGrow: 1,
    justifyContent: 'space-between',
  },
  brandContainer: {
    alignItems: 'center',
    marginVertical: 16,
  },
  logoText: {
    fontSize: 40,
    fontWeight: '900',
    color: '#0F766E', // Gibo wellness teal
    letterSpacing: 2,
  },
  taglineText: {
    fontSize: 14,
    color: '#4B5563',
    textAlign: 'center',
    marginTop: 6,
    lineHeight: 20,
  },
  card: {
    backgroundColor: '#FFFFFF',
    borderRadius: 16,
    padding: 20,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.05,
    shadowRadius: 8,
    elevation: 2,
    marginVertical: 16,
  },
  welcomeTitle: {
    fontSize: 22,
    fontWeight: '700',
    color: '#111827',
    marginBottom: 8,
  },
  welcomeSubtitle: {
    fontSize: 15,
    color: '#4B5563',
    lineHeight: 22,
  },
  disclaimerBox: {
    backgroundColor: '#F0FDFA',
    borderLeftWidth: 4,
    borderLeftColor: '#0D9488',
    padding: 12,
    borderRadius: 8,
    marginTop: 16,
  },
  disclaimerText: {
    fontSize: 13,
    color: '#134E4A',
    lineHeight: 18,
  },
  languageSection: {
    marginVertical: 16,
  },
  sectionLabel: {
    fontSize: 15,
    fontWeight: '600',
    color: '#374151',
    marginBottom: 12,
  },
  languageRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    gap: 8,
  },
  langButton: {
    flex: 1,
    paddingVertical: 14,
    paddingHorizontal: 8,
    borderRadius: 12,
    backgroundColor: '#FFFFFF',
    borderWidth: 1.5,
    borderColor: '#E5E7EB',
    alignItems: 'center',
  },
  langButtonSelected: {
    borderColor: '#0F766E',
    backgroundColor: '#CCFBF1',
  },
  langButtonText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#1F2937',
  },
  langButtonTextSelected: {
    color: '#0F766E',
    fontWeight: '700',
  },
  langSubtext: {
    fontSize: 11,
    color: '#6B7280',
    marginTop: 2,
  },
  primaryButton: {
    backgroundColor: '#0F766E',
    paddingVertical: 16,
    borderRadius: 12,
    alignItems: 'center',
    marginTop: 24,
    minHeight: 48,
  },
  primaryButtonText: {
    color: '#FFFFFF',
    fontSize: 16,
    fontWeight: '700',
  },
});
