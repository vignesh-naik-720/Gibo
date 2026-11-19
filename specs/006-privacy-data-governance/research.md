# Research: Privacy, Trust & Abu Dhabi Data Governance

**Feature**: `006-privacy-data-governance`  
**Status**: Completed  
**Date**: 2026-11-18  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## 1. UAE Regulatory Alignment (PDPL & Health ICT Law)

### Context & Problem
Operating a health companion in Abu Dhabi mandates compliance with:
- **UAE Federal Decree-Law No. 45 of 2021 (PDPL)**: User rights to access, consent revocation, data portability (Art 14), and erasure (Art 15).
- **UAE Federal Law No. 2 of 2019 (Health ICT Law)**: Data residency within UAE borders, strict purpose limitation, and ban on storing identifiable health data overseas without ministerial approval.
- **DoH Abu Dhabi Information & Cyber Security Standard (ADHICS V2)**: Audit logging, AES-256 envelope encryption, and role-based access.

### Decision
- **Data Minimisation**: Store daily step aggregates (e.g. 1,200 steps) rather than continuous GPS tracking, raw sensor feeds, or heart rate second-by-second traces.
- **Data Residency**: Architecture enforces UAE sovereign hosting boundaries; hackathon and demo environments operate exclusively on synthetic profiles.
- **Sovereign User Rights**:
  - `ExportUserDataAsync`: Generates portable, sanitized JSON dossiers within 3 seconds.
  - `ExecuteAccountDeletionAsync`: Executes an atomic database transaction that revokes consents, shreds PII, deletes device connection tokens, and logs a tamper-proof audit event.

---

## 2. Granular Consent Ledger Architecture

### Context & Problem
Bundled "all-or-nothing" terms of service violate PDPL Article 5.

### Decision
Maintain an immutable `ConsentGrant` ledger per category:
- `CoreProfile` (Required for basic habit recommendations)
- `HealthConnectSync` (Optional wearable synchronization)
- `AssistantChatContext` (Optional conversational memory)
- `AnonymousResearch` (Optional de-identified analytics)
Revoking consent for `HealthConnectSync` immediately unlinks wearable credentials without affecting manual task completion.

---

## 3. Abu Dhabi HIE (Malaffi) Integration Boundary

### Context & Problem
Hackathon submissions sometimes falsely claim live integration with Malaffi (Abu Dhabi Health Information Exchange). Unapproved access to government clinical systems is illegal and breaches DoH policy.

### Decision
Define a clean, abstracted boundary interface: `IHealthInformationExchangeAdapter`. Provide a `SyntheticMalaffiAdapter` for testing and demonstration, and clearly document that production Malaffi integration is a strategic future roadmap item subject to formal DoH accreditation and clinical sponsorship.
