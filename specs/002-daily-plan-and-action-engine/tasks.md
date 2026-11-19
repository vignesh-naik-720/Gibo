# Tasks: Daily Plan Generation & Action Engine

**Input**: Design documents from `specs/002-daily-plan-and-action-engine/`  
**Prerequisites**: [plan.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/002-daily-plan-and-action-engine/plan.md), [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/002-daily-plan-and-action-engine/spec.md), [research.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/002-daily-plan-and-action-engine/research.md), [data-model.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/002-daily-plan-and-action-engine/data-model.md), [contracts/](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/002-daily-plan-and-action-engine/contracts/)  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Code Concepts & Architectural Notes for Implementers

> [!NOTE]
> **Developer Guidance**: All tasks below MUST adhere to clean code principles:
> 1. **Deterministic 5-Factor Ranking (Principle III)**:
>    $$\text{FinalScore} = (S_{\text{Goal}} \times 0.30) + (S_{\text{Feas}} \times 0.25) + (S_{\text{Barr}} \times 0.20) + (S_{\text{Pref}} \times 0.15) + (S_{\text{Nov}} \times 0.10)$$
>    Safety exclusions (contraindications & heat constraint) MUST run as boolean filters before scoring; never as soft penalties.
> 2. **Device Parity & Manual Equality (Principle V)**:
>    `TaskInstance.Complete()` awards **exactly 10 XP** whether completed manually or via background Health Connect step synchronization.
> 3. **Non-Overridable User Adjustments (Principles I & II)**:
>    When a user edits a target or swaps a task, set `DailyPlan.UserEditsApplied = true`. Engine recalculation routines MUST respect manual user adjustments and never overwrite them.
> 4. **Offline Resilience & Idempotency (Principle VI)**:
>    Every completion event created offline MUST store a client-generated UUID (`ClientEventId`). When flushed via `POST /api/v1/tasks/sync-offline`, duplicate events must be recognized idempotently without re-crediting XP.

---

## Phase 1: Setup (Shared Data & Contracts)

**Purpose**: Establish domain enums, catalog seed definitions, and mobile state contracts for Feature 002.

