/**
 * Task Swap Sheet Component
 * Path: src/mobile/src/features/dailyPlan/components/TaskSwapSheet.tsx
 * Allows users to choose an approved domain alternative when an action does not fit their schedule.
 */

import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  Modal,
  TouchableOpacity,
  ScrollView,
  I18nManager,
} from 'react-native';
import { TaskInstanceDto } from '../contracts/dailyPlan.types';

export interface SwapAlternative {
  templateId: string;
  titleEn: string;
  titleAr: string;
  actionVerb: string;
  estimatedMinutes: number;
  targetValue: number;
  targetUnit: string;
  rationaleEn: string;
  rationaleAr: string;
}

// Curated domain alternative templates for Abu Dhabi lifestyle
const DOMAIN_ALTERNATIVES: Record<string, SwapAlternative[]> = {
  Movement: [
    {
      templateId: '10000000-0000-0000-0000-000000000002',
      titleEn: '10-Minute Evening Corniche Stroll',
      titleAr: 'مشي مسائي لمدة 10 دقائق على الكورنيش',
      actionVerb: 'Walk',
      estimatedMinutes: 10,
      targetValue: 10,
      targetUnit: 'mins',
      rationaleEn: 'Enjoy the cooler evening sea breeze along the promenade.',
      rationaleAr: 'استمتع بنسيم البحر العليل في المساء على الممشى.',
    },
    {
      templateId: '10000000-0000-0000-0000-000000000003',
      titleEn: '5-Minute Gentle Living Room Stretch',
      titleAr: 'تمارين إطالة لطيفة لمدة 5 دقائق في غرفة المعيشة',
      actionVerb: 'Stretch',
      estimatedMinutes: 5,
      targetValue: 5,
      targetUnit: 'mins',
      rationaleEn: 'Relieves joint stiffness indoors with zero heat exposure.',
      rationaleAr: 'يخفف من تيبس المفاصل داخل المنزل دون التعرض للحرارة.',
    },
    {
      templateId: '10000000-0000-0000-0000-000000000004',
      titleEn: 'Take the Stairs (2 Flights)',
      titleAr: 'استخدم الدرج (طابقان)',
      actionVerb: 'Climb',
      estimatedMinutes: 3,
      targetValue: 2,
      targetUnit: 'flights',
      rationaleEn: 'Quick cardiovascular burst easily integrated into your workday.',
      rationaleAr: 'تنشيط سريع للقلب والأوعية الدموية يسهل دمجه في يوم عملك.',
    },
  ],
  SleepRoutine: [
    {
      templateId: '20000000-0000-0000-0000-000000000002',
      titleEn: 'Screens Off 20 Minutes Before Bed',
      titleAr: 'إغلاق الشاشات قبل النوم بـ 20 دقيقة',
      actionVerb: 'Disconnect',
      estimatedMinutes: 20,
      targetValue: 20,
      targetUnit: 'mins',
      rationaleEn: 'Cuts blue light stimulation to allow natural melatonin production.',
      rationaleAr: 'يقلل من تأثير الضوء الأزرق لإفراز هرمون الميلاتونين بشكل طبيعي.',
    },
    {
      templateId: '20000000-0000-0000-0000-000000000003',
      titleEn: 'Cool Bedroom AC to 21°C Before Sleep',
      titleAr: 'ضبط مكيف غرفة النوم على 21 درجة مئوية',
      actionVerb: 'Cool',
      estimatedMinutes: 1,
      targetValue: 21,
      targetUnit: '°C',
      rationaleEn: 'Optimizes room temperature for deeper REM sleep cycles.',
      rationaleAr: 'يوفر درجة حرارة مثالية لنوم عميق ومريح.',
    },
  ],
  Nutrition: [
    {
      templateId: '30000000-0000-0000-0000-000000000002',
      titleEn: 'Add Fresh Mint or Cucumber to Water Bottle',
      titleAr: 'إضافة النعناع الطازج أو الخيار إلى قارورة الماء',
      actionVerb: 'Infuse',
      estimatedMinutes: 2,
      targetValue: 1,
      targetUnit: 'bottle',
      rationaleEn: 'Makes hydrating refreshing and habitual in warm weather.',
      rationaleAr: 'يجعل شرب الماء منعشاً وممتعاً في الطقس الحار.',
    },
    {
      templateId: '30000000-0000-0000-0000-000000000003',
      titleEn: 'Choose Fresh Fruit Instead of Sugary Karak',
      titleAr: 'اختيار فاكهة طازجة كبديل للشاي المحلى',
      actionVerb: 'Snack',
      estimatedMinutes: 3,
      targetValue: 1,
      targetUnit: 'portion',
      rationaleEn: 'Provides sustained natural glucose without an afternoon energy crash.',
      rationaleAr: 'يمنح طاقة طبيعية مستمرة دون هبوط مفاجئ في النشاط.',
    },
  ],
};

interface TaskSwapSheetProps {
  visible: boolean;
  task: TaskInstanceDto | null;
  locale?: string;
  onClose: () => void;
  onSelectAlternative: (alternative: SwapAlternative) => void;
}

