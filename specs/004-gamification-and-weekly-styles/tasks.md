# Tasks: Purposeful Gamification & Weekly Gibo Styles

**Input**: Design documents from `specs/004-gamification-and-weekly-styles/`  
**Prerequisites**: [plan.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/004-gamification-and-weekly-styles/plan.md), [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/004-gamification-and-weekly-styles/spec.md), [research.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/004-gamification-and-weekly-styles/research.md), [data-model.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/004-gamification-and-weekly-styles/data-model.md), [contracts/](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/004-gamification-and-weekly-styles/contracts/)  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Code Concepts & Architectural Notes for Implementers

> [!NOTE]
> **Developer Guidance**: All tasks below MUST adhere to clean code principles:
> 1. **Immutable XP Ledger**:
>    XP is an append-only ledger of `XpTransaction` records. Never decrement XP or update mutable counters. Invariant: `Amount > 0` always.
> 2. **Non-Clinical Descriptive Language**:
>    Weekly styles celebrate constructive patterns (`ComebackChampion`, `SteadyBuilder`, etc.). Never output diagnostic, judgmental, or shaming terminology.
> 3. **Data Provenance Separation**:
>    Monthly charts must clearly differentiate sensor records from manual check-offs, using observational language without health efficacy overclaims.

---

## Phase 1: Setup (Contracts & Domain Enums)

- [ ] T001 Define gamification domain enums (`XpReason`, `WeeklyStyleType`) in `src/backend/Gibo.Domain/Aggregates/GamificationAggregate/GamificationEnums.cs`
- [ ] T002 [P] Copy TypeScript contracts into mobile codebase in `src/mobile/src/features/gamification/contracts/gamification.types.ts`
- [ ] T003 [P] Add localized strings for weekly styles, encouragement copy, and provenance legends in `src/mobile/src/i18n/locales/`

---

## Phase 2: Foundational (Entities & Classifier)

**⚠️ CRITICAL**: Must be completed before user story implementation begins.

- [ ] T004 Implement `XpTransaction` entity with positive-amount invariant guard in `src/backend/Gibo.Domain/Aggregates/GamificationAggregate/XpTransaction.cs`
- [ ] T005 Implement `WeeklyInsight` entity and `MonthlyMetricAggregate` in `src/backend/Gibo.Domain/Aggregates/GamificationAggregate/WeeklyInsight.cs`
- [ ] T006 [P] Configure EF Core mappings for gamification tables in `src/backend/Gibo.Infrastructure/Persistence/Configurations/GamificationConfigurations.cs`
- [ ] T007 [P] Register gamification sets in `src/backend/Gibo.Infrastructure/Persistence/GiboDbContext.cs`
- [ ] T008 Implement `IWeeklyStyleClassifier` and 7-day pattern evaluation engine in `src/backend/Gibo.Application/Features/Gamification/Services/WeeklyStyleClassifier.cs`
- [ ] T009 [P] Write unit tests validating positive-only XP invariants and style classification in `tests/backend/Gibo.Domain.UnitTests/GamificationTests.cs`

---

## Phase 3: User Story 1 - Earning XP for Effort & Honest Reflection (Priority: P1) 🎯 MVP

**Goal**: Record immutable positive XP awards for task completions, partial milestones, and barrier reflections.

- [ ] T010 [P] [US1] Implement MediatR notification handler listening for `TaskCompletedEvent` to award +10 XP in `src/backend/Gibo.Application/Features/Gamification/EventHandlers/TaskCompletedEventHandler.cs`
- [ ] T011 [P] [US1] Implement `GetXpBalanceQuery` calculating total balance and recent ledger in `src/backend/Gibo.Application/Features/Gamification/Queries/GetXpBalance/GetXpBalanceQueryHandler.cs`
- [ ] T012 [US1] Create `GamificationController` exposing `GET /api/v1/gamification/balance` in `src/backend/Gibo.Api/Controllers/v1/GamificationController.cs`
- [ ] T013 [P] [US1] Build mobile `XpCounterBadge.tsx` component with earned XP animation toast in `src/mobile/src/features/gamification/components/XpCounterBadge.tsx`
- [ ] T014 [US1] Add gamification master toggle in mobile settings allowing users to hide XP elements

---

## Phase 4: User Story 2 - Viewing "Your Gibo Style This Week" (Priority: P2)

**Goal**: Display an encouraging, non-clinical weekly style badge with an option to hide it from the dashboard.

- [ ] T015 [P] [US2] Implement `GetWeeklyStyleQuery` in `src/backend/Gibo.Application/Features/Gamification/Queries/GetWeeklyStyle/GetWeeklyStyleQueryHandler.cs`
- [ ] T016 [US2] Implement `ToggleWeeklyStyleVisibilityCommand` in `src/backend/Gibo.Application/Features/Gamification/Commands/ToggleWeeklyStyleVisibility/ToggleWeeklyStyleVisibilityCommandHandler.cs`
- [ ] T017 [US2] Expose `GET /api/v1/gamification/weekly-style` and `PUT /api/v1/gamification/weekly-style/toggle-visibility` in `src/backend/Gibo.Api/Controllers/v1/GamificationController.cs`
- [ ] T018 [P] [US2] Build `WeeklyStyleCard.tsx` component celebrating the user's style in `src/mobile/src/features/gamification/components/WeeklyStyleCard.tsx`
- [ ] T019 [US2] Build `WeeklyRecapScreen.tsx` displaying weekly statistics and style explanation in `src/mobile/src/features/gamification/screens/WeeklyRecapScreen.tsx`

---

## Phase 5: User Story 3 - Monthly Trend Insights with Data Provenance (Priority: P2)

**Goal**: Render monthly trend visualizations clearly distinguishing sensor-measured data from self-reported actions.

- [ ] T020 [P] [US3] Implement `GetMonthlyTrendsQuery` aggregating monthly counts by source in `src/backend/Gibo.Application/Features/Gamification/Queries/GetMonthlyTrends/GetMonthlyTrendsQueryHandler.cs`
- [ ] T021 [US3] Expose `GET /api/v1/gamification/monthly-trends` in `src/backend/Gibo.Api/Controllers/v1/GamificationController.cs`
- [ ] T022 [P] [US3] Build `ProvenanceTrendChart.tsx` plotting sensor steps and manual check-offs as distinct series in `src/mobile/src/features/gamification/components/ProvenanceTrendChart.tsx`

---

## Phase 6: Polish & Verification

- [ ] T023 [P] Write integration tests for XP ledger, weekly style toggle, and monthly trends in `tests/backend/Gibo.Api.IntegrationTests/GamificationEndpointsTests.cs`
- [ ] T024 Execute `dotnet test` and confirm 100% pass rate
- [ ] T025 Audit Arabic RTL layout mirroring for weekly style cards and trend charts
- [ ] T026 Execute and verify all 3 scenarios from `quickstart.md`

---

## Dependency & Execution Order

```text
Phase 1: Setup (T001-T003)
       │
       ▼
Phase 2: Foundational (T004-T009) ⚠️ BLOCKING
       │
       ▼
Phase 3: User Story 1 (XP Ledger & Reflection) [P1 - MVP] (T010-T014)
       │
       ▼
Phase 4: User Story 2 (Weekly Style Recap) [P2] (T015-T019)
       │
       ▼
Phase 5: User Story 3 (Monthly Trends & Provenance) [P2] (T020-T022)
       │
       ▼
Phase 6: Polish & Verification (T023-T026)
```
