# Implementation Plan: Grounded AI Wellness Assistant & Clinical Safety Guardrails

**Branch**: `005-grounded-ai-assistant` | **Date**: 2026-11-18 | **Spec**: [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/005-grounded-ai-assistant/spec.md)

**Input**: Feature specification from `specs/005-grounded-ai-assistant/spec.md`  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Summary

Implement a bounded, evidence-based conversational wellness assistant governed by a 7-stage defensive RAG pipeline. Restricts retrieval exclusively to a curated, clinician-vetted knowledge base (WHO, Abu Dhabi DoH, ADPHC, Gibo Clinical Team). Incorporates deterministic pre-check safety classification (intercepting acute emergencies, diagnostic inquiries, medication changes, and self-harm to render localized UAE emergency contacts 998/800-50), mandatory safe abstention when evidence is absent, and post-check citation verification with privacy-safe telemetry.

---

## Technical Context

**Language/Version**: C# 13 on .NET 10 LTS & TypeScript 5.6+ on React Native.  
**Primary Dependencies**: `Microsoft.Extensions.AI`, pgvector / in-memory semantic vector index, FluentValidation.  
**Storage**: PostgreSQL tables `KnowledgeSources`, `KnowledgeChunks`, `ChatAudits`.  
**Testing**: Unit safety classifier benchmarks (200 adversarial queries), API integration tests.  
**Constraints**: 
- Absolute zero unconstrained clinical triage or drug advice.
- Raw chat messages redacted from persistent audit logs.

---

## Constitution Check

| Principle | Check | Status | Verification Detail |
| :--- | :--- | :---: | :--- |
| **Principle I: User Agency** | Users can clear chat context or opt out of conversational assistant entirely. | **PASS** | Controlled via Consent Ledger (`AssistantChatContext`). |
| **Principle III: Evidence Before AI** | 7-stage pipeline; curated knowledge base whitelist; deterministic abstention on low confidence; emergency bypass. | **PASS** | `GroundedAssistantPipeline` enforces hard safety pre-checks. |
| **Principle IV: Privacy & UAE PDPL** | Raw chat text is NOT persisted in audit tables; prompt sanitization strips PII before LLM dispatch. | **PASS** | `InputSecuritySanitizer` redacts phone numbers, emails, and full names. |
| **Principle V: Multilingual Parity** | Trilingual disclaimers and guidance in English, Arabic (RTL), and Hindi. | **PASS** | Verified in `assistant.types.ts` and i18n locales. |

---

## Project Structure

### Documentation
```text
specs/005-grounded-ai-assistant/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── assistant.openapi.json
│   └── assistant.types.ts
├── tasks.md
└── checklists/
    └── requirements.md
```

### Source Code
```text
src/
├── backend/
│   ├── Gibo.Domain/Aggregates/AssistantAggregate/
│   │   ├── KnowledgeSource.cs
│   │   ├── KnowledgeChunk.cs
│   │   ├── ChatAudit.cs
│   │   └── AssistantEnums.cs
│   ├── Gibo.Application/Features/Assistant/
│   │   ├── Commands/SubmitAssistantQuery/
│   │   ├── Queries/GetKnowledgeSources/
│   │   └── Services/
│   │       ├── GroundedAssistantPipeline.cs
│   │       ├── SafetyClassifier.cs
│   │       └── CitationVerifier.cs
│   ├── Gibo.Infrastructure/Persistence/Configurations/AssistantConfigurations.cs
│   └── Gibo.Api/Controllers/v1/AssistantController.cs
└── mobile/src/features/assistant/
    ├── components/
    │   ├── ChatMessageBubble.tsx
    │   ├── CitationCard.tsx
    │   └── EmergencyHelplineCard.tsx
    ├── screens/
    │   └── WellnessChatScreen.tsx
    ├── services/assistantApiService.ts
    └── state/useAssistantStore.ts
```
