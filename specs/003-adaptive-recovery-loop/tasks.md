# Tasks: Adaptive Recovery Loop & Disruption Handling

**Input**: Design documents from `specs/003-adaptive-recovery-loop/`  
**Prerequisites**: [plan.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/003-adaptive-recovery-loop/plan.md), [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/003-adaptive-recovery-loop/spec.md), [research.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/003-adaptive-recovery-loop/research.md), [data-model.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/003-adaptive-recovery-loop/data-model.md), [contracts/](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/003-adaptive-recovery-loop/contracts/)  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Code Concepts & Architectural Notes for Implementers

> [!NOTE]
> **Developer Guidance**: All tasks below MUST adhere to clean code principles:
> 1. **Non-Punitive Recovery (Principle II)**:
>    A missed task is a feasibility diagnostic, never a personal failure. NEVER reset streaks, deduct XP, or emit guilt microcopy.
> 2. **Reflection Reward**:
>    Submitting a barrier check-in awards **exactly 2 XP** to positively reinforce self-awareness and continued engagement.
> 3. **Clinical Safety Intercept (Principle III)**:
>    If `UnwellOrPain` is selected or red-flag clinical symptoms are detected, instantly halt active coaching tasks (`State = Paused`) and display Abu Dhabi emergency contacts (998, 800 50).
> 4. **Note Encryption (Principle IV)**:
>    Free-text barrier notes MUST be encrypted via `AesEncryptionProvider` before persistence to safeguard user privacy under UAE PDPL.

---

## Phase 1: Setup (Contracts & Domain Enums)

- [ ] T001 Define recovery domain enums (`BarrierCategory`, `AdaptationType`, `AdaptationStatus`) in `src/backend/Gibo.Domain/Aggregates/RecoveryAggregate/RecoveryEnums.cs`
- [ ] T002 [P] Copy TypeScript contracts into mobile codebase in `src/mobile/src/features/recovery/contracts/recovery.types.ts`
- [ ] T003 [P] Add recovery localized strings (English, Arabic RTL, Hindi) for barrier categories and empathetic check-in prompts in `src/mobile/src/i18n/locales/`

---

## Phase 2: Foundational (Entities & Adaptation Engine)

**⚠️ CRITICAL**: Must be completed before user story implementation begins.

- [ ] T004 Implement `BarrierFeedback` entity with 2 XP reward invariant and encrypted note support in `src/backend/Gibo.Domain/Aggregates/RecoveryAggregate/BarrierFeedback.cs`
- [ ] T005 Implement `PlanAdaptation` entity with acceptance state transitions in `src/backend/Gibo.Domain/Aggregates/RecoveryAggregate/PlanAdaptation.cs`
- [ ] T006 Implement `SafetyEscalationEvent` entity for regulatory safety halt audit logging in `src/backend/Gibo.Domain/Aggregates/RecoveryAggregate/SafetyEscalationEvent.cs`
- [ ] T007 [P] Configure EF Core entity mappings for recovery tables in `src/backend/Gibo.Infrastructure/Persistence/Configurations/RecoveryConfigurations.cs`
- [ ] T008 [P] Register recovery entity sets in `src/backend/Gibo.Infrastructure/Persistence/GiboDbContext.cs`
- [ ] T009 Implement `RecoveryAdaptationService` and rules-first `AdaptationDecisionMatrix` in `src/backend/Gibo.Application/Features/Recovery/Services/RecoveryAdaptationService.cs`
- [ ] T010 [P] Write unit tests validating decision matrix mapping and safety halts in `tests/backend/Gibo.Domain.UnitTests/RecoveryAdaptationTests.cs`

---

## Phase 3: User Story 1 - Gentle Barrier Check-in After a Missed Action (Priority: P1) 🎯 MVP

**Goal**: Present an empathetic, guilt-free friction check-in when a scheduled action is missed, awarding +2 XP for reflection.

- [ ] T011 [P] [US1] Implement `SubmitBarrierCommand` and MediatR handler in `src/backend/Gibo.Application/Features/Recovery/Commands/SubmitBarrier/SubmitBarrierCommandHandler.cs`
- [ ] T012 [US1] Create `RecoveryController` exposing `POST /api/v1/recovery/barriers` with idempotency support in `src/backend/Gibo.Api/Controllers/v1/RecoveryController.cs`
- [ ] T013 [P] [US1] Implement recovery API client `recoveryApiService.ts` in `src/mobile/src/features/recovery/services/recoveryApiService.ts`
- [ ] T014 [P] [US1] Implement Zustand store `useRecoveryStore.ts` for managing barrier prompts and reflection XP in `src/mobile/src/features/recovery/state/useRecoveryStore.ts`
- [ ] T015 [US1] Build mobile `BarrierCheckInModal.tsx` presenting 7 friction categories with non-judgmental microcopy in `src/mobile/src/features/recovery/components/BarrierCheckInModal.tsx`
- [ ] T016 [US1] Integrate barrier check-in trigger into `TodayPlanScreen.tsx` when yesterday's tasks remain incomplete

