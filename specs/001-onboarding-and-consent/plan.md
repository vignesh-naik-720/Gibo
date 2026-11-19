# Implementation Plan: 001 - Onboarding & Consent Governance

**Branch**: `001-onboarding-and-consent` | **Date**: 2026-11-15 | **Spec**: [spec.md](file:///C:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/001-onboarding-and-consent/spec.md)

**Input**: Feature specification from `specs/001-onboarding-and-consent/spec.md`

---

## Summary

This feature establishes Gibo's foundational user entry point: trust-centered progressive onboarding, trilingual language selection (English, Arabic RTL, Hindi), adult verification (18+), goal selection (Movement, Sleep Routine, Nutrition), contextual feasibility constraints (work shifts, indoor heat preferences, equipment/space limits), accessibility adaptations, granular opt-in consent governance complying with UAE PDPL (Federal Decree-Law No. 45 of 2021) and UAE Health ICT Law (Federal Law No. 2 of 2019), and preview of the user's first achievable daily micro-plan.

The technical approach implements a Clean Architecture modular monolith in ASP.NET Core on .NET 10 LTS with PostgreSQL and EF Core on the backend, alongside an Android-first React Native (0.88.x + TypeScript) mobile application utilizing React Navigation, Zustand, and i18next with native RTL layout mirroring.

---

## Technical Context

**Language/Version**:
- Backend: C# 13 on .NET 10 LTS (supported through November 2028)
- Mobile Client: TypeScript 5.6+ on React Native 0.88.x (Android API Level 28+)

**Primary Dependencies**:
- Backend: `Microsoft.AspNetCore.OpenApi`, `Npgsql.EntityFrameworkCore.PostgreSQL` (10.x), `MediatR`, `FluentValidation.AspNetCore`, `AspNetCore.HealthChecks.NpgSql`, `BCrypt.Net-Next`
- Mobile Client: `@react-navigation/native`, `@react-navigation/native-stack`, `@tanstack/react-query` (v5), `zustand` (v5), `i18next`, `react-i18next`, `react-native-safe-area-context`, `react-native-mmkv`

**Storage**:
- PostgreSQL 16+ with Entity Framework Core 10
- Encrypted table fields (AES-256-GCM) for sensitive preference and note data
- Local client cache: `react-native-mmkv` with AES encryption key stored in Android KeyStore

**Testing**:
- Backend: `xUnit`, `FluentAssertions`, `Moq`, `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory), `Testcontainers.PostgreSql`
- Mobile: `Jest`, `@testing-library/react-native`, `jest-expo` / `react-test-renderer`
- Contract: OpenAPI schema validation via automated integration tests

**Target Platform**:
- Mobile: Android-first (Phones & Tablets, Android 9.0 / API 28 through Android 15 / API 35)
- Server: Containerized Linux (Alpine/Debian distroless), hosted in UAE sovereign cloud regions (Azure UAE Central, AWS Middle East)

**Project Type**: Mobile Application + Web API Modular Monolith

**Performance Goals**:
- Onboarding API response time: < 200ms (p95)
- Mobile cold launch to onboarding welcome screen: < 800ms
- Trilingual locale switch latency: < 50ms with zero visual layout stutter

**Constraints**:
- Absolute zero collection of medical records, diagnostics, or EHR credentials in onboarding
- Strict opt-in per consent category; no pre-checked checkboxes
- Zero transmission of identifiable UAE user data to unapproved overseas regions (Law No. 2/2019)
- Offline fallback capability for client-side state caching

**Scale/Scope**:
- Target 10,000 pilot users; 100 concurrent onboarding sessions; 6 onboarding screens

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Assessment | Status |
|---|---|---|
| **Principle I: User Agency** | Users independently choose goals, action count (1-3), data sources, quiet hours, and privacy permissions. Weight loss is strictly excluded as default goal. | **PASS** |
| **Principle II: Feasibility Over Streaks** | Onboarding starts below stated capacity (1 micro-task, 5-7 mins). Optional questions support "Skip" and "Prefer not to say". | **PASS** |
| **Principle III: Evidence & Safety** | Discloses wellness nature (not clinical advice). High-risk mobility disclosures suppress high-impact tasks and display physician consultation disclaimers. | **PASS** |
| **Principle IV: Privacy & UAE Sovereignty** | Granular consent ledger per category (`ConsentGrant`). In-app revocation and data export supported. Data stored in UAE compliance boundaries; synthetic test personas. | **PASS** |
| **Principle V: Inclusive Equity & RTL** | Equal status for non-wearable users. Full native RTL layout mirroring for Modern Standard Arabic, plus Hindi and English. High-contrast & TalkBack accessibility. | **PASS** |
| **Principle VI: Test-First & Clean Architecture** | Clean separation (`Gibo.Domain`, `Gibo.Application`, `Gibo.Infrastructure`, `Gibo.Api`). Strict nullable reference types, immutable value objects, idempotency keys. | **PASS** |

---

## Project Structure

### Documentation (this feature)

```text
specs/001-onboarding-and-consent/
├── spec.md              # Feature specification
├── plan.md              # This implementation plan
├── research.md          # Phase 0 research decisions & technology justifications
├── data-model.md        # Phase 1 domain entities, schema DDL, invariants, and state transitions
├── quickstart.md        # Phase 1 end-to-end runnable validation and test instructions
├── contracts/           # Phase 1 API contracts, schemas, and TypeScript interfaces
│   ├── onboarding.openapi.json
│   └── onboarding.types.ts
└── checklists/
    └── requirements.md  # Spec quality verification checklist
```

### Source Code (repository root)

```text
src/
├── backend/
│   ├── Gibo.Domain/
│   │   ├── Aggregates/UserAggregate/
│   │   │   ├── User.cs
│   │   │   ├── UserPreference.cs
│   │   │   ├── ConsentGrant.cs
│   │   │   ├── Goal.cs
│   │   │   └── Enums.cs
│   │   ├── Common/
│   │   └── Events/
│   ├── Gibo.Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/IdempotencyBehavior.cs
│   │   │   └── Interfaces/
│   │   └── Features/Onboarding/
│   │       ├── Commands/RegisterOnboardingUser/
│   │       ├── Queries/GetFirstPlanPreview/
│   │       └── Validators/
│   ├── Gibo.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   ├── GiboDbContext.cs
│   │   │   └── Migrations/
│   │   ├── Security/AesEncryptionProvider.cs
│   │   └── Logging/AuditEventLogger.cs
│   └── Gibo.Api/
│       ├── Controllers/v1/
│       │   ├── OnboardingController.cs
│       │   └── ConsentController.cs
│       ├── Middleware/
│       │   ├── ErrorHandlingMiddleware.cs
│       │   └── SecurityHeadersMiddleware.cs
│       └── Program.cs
│
├── mobile/
│   ├── src/
│   │   ├── features/
│   │   │   └── onboarding/
│   │   │       ├── screens/
│   │   │       │   ├── WelcomeScreen.tsx
│   │   │       │   ├── LanguageSelectionScreen.tsx
│   │   │       │   ├── ConsentScreen.tsx
│   │   │       │   ├── GoalSelectionScreen.tsx
│   │   │       │   ├── ContextConstraintsScreen.tsx
│   │   │       │   ├── AbilityBaselineScreen.tsx
│   │   │       │   └── FirstPlanPreviewScreen.tsx
│   │   │       ├── components/
│   │   │       └── state/useOnboardingStore.ts
│   │   ├── i18n/
│   │   │   ├── config.ts
│   │   │   └── locales/ (en.json, ar.json, hi.json)
│   │   ├── services/
│   │   │   └── api/onboardingApi.ts
│   │   └── navigation/
│   └── android/
│
└── tests/
    ├── backend/
    │   ├── Gibo.Domain.UnitTests/
    │   ├── Gibo.Application.UnitTests/
    │   └── Gibo.Api.IntegrationTests/
    └── mobile/
        └── src/features/onboarding/
```

**Structure Decision**: Selected Option 3 (Mobile Client + ASP.NET Core Web API Modular Monolith), matching the official architecture proposal in Section 7.3 of the project document.

---

## Complexity Tracking

*No constitutional violations identified. No exceptions required.*