export const TaskSwapSheet: React.FC<TaskSwapSheetProps> = ({
  visible,
  task,
  locale = 'en',
  onClose,
  onSelectAlternative,
}) => {
  if (!task) return null;

  const isRTL = I18nManager.isRTL || locale === 'ar';
  const alternatives = DOMAIN_ALTERNATIVES[task.domain] || [];

  return (
    <Modal visible={visible} transparent animationType="slide" onRequestClose={onClose}>
      <View style={styles.overlay}>
        <View style={styles.sheetContainer}>
          {/* Header */}
          <View style={[styles.header, isRTL && styles.rtlRow]}>
            <View>
              <Text style={[styles.sheetTitle, isRTL && styles.rtlText]}>
                {locale === 'ar' ? 'تبديل النشاط' : 'Swap Activity'}
              </Text>
              <Text style={[styles.sheetSubtitle, isRTL && styles.rtlText]}>
                {locale === 'ar'
                  ? `اختر بديلاً مناسباً لمجال ${task.domain}`
                  : `Choose a better fit for ${task.domain}`}
              </Text>
            </View>
            <TouchableOpacity onPress={onClose} style={styles.closeButton}>
              <Text style={styles.closeText}>✕</Text>
            </TouchableOpacity>
          </View>

          {/* Current Task Reminder */}
          <View style={styles.currentTaskCard}>
            <Text style={styles.currentTaskLabel}>
              {locale === 'ar' ? 'النشاط الحالي:' : 'Current Action:'}
            </Text>
            <Text style={[styles.currentTaskTitle, isRTL && styles.rtlText]}>
              {task.title}
            </Text>
          </View>

          {/* Alternatives List */}
          <ScrollView style={styles.scrollList} contentContainerStyle={styles.scrollContent}>
            {alternatives.map((alt) => (
              <TouchableOpacity
                key={alt.templateId}
                style={styles.alternativeCard}
                activeOpacity={0.7}
                onPress={() => onSelectAlternative(alt)}
              >
                <View style={[styles.altHeader, isRTL && styles.rtlRow]}>
                  <Text style={styles.altVerb}>{alt.actionVerb}</Text>
                  <Text style={styles.altDuration}>⏱️ {alt.estimatedMinutes}m</Text>
                </View>
                <Text style={[styles.altTitle, isRTL && styles.rtlText]}>
                  {locale === 'ar' ? alt.titleAr : alt.titleEn}
                </Text>
                <Text style={[styles.altRationale, isRTL && styles.rtlText]}>
                  {locale === 'ar' ? alt.rationaleAr : alt.rationaleEn}
                </Text>
                <View style={styles.chooseButton}>
                  <Text style={styles.chooseButtonText}>
                    {locale === 'ar' ? 'اختر هذا البديل' : 'Select This Alternative'}
                  </Text>
                </View>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.65)',
    justifyContent: 'flex-end',
  },
  sheetContainer: {
    backgroundColor: '#0F172A',
    borderTopLeftRadius: 24,
    borderTopRightRadius: 24,
    padding: 20,
    maxHeight: '80%',
    borderWidth: 1,
    borderColor: '#334155',
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  rtlRow: {
    flexDirection: 'row-reverse',
  },
  sheetTitle: {
    fontSize: 20,
    fontWeight: '700',
    color: '#F8FAFC',
  },
  sheetSubtitle: {
    fontSize: 13,
    color: '#94A3B8',
    marginTop: 2,
  },
  rtlText: {
    textAlign: 'right',
  },
  closeButton: {
    padding: 8,
  },
  closeText: {
    fontSize: 20,
    color: '#94A3B8',
  },
  currentTaskCard: {
    backgroundColor: '#1E293B',
    borderRadius: 12,
    padding: 12,
    marginBottom: 16,
    borderLeftWidth: 4,
    borderLeftColor: '#38BDF8',
  },
  currentTaskLabel: {
    fontSize: 11,
    color: '#38BDF8',
    fontWeight: '600',
    textTransform: 'uppercase',
    marginBottom: 4,
  },
  currentTaskTitle: {
    fontSize: 14,
    color: '#E2E8F0',
    fontWeight: '500',
  },
  scrollList: {
    marginBottom: 10,
  },
  scrollContent: {
    paddingBottom: 20,
  },
  alternativeCard: {
    backgroundColor: '#1E293B',
    borderRadius: 16,
    borderWidth: 1,
    borderColor: '#334155',
    padding: 16,
    marginBottom: 12,
  },
  altHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 6,
  },
  altVerb: {
    fontSize: 12,
    fontWeight: '700',
    color: '#38BDF8',
    textTransform: 'uppercase',
  },
  altDuration: {
    fontSize: 12,
    color: '#CBD5E1',
  },
  altTitle: {
    fontSize: 15,
    fontWeight: '600',
    color: '#F8FAFC',
    marginBottom: 6,
  },
  altRationale: {
    fontSize: 12,
    color: '#94A3B8',
    lineHeight: 18,
    marginBottom: 12,
  },
  chooseButton: {
    backgroundColor: '#0284C7',
    borderRadius: 10,
    paddingVertical: 8,
    alignItems: 'center',
  },
  chooseButtonText: {
    color: '#FFFFFF',
    fontSize: 13,
    fontWeight: '600',
  },
});
