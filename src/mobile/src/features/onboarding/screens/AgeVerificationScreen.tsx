import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Modal,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useOnboardingStore } from '../state/useOnboardingStore';

interface AgeVerificationScreenProps {
  onNext: () => void;
  onBack: () => void;
}

export const AgeVerificationScreen: React.FC<AgeVerificationScreenProps> = ({
  onNext,
  onBack,
}) => {
  const { t } = useTranslation();
  const confirmAdult = useOnboardingStore((state) => state.confirmAdult);
  const [showUnderageModal, setShowUnderageModal] = useState(false);

  const handleAdultSelection = () => {
    confirmAdult(true);
    onNext();
  };

  const handleUnderageSelection = () => {
    confirmAdult(false);
    setShowUnderageModal(true);
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <View>
          <TouchableOpacity onPress={onBack} style={styles.backButton}>
            <Text style={styles.backButtonText}>← {t('common.back')}</Text>
          </TouchableOpacity>

          <Text style={styles.title}>{t('eligibility.title')}</Text>
          <Text style={styles.subtitle}>{t('eligibility.question')}</Text>

          <View style={styles.buttonStack}>
            <TouchableOpacity
              style={styles.choiceButtonAdult}
              onPress={handleAdultSelection}
              accessibilityRole="button"
              accessibilityLabel={t('eligibility.yesAdult')}
            >
              <Text style={styles.choiceButtonAdultText}>
                ✓ {t('eligibility.yesAdult')}
              </Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.choiceButtonUnderage}
              onPress={handleUnderageSelection}
              accessibilityRole="button"
              accessibilityLabel={t('eligibility.noUnderage')}
            >
              <Text style={styles.choiceButtonUnderageText}>
                {t('eligibility.noUnderage')}
              </Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* Modal when user is underage */}
        <Modal
          visible={showUnderageModal}
          transparent
          animationType="fade"
          onRequestClose={() => setShowUnderageModal(false)}
        >
          <View style={styles.modalOverlay}>
            <View style={styles.modalContent}>
              <Text style={styles.modalHeader}>Notice</Text>
              <Text style={styles.modalBody}>
                {t('eligibility.underageMessage')}
              </Text>
              <TouchableOpacity
                style={styles.modalCloseButton}
                onPress={() => setShowUnderageModal(false)}
              >
                <Text style={styles.modalCloseButtonText}>Understood</Text>
              </TouchableOpacity>
            </View>
          </View>
        </Modal>
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
  backButton: {
    paddingVertical: 8,
    marginBottom: 16,
  },
  backButtonText: {
    color: '#0F766E',
    fontSize: 16,
    fontWeight: '600',
  },
  title: {
    fontSize: 26,
    fontWeight: '800',
    color: '#111827',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    color: '#4B5563',
    lineHeight: 24,
    marginBottom: 32,
  },
  buttonStack: {
    gap: 16,
  },
  choiceButtonAdult: {
    backgroundColor: '#0F766E',
    paddingVertical: 18,
    paddingHorizontal: 16,
    borderRadius: 14,
    alignItems: 'center',
    shadowColor: '#0F766E',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.2,
    shadowRadius: 6,
    elevation: 3,
  },
  choiceButtonAdultText: {
    color: '#FFFFFF',
    fontSize: 16,
    fontWeight: '700',
  },
  choiceButtonUnderage: {
    backgroundColor: '#FFFFFF',
    borderWidth: 1.5,
    borderColor: '#D1D5DB',
    paddingVertical: 18,
    paddingHorizontal: 16,
    borderRadius: 14,
    alignItems: 'center',
  },
  choiceButtonUnderageText: {
    color: '#6B7280',
    fontSize: 16,
    fontWeight: '600',
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
  },
  modalContent: {
    backgroundColor: '#FFFFFF',
    borderRadius: 16,
    padding: 24,
    width: '100%',
    maxWidth: 400,
  },
  modalHeader: {
    fontSize: 20,
    fontWeight: '700',
    color: '#111827',
    marginBottom: 12,
  },
  modalBody: {
    fontSize: 15,
    color: '#4B5563',
    lineHeight: 22,
    marginBottom: 20,
  },
  modalCloseButton: {
    backgroundColor: '#0F766E',
    paddingVertical: 12,
    borderRadius: 8,
    alignItems: 'center',
  },
  modalCloseButtonText: {
    color: '#FFFFFF',
    fontSize: 15,
    fontWeight: '600',
  },
});
