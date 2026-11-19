import {
  OnboardingRegistrationRequest,
  OnboardingRegistrationResponse,
  FirstPlanPreviewRequest,
  FirstPlanPreviewResponse,
} from '../contracts/onboarding.types';
import { API_BASE_URL } from '../../../config/api';

/**
 * Client service connecting React Native onboarding to the ASP.NET Core API.
 * Uses client-generated UUID idempotency keys to ensure zero duplicate registrations under intermittent network.
 */
export const onboardingApi = {
  async registerOnboarding(
    payload: OnboardingRegistrationRequest,
    idempotencyKey?: string
  ): Promise<OnboardingRegistrationResponse> {
    const key = idempotencyKey || crypto.randomUUID();

    const response = await fetch(`${API_BASE_URL}/api/v1/onboarding`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Idempotency-Key': key,
      },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || `Registration failed with status ${response.status}`);
    }

    return response.json();
  },

  async getFirstPlanPreview(
    payload: FirstPlanPreviewRequest
  ): Promise<FirstPlanPreviewResponse> {
    const response = await fetch(`${API_BASE_URL}/api/v1/onboarding/first-plan-preview`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      throw new Error(`Failed to load plan preview: ${response.status}`);
    }

    return response.json();
  },
};