- [x] T001 Define plan domain enums (`GoalDomain`, `MeasurementType`, `EvidenceMode`, `TaskDifficulty`, `TaskState`, `CompletionEvidenceSource`) in `src/backend/Gibo.Domain/Aggregates/PlanAggregate/PlanEnums.cs`
- [x] T002 [P] Create `TaskTemplate` domain entity with localized trilingual instruction dictionaries and safety tag arrays in `src/backend/Gibo.Domain/Aggregates/PlanAggregate/TaskTemplate.cs`
- [x] T003 [P] Create initial seed data containing 30 curated Abu Dhabi action templates across Movement, Sleep Routine, and Nutrition in `src/backend/Gibo.Infrastructure/Persistence/Seeding/TaskCatalogSeed.cs`
- [x] T004 [P] Copy TypeScript contracts into mobile codebase in `src/mobile/src/features/dailyPlan/contracts/dailyPlan.types.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Database schema persistence, aggregate roots, and deterministic ranking engine.

**⚠️ CRITICAL**: Must be completed before user story implementation begins.

- [x] T005 Implement `TaskInstance` entity with state transitions (`Scheduled`, `Completed`, `Partial`, `Skipped`, `Expired`, `Paused`) and XP invariants in `src/backend/Gibo.Domain/Aggregates/PlanAggregate/TaskInstance.cs`
- [x] T006 Implement `DailyPlan` aggregate root enforcing unique `(UserId, PlanDate)` and `MaxDailyActions` cap $[1, 3]$ in `src/backend/Gibo.Domain/Aggregates/PlanAggregate/DailyPlan.cs`
- [x] T007 Implement `CompletionEvent` entity recording audit timestamps, evidence source, and client UUID in `src/backend/Gibo.Domain/Aggregates/PlanAggregate/CompletionEvent.cs`
- [x] T008 [P] Configure EF Core entity configurations for `DailyPlan`, `TaskInstance`, `TaskTemplate`, and `CompletionEvent` in `src/backend/Gibo.Infrastructure/Persistence/Configurations/PlanConfigurations.cs`
- [x] T009 [P] Register Plan entity sets and seed configuration in `src/backend/Gibo.Infrastructure/Persistence/GiboDbContext.cs`
- [x] T010 Implement `ITaskRankingEngine` and deterministic 5-factor scoring algorithm in `src/backend/Gibo.Application/Features/Planning/Services/TaskRankingEngine.cs`
- [x] T011 [P] Write unit tests validating 5-factor scoring weights, safety tag exclusions, and heat filtering in `tests/backend/Gibo.Domain.UnitTests/TaskRankingEngineTests.cs`

---

## Phase 3: User Story 1 - Personalized Daily Action Card View (Priority: P1) 🎯 MVP

**Goal**: Deliver the primary home surface showing 1 to 3 curated micro-habits with clear verbs, targets, duration, and "Why this fits you" rationales.

- [x] T012 [P] [US1] Implement `GenerateDailyPlanCommand` and MediatR handler in `src/backend/Gibo.Application/Features/Planning/Commands/GenerateDailyPlan/GenerateDailyPlanCommandHandler.cs`
- [x] T013 [P] [US1] Implement `GetTodayPlanQuery` and MediatR handler in `src/backend/Gibo.Application/Features/Planning/Queries/GetTodayPlan/GetTodayPlanQueryHandler.cs`
- [x] T014 [US1] Create `PlansController` exposing `GET /api/v1/plans/today` and `POST /api/v1/plans/generate` in `src/backend/Gibo.Api/Controllers/v1/PlansController.cs`
- [x] T015 [P] [US1] Implement API client service `dailyPlanApiService.ts` with `getTodayPlan` and `generatePlan` in `src/mobile/src/features/dailyPlan/services/dailyPlanApiService.ts`
- [x] T016 [P] [US1] Implement Zustand local store `useDailyPlanStore.ts` for managing today's plan state and active cards in `src/mobile/src/features/dailyPlan/state/useDailyPlanStore.ts`
- [x] T017 [US1] Build mobile `ActionCard.tsx` component with action verb, duration badge, localized title, and rationale expander in `src/mobile/src/features/dailyPlan/components/ActionCard.tsx`
- [x] T018 [US1] Build `TodayPlanScreen.tsx` integrating action cards, Arabic RTL mirroring, and empty-state generator in `src/mobile/src/features/dailyPlan/screens/TodayPlanScreen.tsx`
- [x] T019 [US1] Link `TodayPlanScreen` as default authenticated route in `src/mobile/src/navigation/RootNavigator.tsx`

---

## Phase 4: User Story 2 - Dual-Mode Completion: Manual Parity & Health Connect (Priority: P1)

**Goal**: Enable marking actions complete via manual tap or background Health Connect sync, awarding identical 10 XP with duplicate protection.

- [x] T020 [P] [US2] Implement `CompleteTaskCommand`, validator, and MediatR handler enforcing 10 XP award in `src/backend/Gibo.Application/Features/Planning/Commands/CompleteTask/CompleteTaskCommandHandler.cs`
- [x] T021 [US2] Create `TasksController` exposing `POST /api/v1/tasks/{taskId}/complete` in `src/backend/Gibo.Api/Controllers/v1/TasksController.cs`
- [x] T022 [P] [US2] Add completion mutation and optimistic UI update to `useDailyPlanStore.ts` in `src/mobile/src/features/dailyPlan/state/useDailyPlanStore.ts`
- [x] T023 [US2] Add one-tap complete button with animated checkmark and 10 XP toast to `ActionCard.tsx` in `src/mobile/src/features/dailyPlan/components/ActionCard.tsx`
- [x] T024 [P] [US2] Implement Health Connect mock/bridge service `healthConnectSyncService.ts` for step counts in `src/mobile/src/features/dailyPlan/services/healthConnectSyncService.ts`
- [x] T025 [P] [US2] Write unit tests verifying manual tap and Health Connect both award exactly 10 XP in `tests/backend/Gibo.Domain.UnitTests/TaskCompletionTests.cs`

---

## Phase 5: User Story 3 - User-Driven Plan Adjustments: Edit, Swap & Pause (Priority: P2)

**Goal**: Empower users to adjust targets, swap tasks for approved domain alternatives, or pause a domain without penalty.

- [x] T026 [P] [US3] Implement `SwapTaskCommand` replacing a task instance and setting `UserEditsApplied = true` in `src/backend/Gibo.Application/Features/Planning/Commands/SwapTask/SwapTaskCommandHandler.cs`
- [x] T027 [P] [US3] Implement `AdjustTaskTargetCommand` reducing duration/count in `src/backend/Gibo.Application/Features/Planning/Commands/AdjustTaskTarget/AdjustTaskTargetCommandHandler.cs`
- [x] T028 [US3] Expose `POST /api/v1/tasks/{taskId}/swap` and `PUT /api/v1/tasks/{taskId}/target` in `src/backend/Gibo.Api/Controllers/v1/TasksController.cs`
- [x] T029 [P] [US3] Build `TaskSwapSheet.tsx` bottom sheet presenting 2-3 ranked alternatives from the same domain in `src/mobile/src/features/dailyPlan/components/TaskSwapSheet.tsx`
- [x] T030 [US3] Integrate target reduction dialog and swap sheet into `ActionCard.tsx` in `src/mobile/src/features/dailyPlan/components/ActionCard.tsx`

---

## Phase 6: User Story 4 - Offline Resilience & Idempotent Synchronization (Priority: P2)

**Goal**: Guarantee full offline capability, caching plans locally and batch-syncing queued completion events without duplicate XP awards.

- [x] T031 [P] [US4] Implement `SyncOfflineEventsCommand` and batch processor in `src/backend/Gibo.Application/Features/Planning/Commands/SyncOfflineEvents/SyncOfflineEventsCommandHandler.cs`
- [x] T032 [US4] Expose `POST /api/v1/tasks/sync-offline` in `src/backend/Gibo.Api/Controllers/v1/TasksController.cs`
- [x] T033 [P] [US4] Implement persistent offline event queue in `src/mobile/src/features/dailyPlan/services/offlineSyncService.ts`
- [x] T034 [US4] Integrate network connectivity listener (`NetInfo`) to automatically flush queue on reconnect in `src/mobile/src/features/dailyPlan/services/offlineSyncService.ts`

---

## Phase 7: Polish & Verification

**Purpose**: Execute end-to-end integration tests, RTL layout audits, and quickstart scenarios.

- [x] T035 [P] Write integration tests for plan generation, duplicate completion, and offline sync in `tests/backend/Gibo.Api.IntegrationTests/DailyPlanEndpointsTests.cs`
- [x] T036 Execute test suite via `dotnet test` and verify 100% pass rate
- [x] T037 Audit Arabic RTL layout mirroring for Action Cards, duration chips, and swap bottom sheets
- [x] T038 Execute and document all 5 validation scenarios from `quickstart.md`

---

## Dependency & Execution Order

```text
Phase 1: Setup (T001-T004)
       │
       ▼
Phase 2: Foundational (T005-T011) ⚠️ BLOCKING
       │
       ▼
Phase 3: User Story 1 (Action Cards & Plan Generation) [P1 - MVP] (T012-T019)
       │
       ▼
Phase 4: User Story 2 (Dual-Mode Completion & Parity) [P1] (T020-T025)
       │
       ▼
Phase 5: User Story 3 (Edit, Swap & Pause) [P2] (T026-T030)
       │
       ▼
Phase 6: User Story 4 (Offline Queue & Sync) [P2] (T031-T034)
       │
       ▼
Phase 7: Polish & Verification (T035-T038)
```
