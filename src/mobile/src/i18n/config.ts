/**
 * Gibo Internationalization (i18n) Configuration
 * Supports English ('en'), Modern Standard Arabic ('ar') with full RTL layout mirroring, and Hindi ('hi').
 */

import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import { I18nManager } from 'react-native';

import en from './locales/en.json';
import ar from './locales/ar.json';
import hi from './locales/hi.json';

export const SUPPORTED_LANGUAGES = [
  { code: 'en', name: 'English', nativeName: 'English', isRTL: false },
  { code: 'ar', name: 'Arabic', nativeName: 'العربية', isRTL: true },
  { code: 'hi', name: 'Hindi', nativeName: 'हिन्दी', isRTL: false },
] as const;

export type SupportedLanguageCode = (typeof SUPPORTED_LANGUAGES)[number]['code'];

export const resources = {
  en: { translation: en },
  ar: { translation: ar },
  hi: { translation: hi },
};

i18n.use(initReactI18next).init({
  resources,
  lng: 'en',
  fallbackLng: 'en',
  interpolation: {
    escapeValue: false, // React already safeguards against XSS
  },
  compatibilityJSON: 'v4',
});

/**
 * Changes language dynamically and toggles RTL layout for Arabic.
 * In React Native, if RTL state changes, I18nManager forces the layout direction.
 */
export const changeLanguage = async (languageCode: SupportedLanguageCode): Promise<void> => {
  const isRTL = languageCode === 'ar';
  
  // If RTL direction differs from current layout, force update
  if (I18nManager.isRTL !== isRTL) {
    I18nManager.allowRTL(isRTL);
    I18nManager.forceRTL(isRTL);
  }

  await i18n.changeLanguage(languageCode);
};

export default i18n;
