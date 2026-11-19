import Constants from 'expo-constants';
import { Platform } from 'react-native';

/**
 * Resolves the backend base URL dynamically.
 * In Expo Go on a physical phone, hostUri gives the development computer's LAN IP (e.g. 192.168.1.10).
 * In an Android Emulator, 10.0.2.2 connects to host localhost.
 * Otherwise, falls back to the local network IP.
 */
export function getApiBaseUrl(): string {
  try {
    const hostUri = Constants.expoConfig?.hostUri;
    if (hostUri) {
      const hostIp = hostUri.split(':')[0];
      if (hostIp) {
        return `http://${hostIp}:5000`;
      }
    }

    if (Platform.OS === 'android') {
      return 'http://10.0.2.2:5000';
    }
  } catch {
    // If native constants are evaluated before runtime initialization
  }

  return 'http://192.168.1.10:5000';
}

export const API_BASE_URL = getApiBaseUrl();
