# Implementation Plan: Privacy, Trust & Abu Dhabi Data Governance

**Branch**: `006-privacy-data-governance` | **Date**: 2026-11-18 | **Spec**: [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/006-privacy-data-governance/spec.md)

**Input**: Feature specification from `specs/006-privacy-data-governance/spec.md`  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Summary

Implement comprehensive privacy by design and regulatory governance adhering to UAE Federal Decree-Law No. 45 of 2021 (PDPL) and UAE Federal Law No. 2 of 2019 (Health ICT Law). Enforce granular consent management with independent category revocation, portable data dossier generation (JSON export within 3s), atomic verified account deletion and cryptographic shredding, data minimisation (daily step aggregates instead of raw sensor streams), strict sovereign UAE residency boundaries, and an abstracted architectural boundary (`IHealthInformationExchangeAdapter`) for future Malaffi integration.

---

## Technical Context

**Language/Version**: C# 13 on .NET 10 LTS & TypeScript 5.6+ on React Native.  
**Storage**: PostgreSQL tables `ConsentGrants`, `DeviceConnections`, `AuditEvents`.  
**Testing**: Unit tests for consent revocation and deletion transactions, integration tests.  
**Constraints**: 
- Absolute zero transmission of identifiable UAE user data to unapproved overseas endpoints.
- Account deletion executes atomically across all aggregate roots.

---

## Constitution Check

| Principle | Check | Status | Verification Detail |
| :--- | :--- | :---: | :--- |
| **Principle I: User Agency** | Users have complete authority to revoke consents, export their data, or delete accounts permanently. | **PASS** | Evaluated in `spec.md` User Stories 1, 2, and 3. |
| **Principle IV: Privacy & UAE Compliance** | Compliance with UAE PDPL Articles 5, 14, and 15; data minimisation enforced; audit logging active. | **PASS** | `DataGovernanceService` implements export, revocation, and atomic purge. |
| **Principle V: Manual-Device Equality** | Revoking Health Connect consent preserves full manual functionality without penalty. | **PASS** | Invariant enforced in `RevokeConsent` command handler. |
| **Principle VI: Clean Modular Architecture** | Future Malaffi integration is safely abstracted behind an adapter interface; zero live unauthorized calls. | **PASS** | `IHealthInformationExchangeAdapter` defined with synthetic implementation. |

---

## Project Structure

### Documentation
```text
specs/006-privacy-data-governance/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── privacy.openapi.json
│   └── privacy.types.ts
├── tasks.md
└── checklists/
    └── requirements.md
```

### Source Code
```text
src/
├── backend/
│   ├── Gibo.Domain/Aggregates/PrivacyAggregate/
│   │   ├── DeviceConnection.cs
│   │   └── PrivacyEnums.cs
│   ├── Gibo.Application/Features/Privacy/
│   │   ├── Commands/RevokeConsent/
│   │   ├── Commands/DeleteAccount/
│   │   ├── Queries/ExportUserData/
│   │   ├── Queries/GetActiveConsents/
│   │   └── Services/IDataGovernanceService.cs
│   ├── Gibo.Infrastructure/
│   │   ├── Privacy/DataGovernanceService.cs
│   │   └── Integration/Hie/
│   │       ├── IHealthInformationExchangeAdapter.cs
│   │       └── SyntheticMalaffiAdapter.cs
│   └── Gibo.Api/Controllers/v1/PrivacyController.cs
└── mobile/src/features/privacy/
    ├── components/
    │   ├── ConsentToggleList.tsx
    │   ├── DataExportModal.tsx
    │   └── DeleteAccountConfirmationDialog.tsx
    ├── screens/
    │   └── PrivacySettingsScreen.tsx
    ├── services/privacyApiService.ts
    └── state/usePrivacyStore.ts
```
