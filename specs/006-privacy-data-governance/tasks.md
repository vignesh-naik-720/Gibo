# Tasks: Privacy, Trust & Abu Dhabi Data Governance

**Input**: Design documents from `specs/006-privacy-data-governance/`  
**Prerequisites**: [plan.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/006-privacy-data-governance/plan.md), [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/006-privacy-data-governance/spec.md), [research.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/006-privacy-data-governance/research.md), [data-model.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/006-privacy-data-governance/data-model.md), [contracts/](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/006-privacy-data-governance/contracts/)  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Code Concepts & Architectural Notes for Implementers

> [!NOTE]
> **Developer Guidance**: All tasks below MUST adhere to clean code principles:
> 1. **UAE Data Sovereignty & Minimisation (Principle IV)**:
>    Store only daily step aggregates; never record raw continuous GPS or sensor streams. Strictly prohibit overseas transmission of identifiable data.
> 2. **Instant Consent Revocation**:
>    Revoking a consent category (e.g., `HealthConnectSync`) must take effect immediately (<100ms), severing provider credentials while preserving core manual habit features.
> 3. **Atomic Purge (PDPL Article 15)**:
>    Account deletion MUST execute within an atomic transaction that revokes consents, shreds PII, and invalidates active JWT tokens without leaving orphan records.
> 4. **HIE Boundary Isolation**:
>    Abstract Malaffi readiness behind `IHealthInformationExchangeAdapter`. The prototype connects only to a synthetic adapter; live governmental calls are strictly prohibited.

---

## Phase 1: Setup (Contracts & Domain Enums)

- [ ] T001 Define privacy domain enums (`DeviceProvider`) in `src/backend/Gibo.Domain/Aggregates/PrivacyAggregate/PrivacyEnums.cs`
- [ ] T002 [P] Copy TypeScript contracts into mobile codebase in `src/mobile/src/features/privacy/contracts/privacy.types.ts`
- [ ] T003 [P] Add privacy policy, consent purpose, and data export strings (English, Arabic RTL, Hindi) in `src/mobile/src/i18n/locales/`

---

## Phase 2: Foundational (Entities & Governance Service)

**⚠️ CRITICAL**: Must be completed before user story implementation begins.

- [ ] T004 Implement `DeviceConnection` domain entity managing wearable tokens in `src/backend/Gibo.Domain/Aggregates/PrivacyAggregate/DeviceConnection.cs`
- [ ] T005 [P] Configure EF Core mappings for `DeviceConnection` in `src/backend/Gibo.Infrastructure/Persistence/Configurations/PrivacyConfigurations.cs`
- [ ] T006 Implement `IDataGovernanceService` in `src/backend/Gibo.Application/Features/Privacy/Services/IDataGovernanceService.cs`
- [ ] T007 Implement concrete `DataGovernanceService` in `src/backend/Gibo.Infrastructure/Privacy/DataGovernanceService.cs`
- [ ] T008 [P] Write unit tests validating atomic deletion and consent revocation in `tests/backend/Gibo.Domain.UnitTests/DataGovernanceTests.cs`

---

## Phase 3: User Story 1 - Granular Consent Review & Immediate Revocation (Priority: P1) 🎯 MVP

**Goal**: Enable users to view and independently revoke consent categories with immediate effect.

- [ ] T009 [P] [US1] Implement `GetActiveConsentsQuery` in `src/backend/Gibo.Application/Features/Privacy/Queries/GetActiveConsents/GetActiveConsentsQueryHandler.cs`
- [ ] T010 [P] [US1] Implement `RevokeConsentCategoryCommand` in `src/backend/Gibo.Application/Features/Privacy/Commands/RevokeConsent/RevokeConsentCommandHandler.cs`
- [ ] T011 [US1] Create `PrivacyController` exposing `GET /api/v1/privacy/consents` and `POST /api/v1/privacy/consents/{category}/revoke` in `src/backend/Gibo.Api/Controllers/v1/PrivacyController.cs`
- [ ] T012 [P] [US1] Build mobile `ConsentToggleList.tsx` showing granular opt-in statuses in `src/mobile/src/features/privacy/components/ConsentToggleList.tsx`
- [ ] T013 [US1] Build `PrivacySettingsScreen.tsx` integrating consent management in `src/mobile/src/features/privacy/screens/PrivacySettingsScreen.tsx`

