/**
 * TypeScript Contracts: Grounded AI Wellness Assistant
 * Path: specs/005-grounded-ai-assistant/contracts/assistant.types.ts
 */

export type SafetyCategory =
  | 'SafeWellness'
  | 'EmergencyRedirect'
  | 'DiagnosticBoundary'
  | 'DisorderedEating'
  | 'PromptInjection';

export type ChatOutcome =
  | 'AnsweredWithCitations'
  | 'AbstainedLowConfidence'
  | 'EmergencyHalt';

export interface CitationDto {
  sourceId: string;
  title: string;
  organization: string;
  publicationYear: number;
  url: string;
}

export interface EmergencyContactsDto {
  ambulanceNumber: string;
  policeNumber: string;
  telehealthNumber: string;
  mentalHealthNumber: string;
  message: string;
}

export interface AssistantQueryRequestDto {
  query: string;
  conversationId?: string | null;
  locale?: 'en' | 'ar' | 'hi';
}

export interface AssistantResponseDto {
  answer: string;
  outcome: ChatOutcome;
  safetyClassification: SafetyCategory;
  citations: CitationDto[];
  emergencyGuidance?: EmergencyContactsDto | null;
}

export interface KnowledgeSourceDto {
  sourceId: string;
  title: string;
  organization: string;
  publicationDate: string;
  version: string;
  allowedTopicTags: string[];
}