---

## Phase 4: User Story 2 - Dynamic Plan Adaptation & Acceptance (Priority: P1)

**Goal**: Automatically propose targeted plan modifications (smaller targets, indoor alternatives, time shifts) that users can accept with one tap.

- [ ] T017 [P] [US2] Implement `RespondAdaptationCommand` updating today's active plan upon acceptance in `src/backend/Gibo.Application/Features/Recovery/Commands/RespondAdaptation/RespondAdaptationCommandHandler.cs`
- [ ] T018 [US2] Expose `POST /api/v1/recovery/adaptations/{adaptationId}/respond` in `src/backend/Gibo.Api/Controllers/v1/RecoveryController.cs`
- [ ] T019 [US2] Build `AdaptationProposalCard.tsx` showing the concrete proposed change and "Accept" / "Decline" buttons in `src/mobile/src/features/recovery/components/AdaptationProposalCard.tsx`
- [ ] T020 [US2] Wire adaptation acceptance into `useDailyPlanStore.ts` to seamlessly update active task cards without page reload

---

## Phase 5: User Story 4 - Safety Escalation for Reported Health Concerns (Priority: P1)

**Goal**: Immediately halt coaching and surface Abu Dhabi emergency and teleconsultation resources when health distress is disclosed.

- [ ] T021 [P] [US4] Implement red-flag keyword symptom scanner in `src/backend/Gibo.Application/Features/Recovery/Services/SymptomSafetyScanner.cs`
- [ ] T022 [US4] Add `SafetyEscalationEvent` persistence and coaching pause logic to `SubmitBarrierCommandHandler.cs`
- [ ] T023 [US4] Expose `POST /api/v1/recovery/safety/resolve` for recovery self-attestation in `src/backend/Gibo.Api/Controllers/v1/RecoveryController.cs`
- [ ] T024 [P] [US4] Build `EmergencyGuidanceBanner.tsx` displaying Abu Dhabi 998 and 800-50 call buttons in `src/mobile/src/features/recovery/components/EmergencyGuidanceBanner.tsx`
- [ ] T025 [US4] Write unit tests verifying acute symptom disclosure triggers immediate safety pause in `tests/backend/Gibo.Domain.UnitTests/SafetyEscalationTests.cs`

---

## Phase 6: User Story 3 - Guilt-Free Re-entry After an Extended Gap (Priority: P2)

**Goal**: Provide a warm, lecture-free welcome for users returning after a 7+ day absence, preserving 100% of historical effort.

- [ ] T026 [P] [US3] Implement `GetReEntryStatusQuery` checking user activity interval in `src/backend/Gibo.Application/Features/Recovery/Queries/GetReEntryStatus/GetReEntryStatusQueryHandler.cs`
- [ ] T027 [US3] Expose `GET /api/v1/recovery/reentry` in `src/backend/Gibo.Api/Controllers/v1/RecoveryController.cs`
- [ ] T028 [US3] Build `ReEntryWelcomeScreen.tsx` offering a gentle 1-action restart and displaying preserved XP in `src/mobile/src/features/recovery/screens/ReEntryWelcomeScreen.tsx`
- [ ] T029 [US3] Hook re-entry check into mobile app launch sequence in `src/mobile/src/navigation/RootNavigator.tsx`

---

## Phase 7: Polish & Verification

- [ ] T030 [P] Write integration tests for barrier submission, adaptation acceptance, and emergency halt in `tests/backend/Gibo.Api.IntegrationTests/RecoveryEndpointsTests.cs`
- [ ] T031 Execute backend test suite via `dotnet test` and confirm 100% pass rate
- [ ] T032 Audit Arabic RTL alignment for barrier modal, emergency banner, and re-entry screen
- [ ] T033 Execute and verify all 3 scenarios from `quickstart.md`

---

## Dependency & Execution Order

```text
Phase 1: Setup (T001-T003)
       │
       ▼
Phase 2: Foundational (T004-T010) ⚠️ BLOCKING
       │
       ▼
Phase 3: User Story 1 (Barrier Check-in) [P1 - MVP] (T011-T016)
       │
       ▼
Phase 4: User Story 2 (Plan Adaptation) [P1] (T017-T020)
       │
       ▼
Phase 5: User Story 4 (Safety Escalation) [P1] (T021-T025)
       │
       ▼
Phase 6: User Story 3 (Absence Re-entry) [P2] (T026-T029)
       │
       ▼
Phase 7: Polish & Verification (T030-T033)
```