---

## Phase 4: User Story 3 - Verified Account Deletion & Cryptographic Shredding (Priority: P1)

**Goal**: Provide a verified 2-step account deletion flow executing an atomic database purge.

- [ ] T014 [P] [US3] Implement `DeleteAccountCommand` executing transactional hard-deletion and token invalidation in `src/backend/Gibo.Application/Features/Privacy/Commands/DeleteAccount/DeleteAccountCommandHandler.cs`
- [ ] T015 [US3] Expose `DELETE /api/v1/privacy/account` in `src/backend/Gibo.Api/Controllers/v1/PrivacyController.cs`
- [ ] T016 [P] [US3] Build `DeleteAccountConfirmationDialog.tsx` with explicit confirmation prompt in `src/mobile/src/features/privacy/components/DeleteAccountConfirmationDialog.tsx`
- [ ] T017 [US3] Wire account deletion to clear mobile local encrypted storage and reset to `WelcomeScreen`

---

## Phase 5: User Story 2 - Portable User Data Export (Priority: P2)

**Goal**: Deliver a structured, portable JSON dossier of the user's complete data record within 3 seconds.

- [ ] T018 [P] [US2] Implement `ExportUserDataCommand` and JSON serializer in `src/backend/Gibo.Application/Features/Privacy/Commands/ExportUserData/ExportUserDataCommandHandler.cs`
- [ ] T019 [US2] Expose `POST /api/v1/privacy/export` in `src/backend/Gibo.Api/Controllers/v1/PrivacyController.cs`
- [ ] T020 [P] [US2] Build `DataExportModal.tsx` displaying download progress and payload preview in `src/mobile/src/features/privacy/components/DataExportModal.tsx`

---

## Phase 6: User Story 4 - Future HIE / Malaffi Integration Boundary (Priority: P3)

**Goal**: Abstract Malaffi readiness behind an explicit interface without making unauthorized external calls.

- [ ] T021 [P] [US4] Define `IHealthInformationExchangeAdapter` interface in `src/backend/Gibo.Infrastructure/Integration/Hie/IHealthInformationExchangeAdapter.cs`
- [ ] T022 [P] [US4] Implement `SyntheticMalaffiAdapter` operating exclusively on synthetic test data in `src/backend/Gibo.Infrastructure/Integration/Hie/SyntheticMalaffiAdapter.cs`
- [ ] T023 [US4] Document Malaffi regulatory prerequisites and accreditation roadmap in `docs/malaffi-integration-architecture.md`

---

## Phase 7: Polish & Verification

- [ ] T024 [P] Write integration tests for consent revocation, data export, and account deletion in `tests/backend/Gibo.Api.IntegrationTests/PrivacyEndpointsTests.cs`
- [ ] T025 Execute `dotnet test` and confirm 100% pass rate across entire solution
- [ ] T026 Audit Arabic RTL layout mirroring for privacy toggles, export dialogs, and deletion warnings
- [ ] T027 Execute and verify all 3 scenarios from `quickstart.md`

---

## Dependency & Execution Order

```text
Phase 1: Setup (T001-T003)
       │
       ▼
Phase 2: Foundational (T004-T008) ⚠️ BLOCKING
       │
       ▼
Phase 3: User Story 1 (Consent Revocation) [P1 - MVP] (T009-T013)
       │
       ▼
Phase 4: User Story 3 (Account Deletion) [P1] (T014-T017)
       │
       ▼
Phase 5: User Story 2 (Data Export) [P2] (T018-T020)
       │
       ▼
Phase 6: User Story 4 (HIE / Malaffi Boundary) [P3] (T021-T023)
       │
       ▼
Phase 7: Polish & Verification (T024-T027)
```
