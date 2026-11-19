# Tasks: Onboarding & Consent Governance

**Input**: Design documents from `specs/001-onboarding-and-consent/`
**Prerequisites**: [plan.md](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/001-onboarding-and-consent/plan.md), [spec.md](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/001-onboarding-and-consent/spec.md), [research.md](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/001-onboarding-and-consent/research.md), [data-model.md](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/001-onboarding-and-consent/data-model.md), [contracts/](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/001-onboarding-and-consent/contracts/)
**Constitution**: Adheres to [constitution.md](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Code Concepts & Architectural Notes for Implementers

> [!NOTE]
> **Developer Guidance**: All tasks below MUST adhere to clean code principles:
> 1. **Domain Model Invariants**: Never expose public setters on domain entities; mutate state via explicit domain methods that enforce invariants (e.g. `IsAdultConfirmed == true`, `MaxDailyActions` clamped between 1 and 3).
> 2. **UAE Regulatory Guardrails**: Consent must be recorded per category (`CoreProfile`, `HealthConnectSync`, `AssistantChatContext`, `AnonymousResearch`). Pre-checked boxes are forbidden.
> 3. **Defensive Programming**: Validate every request body against strict FluentValidation rules and require the `Idempotency-Key` header on all registration endpoints.
> 4. **Trilingual & RTL**: Mobile screens must use `useTranslation()` from `react-i18next` and dynamically adapt layout directions for Arabic using `I18nManager.isRTL`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffolding backend modular monolith projects, mobile client base, and toolchains.

- [X] T001 Initialize backend solution structure creating `Gibo.Domain`, `Gibo.Application`, `Gibo.Infrastructure`, and `Gibo.Api` projects in `src/backend/Gibo.sln`
- [X] T002 [P] Configure EF Core 10, Npgsql PostgreSQL provider, MediatR, and FluentValidation packages in `src/backend/Gibo.Api/Gibo.Api.csproj`
- [X] T003 [P] Initialize React Native 0.88.x TypeScript mobile application with strict type checking and ESLint rules in `src/mobile/tsconfig.json`
- [X] T004 [P] Configure trilingual `i18next` and `react-i18next` localization with English, Arabic (`ar-AE`), and Hindi (`hi-IN`) resource files in `src/mobile/src/i18n/config.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core database context, security middleware, and cross-cutting infrastructure required by all user stories.

**⚠️ CRITICAL**: Must be completed before user story implementation begins.

- [X] T005 Setup PostgreSQL database context `GiboDbContext` and entity configurations in `src/backend/Gibo.Infrastructure/Persistence/GiboDbContext.cs`
- [X] T006 [P] Implement AES-256-GCM envelope encryption provider for sensitive database fields in `src/backend/Gibo.Infrastructure/Security/AesEncryptionProvider.cs`
- [X] T007 [P] Implement immutable security audit logging service `AuditEventLogger` in `src/backend/Gibo.Infrastructure/Logging/AuditEventLogger.cs`
- [X] T008 [P] Implement `IdempotencyBehavior` MediatR pipeline and HTTP header middleware in `src/backend/Gibo.Application/Common/Behaviors/IdempotencyBehavior.cs`
- [X] T009 [P] Configure global API error handling and RFC 7807 problem details middleware in `src/backend/Gibo.Api/Middleware/ErrorHandlingMiddleware.cs`
- [X] T010 Setup React Navigation native stack container and theme provider in `src/mobile/src/navigation/RootNavigator.tsx`

**Checkpoint**: Foundation ready — database schema configured and core middleware operational.

---

## Phase 3: User Story 1 - Trust-Centered Welcome & Granular Consent (Priority: P1) 🎯 MVP

**Goal**: Deliver a culturally welcoming, trilingual onboarding entry point with adult verification (18+) and granular, legally compliant consent capture under UAE PDPL.

**Independent Test**: Launch the app, switch language to Arabic (verify instant RTL mirroring), confirm age eligibility (18+), review consent categories, toggle `CoreProfile` on and `HealthConnectSync` off, submit registration, and verify database rows in `users` and `consent_grants`.

### Tests for User Story 1
- [X] T011 [P] [US1] Unit test `User` aggregate invariant: rejecting under-18 registration (`IsAdultConfirmed == false` throws `DomainValidationException`) in `tests/backend/Gibo.Domain.UnitTests/UserAggregateTests.cs`
- [X] T012 [P] [US1] Integration contract test for `POST /api/v1/onboarding` verifying consent ledger generation in `tests/backend/Gibo.Api.IntegrationTests/OnboardingEndpointsTests.cs`
- [X] T013 [P] [US1] UI snapshot and RTL layout test for `WelcomeScreen` and `ConsentScreen` in `tests/mobile/features/onboarding/ConsentScreen.test.tsx`

### Implementation for User Story 1
- [X] T014 [P] [US1] Implement `User` aggregate root with fields `is_adult_confirmed` (required, checked true), `preferred_locale` (enum: 'en', 'ar', 'hi'), `timezone_id` (default: 'Asia/Dubai'), and `account_status` in `src/backend/Gibo.Domain/Aggregates/UserAggregate/User.cs`
- [X] T015 [P] [US1] Implement `ConsentGrant` entity with fields `category` (enum: `CoreProfile`, `HealthConnectSync`, `AssistantChatContext`, `AnonymousResearch`), `purpose` (text), `policy_version` (varchar 20), `granted_at` (UTC), and `revoked_at` (nullable UTC) in `src/backend/Gibo.Domain/Aggregates/UserAggregate/ConsentGrant.cs`
- [X] T016 [US1] Implement EF Core entity type configurations for `User` and `ConsentGrant` with table mappings and indexes in `src/backend/Gibo.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- [X] T017 [US1] Implement MediatR command and handler `RegisterOnboardingUserCommand` enforcing atomic user and consent persistence in `src/backend/Gibo.Application/Features/Onboarding/Commands/RegisterOnboardingUser/RegisterOnboardingUserCommandHandler.cs`
- [X] T018 [US1] Implement FluentValidation rules for `RegisterOnboardingUserCommand` (rejecting underage, empty locale, or unbundled consents) in `src/backend/Gibo.Application/Features/Onboarding/Commands/RegisterOnboardingUser/RegisterOnboardingUserValidator.cs`
- [X] T019 [US1] Implement REST controller endpoint `POST /api/v1/onboarding` consuming `Idempotency-Key` in `src/backend/Gibo.Api/Controllers/v1/OnboardingController.cs`
- [X] T020 [P] [US1] Build mobile `WelcomeScreen` with trilingual switcher (EN, AR RTL, HI) and non-medical wellness disclaimer in `src/mobile/src/features/onboarding/screens/WelcomeScreen.tsx`
- [X] T021 [P] [US1] Build mobile `AgeVerificationScreen` with 18+ eligibility gate and underage modal dialog in `src/mobile/src/features/onboarding/screens/AgeVerificationScreen.tsx`
- [X] T022 [US1] Build mobile `ConsentScreen` with independent opt-in switches per consent category in `src/mobile/src/features/onboarding/screens/ConsentScreen.tsx`
- [X] T023 [US1] Implement Zustand onboarding client store `useOnboardingStore` with encrypted local MMKV persistence in `src/mobile/src/features/onboarding/state/useOnboardingStore.ts`

**Checkpoint**: User Story 1 complete — User can complete welcome, age verification, and consent selection with full trilingual RTL support.

---

## Phase 4: User Story 2 - Goal Selection & Feasibility Context Capture (Priority: P1)

**Goal**: Capture the user's primary wellness goal and real-life feasibility constraints (work shifts, indoor heat preferences, budget/equipment limits) with full support for skipping non-essential questions.

**Independent Test**: Complete Story 1, select "Movement" goal, specify "ShiftWorker", set "IndoorPreferred = true", tap "Skip" on equipment details, and verify `user_preferences` and `goals` records in PostgreSQL.

### Tests for User Story 2
- [X] T024 [P] [US2] Unit test `UserPreference` value constraints (`MaxDailyActions` clamped between 1 and 3; defaults to 1 for starter baseline) in `tests/backend/Gibo.Domain.UnitTests/UserPreferenceTests.cs`
- [X] T025 [P] [US2] Integration test validating optional question skip behavior and default context values in `tests/backend/Gibo.Api.IntegrationTests/ContextCaptureTests.cs`

### Implementation for User Story 2
- [X] T026 [P] [US2] Implement `UserPreference` entity with `primary_goal_domain` (enum: `Movement`, `SleepRoutine`, `Nutrition`), `work_pattern` (enum: `StandardDaytime`, `ShiftWorker`, `FlexibleOrIrregular`), `indoor_preferred` (bool, default true), `max_daily_actions` (int between 1 and 3), `quiet_hours_start` (default 22:00), `quiet_hours_end` (default 07:00), and `accessibility_tags` in `src/backend/Gibo.Domain/Aggregates/UserAggregate/UserPreference.cs`
- [X] T027 [P] [US2] Implement `Goal` entity with `domain` (enum: `Movement`, `SleepRoutine`, `Nutrition`), `desired_outcome` (varchar 255), and `status` in `src/backend/Gibo.Domain/Aggregates/UserAggregate/Goal.cs`
- [X] T028 [US2] Implement EF Core mappings for `UserPreference` and `Goal` in `src/backend/Gibo.Infrastructure/Persistence/Configurations/PreferenceConfiguration.cs`
- [X] T029 [US2] Update `RegisterOnboardingUserCommandHandler` to atomically bind preferences and initial goal to the user aggregate in `src/backend/Gibo.Application/Features/Onboarding/Commands/RegisterOnboardingUser/RegisterOnboardingUserCommandHandler.cs`
- [X] T030 [P] [US2] Build mobile `GoalSelectionScreen` presenting Movement, Sleep Routine, and Nutrition (weight loss explicitly excluded) in `src/mobile/src/features/onboarding/screens/GoalSelectionScreen.tsx`
- [X] T031 [P] [US2] Build mobile `ContextConstraintsScreen` capturing work shift, active windows, indoor/outdoor toggle, and "Skip" buttons in `src/mobile/src/features/onboarding/screens/ContextConstraintsScreen.tsx`

**Checkpoint**: User Stories 1 AND 2 functional — User can select goals and context constraints with zero blocking barriers.

---

## Phase 5: User Story 3 - Ability Baseline, Health Restrictions & First Plan Preview (Priority: P2)

**Goal**: Assess self-reported activity comfort, safely suppress high-impact tasks when restrictions are disclosed, and present a single achievable starter micro-task preview with "Why this fits" explanation.

**Independent Test**: Disclose "Joint Discomfort", verify high-impact templates are filtered, receive a 5-minute indoor walking starter task preview, tap "Accept", and verify onboarding completes successfully.

### Tests for User Story 3
- [X] T032 [P] [US3] Unit test starter task filtering algorithm: ensuring contraindicated tags (e.g. `JointImpact`) exclude high-impact templates in `tests/backend/Gibo.Application.UnitTests/FirstPlanCandidateSelectorTests.cs`
- [X] T033 [P] [US3] Contract test for `POST /api/v1/onboarding/first-plan-preview` in `tests/backend/Gibo.Api.IntegrationTests/FirstPlanPreviewTests.cs`

### Implementation for User Story 3
- [X] T034 [P] [US3] Implement `FirstPlanCandidateSelector` service selecting starter micro-tasks (< 10 minutes) matching indoor and baseline filters in `src/backend/Gibo.Application/Features/Onboarding/Services/FirstPlanCandidateSelector.cs`
- [X] T035 [US3] Implement query handler `GetFirstPlanPreviewQueryHandler` returning localized task verbs, targets, and "Why this fits" rationale in `src/backend/Gibo.Application/Features/Onboarding/Queries/GetFirstPlanPreview/GetFirstPlanPreviewQueryHandler.cs`
- [X] T036 [US3] Implement REST endpoint `POST /api/v1/onboarding/first-plan-preview` in `src/backend/Gibo.Api/Controllers/v1/OnboardingController.cs`
- [X] T037 [P] [US3] Build mobile `AbilityBaselineScreen` capturing fitness comfort and disclosing health boundaries with physician consultation advisory in `src/mobile/src/features/onboarding/screens/AbilityBaselineScreen.tsx`
- [X] T038 [US3] Build mobile `FirstPlanPreviewScreen` displaying the single starter task card with action verb, duration, evidence mode, "Why this fits" explanation, and Accept/Swap buttons in `src/mobile/src/features/onboarding/screens/FirstPlanPreviewScreen.tsx`
- [X] T039 [US3] Wire API client submission and navigation transition from `FirstPlanPreviewScreen` to main dashboard in `src/mobile/src/features/onboarding/services/onboardingApiService.ts`

**Checkpoint**: All onboarding user stories functional — Complete flow from welcome through consent, goal selection, baseline, and plan preview works seamlessly.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Accessibility verification, RTL layout testing, security hardening, and end-to-end quickstart execution.

- [X] T040 [P] Conduct automated accessibility audit (TalkBack labels, minimum 48x48 dp touch targets, WCAG 2.1 AA contrast) on all onboarding screens in `tests/mobile/features/onboarding/AccessibilityAudit.test.tsx`
- [X] T041 [P] Verify Right-to-Left (RTL) Arabic bidirectional text isolation for numerals and measurement units in `src/mobile/src/i18n/locales/ar.json`
- [X] T042 [P] Implement rate limiting (max 10 requests/min per IP) on onboarding endpoints using Redis in `src/backend/Gibo.Api/Middleware/RateLimitingMiddleware.cs`
- [X] T043 Add database index migration script for `users` and `consent_grants` in `src/backend/Gibo.Infrastructure/Persistence/Migrations/`
- [X] T044 Execute complete end-to-end verification scenarios per [quickstart.md](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/001-onboarding-and-consent/quickstart.md)

---

## Dependencies & Execution Order

### Phase Dependencies
1. **Setup (Phase 1)**: Can begin immediately.
2. **Foundational (Phase 2)**: Depends on Phase 1. Blocks all user stories.
3. **User Story 1 (Phase 3)**: Depends on Phase 2. Delivers the core MVP.
4. **User Story 2 (Phase 4)**: Depends on Phase 2. Integrates with US1.
5. **User Story 3 (Phase 5)**: Depends on Phase 2 and US2.
6. **Polish (Phase 6)**: Depends on all user story phases being complete.

### User Story Dependencies
```text
Phase 2 (Foundation)
       │
       ▼
Phase 3 (User Story 1 - Welcome & Consent) [P1 - MVP]
       │
       ▼
Phase 4 (User Story 2 - Goals & Context)   [P1]
       │
       ▼
Phase 5 (User Story 3 - Baseline & Preview)[P2]
       │
       ▼
Phase 6 (Polish & Verification)
```

---

## Parallel Execution Opportunities

- **Backend / Mobile Separation**: Backend domain models and API handlers (`T014-T019`) can be implemented in parallel with mobile UI screens (`T020-T023`).
- **Phase 1 & 2 Tasks**: `T002`, `T003`, `T004`, `T006`, `T007`, `T008`, `T009` touch separate files and can execute concurrently.
- **Unit & Contract Tests**: Test tasks (`T011`, `T012`, `T013`, `T024`, `T032`) can be written before or alongside implementation.

---

## Implementation Strategy: MVP First

1. **Step 1**: Complete Phase 1 (Setup) and Phase 2 (Foundational).
2. **Step 2**: Implement User Story 1 (Phase 3).
3. **Step 3 (MVP Validation)**: Verify adult verification, trilingual toggle (Arabic RTL), and consent grant persistence in PostgreSQL.
4. **Step 4**: Incrementally layer User Story 2 (Goals/Context) and User Story 3 (Baseline/Plan Preview).
5. **Step 5**: Execute Phase 6 polish and run `quickstart.md` cURL and test suite commands.
