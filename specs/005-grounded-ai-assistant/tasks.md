# Tasks: Grounded AI Wellness Assistant & Clinical Safety Guardrails

**Input**: Design documents from `specs/005-grounded-ai-assistant/`  
**Prerequisites**: [plan.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/005-grounded-ai-assistant/plan.md), [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/005-grounded-ai-assistant/spec.md), [research.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/005-grounded-ai-assistant/research.md), [data-model.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/005-grounded-ai-assistant/data-model.md), [contracts/](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/005-grounded-ai-assistant/contracts/)  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Code Concepts & Architectural Notes for Implementers

> [!NOTE]
> **Developer Guidance**: All tasks below MUST adhere to clean code principles:
> 1. **7-Stage Defensive RAG Pipeline**:
>    Sanitization $\to$ Pre-Check Classifier $\to$ Curated Retrieval $\to$ Abstention Check $\to$ Generation $\to$ Citation Verification $\to$ Redacted Audit Log.
> 2. **Emergency Bypass (Principle III)**:
>    Never execute LLM generation if an acute symptom or self-harm risk is detected. Immediately return localized UAE emergency numbers (998, 800 50, 800 4673).
> 3. **Mandatory Safe Abstention**:
>    If semantic similarity $<0.78$ or no approved chunk matches, return pre-vetted abstention text without hallucinating.
> 4. **Privacy-Preserving Telemetry (Principle IV)**:
>    Do not store raw query text in `ChatAudit`. Store only classification labels, matched source IDs, and timestamps.

---

## Phase 1: Setup (Contracts & Domain Enums)

- [ ] T001 Define assistant domain enums (`SafetyCategory`, `ChatOutcome`, `OrganizationType`) in `src/backend/Gibo.Domain/Aggregates/AssistantAggregate/AssistantEnums.cs`
- [ ] T002 [P] Copy TypeScript contracts into mobile codebase in `src/mobile/src/features/assistant/contracts/assistant.types.ts`
- [ ] T003 [P] Add assistant disclaimer copy and emergency strings (English, Arabic RTL, Hindi) in `src/mobile/src/i18n/locales/`

---

## Phase 2: Foundational (Entities & Vector Knowledge Base)

**⚠️ CRITICAL**: Must be completed before user story implementation begins.

- [ ] T004 Implement `KnowledgeSource` and `KnowledgeChunk` domain entities in `src/backend/Gibo.Domain/Aggregates/AssistantAggregate/KnowledgeSource.cs`
- [ ] T005 Implement `ChatAudit` entity with privacy-preserving telemetry fields in `src/backend/Gibo.Domain/Aggregates/AssistantAggregate/ChatAudit.cs`
- [ ] T006 [P] Configure EF Core mappings for assistant tables in `src/backend/Gibo.Infrastructure/Persistence/Configurations/AssistantConfigurations.cs`
- [ ] T007 [P] Seed 15 approved clinical and public health reference documents in `src/backend/Gibo.Infrastructure/Persistence/Seeding/KnowledgeBaseSeed.cs`
- [ ] T008 Implement `ISafetyClassifier` detecting emergencies, self-harm, and diagnostic boundaries in `src/backend/Gibo.Application/Features/Assistant/Services/SafetyClassifier.cs`
- [ ] T009 [P] Write unit tests validating safety classification across 200 adversarial queries in `tests/backend/Gibo.Domain.UnitTests/AssistantSafetyClassifierTests.cs`

---

## Phase 3: User Story 2 - Immediate Interception & Redirection for High-Risk Queries (Priority: P1) 🎯 MVP

**Goal**: Guarantee instant emergency redirection and clinical boundary protection for high-risk prompts.

- [ ] T010 [P] [US2] Implement emergency bypass handler returning UAE 998 / 800-50 contact cards in `src/backend/Gibo.Application/Features/Assistant/Services/EmergencyBypassHandler.cs`
- [ ] T011 [US2] Create `AssistantController` exposing `POST /api/v1/assistant/chat` in `src/backend/Gibo.Api/Controllers/v1/AssistantController.cs`
- [ ] T012 [P] [US2] Build `EmergencyHelplineCard.tsx` with native one-tap dialing buttons in `src/mobile/src/features/assistant/components/EmergencyHelplineCard.tsx`
- [ ] T013 [US2] Build mobile `WellnessChatScreen.tsx` container with disclaimers and emergency routing in `src/mobile/src/features/assistant/screens/WellnessChatScreen.tsx`

---

## Phase 4: User Story 1 - Asking General Wellness Questions with Citations (Priority: P1)

**Goal**: Deliver grounded, supportive answers citing approved WHO and Abu Dhabi health sources.

- [ ] T014 [P] [US1] Implement semantic knowledge retriever querying curated vector chunks in `src/backend/Gibo.Application/Features/Assistant/Services/KnowledgeRetriever.cs`
- [ ] T015 [P] [US1] Implement `CitationVerifier` ensuring all health statements link to valid source IDs in `src/backend/Gibo.Application/Features/Assistant/Services/CitationVerifier.cs`
- [ ] T016 [US1] Implement full `GroundedAssistantPipeline` orchestrating stages 1 through 7 in `src/backend/Gibo.Application/Features/Assistant/Services/GroundedAssistantPipeline.cs`
- [ ] T017 [P] [US1] Build `CitationCard.tsx` displaying document title, organization, and publication date in `src/mobile/src/features/assistant/components/CitationCard.tsx`
- [ ] T018 [US1] Build `ChatMessageBubble.tsx` with Arabic RTL formatting and markdown rendering in `src/mobile/src/features/assistant/components/ChatMessageBubble.tsx`

---

## Phase 5: User Story 3 - Safe Abstention on Missing or Low-Confidence Evidence (Priority: P2)

**Goal**: Ensure the assistant honestly abstains when confidence is low or when queries fall outside approved wellness domains.

- [ ] T019 [P] [US3] Implement confidence evaluator and deterministic safe abstention generator in `src/backend/Gibo.Application/Features/Assistant/Services/AbstentionEvaluator.cs`
- [ ] T020 [US3] Implement prompt injection filter stripping malicious jailbreak tags in `src/backend/Gibo.Application/Features/Assistant/Services/InputSecuritySanitizer.cs`
- [ ] T021 [P] [US3] Expose `GET /api/v1/assistant/sources` listing approved evidence whitelist in `src/backend/Gibo.Api/Controllers/v1/AssistantController.cs`

---

## Phase 6: Polish & Verification

- [ ] T022 [P] Write integration tests for grounded generation, emergency halt, and abstention in `tests/backend/Gibo.Api.IntegrationTests/AssistantEndpointsTests.cs`
- [ ] T023 Execute `dotnet test` and confirm 100% pass rate
- [ ] T024 Audit Arabic RTL rendering of chat bubbles, citations, and emergency cards
- [ ] T025 Execute and verify all 3 scenarios from `quickstart.md`

---

## Dependency & Execution Order

```text
Phase 1: Setup (T001-T003)
       │
       ▼
Phase 2: Foundational (T004-T009) ⚠️ BLOCKING
       │
       ▼
Phase 3: User Story 2 (Emergency Interception) [P1 - MVP] (T010-T013)
       │
       ▼
Phase 4: User Story 1 (Grounded Q&A with Citations) [P1] (T014-T018)
       │
       ▼
Phase 5: User Story 3 (Safe Abstention & Jailbreak Defense) [P2] (T019-T021)
       │
       ▼
Phase 6: Polish & Verification (T022-T025)
```
