# Implementation Plan: Daily Plan Generation & Action Engine

**Branch**: `002-daily-plan-and-action-engine` | **Date**: 2026-11-15 | **Spec**: [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/002-daily-plan-and-action-engine/spec.md)

**Input**: Feature specification from `specs/002-daily-plan-and-action-engine/spec.md`  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Summary

Deliver the core behavioural engine of Gibo: a rules-first, transparent task ranking and scoring system operating over a curated library of 30+ clinician-reviewed action templates. Generates 1 to 3 daily micro-actions tailored to user goals (Movement, Sleep Routine, Nutrition), baseline fitness, and Abu Dhabi climate constraints. Implements dual-mode completion (manual tap + Health Connect background sync with 100% XP parity), user plan adjustments (target reduction, task swapping, domain pause), and offline event queueing with idempotent backend synchronization.

---

## Technical Context

**Language/Version**: C# 13 on .NET 10 LTS (Modular Monolith Backend) & TypeScript 5.6+ on React Native 0.76+ (Expo SDK 54 Mobile Client).  
**Primary Dependencies**: 
- Backend: MediatR 12.x, FluentValidation 11.x, Entity Framework Core 10, Npgsql.
- Mobile: Zustand (local UI & offline queue state), TanStack Query (server state), i18next (trilingual localized strings), React Native Safe Area & Screens.  
**Storage**: PostgreSQL 16+ with EF Core (tables: `DailyPlans`, `TaskInstances`, `TaskTemplates`, `CompletionEvents`).  
**Testing**: 
- Backend: Unit tests (`Gibo.Domain.UnitTests`), API Integration tests (`Gibo.Api.IntegrationTests`).
- Mobile: Jest component and store tests.  
**Target Platform**: Android (Google Play Services / Android 9+ with Health Connect) and cross-platform mobile client; ASP.NET Core Linux/Windows containerized backend.  
**Project Type**: Mobile Application + Web API Modular Monolith.  
**Performance Goals**: 
- Daily plan retrieval & generation in $<500\text{ms}$ on mobile launch.
- Algorithmic scoring computation $<5\text{ms}$ for 30 candidates.  
**Constraints**: 
- Offline-capable: cached daily plan renders offline; completion events queue locally and flush idempotently upon reconnection.
- Hard boolean safety filters eliminate contraindications prior to scoring.  
**Scale/Scope**: 30 seeded task templates, 4 domain aggregates, 5 API endpoints, 3 primary mobile screen components (Action Card, Detail Modal, Swap Sheet).

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status | Verification Detail |
| :--- | :--- | :---: | :--- |
| **Principle I: User Agency** | Action cap limited to $[1, 3]$ daily; user can edit target, swap, or pause tasks without penalty or guilt copy. | **PASS** | Evaluated in `spec.md` User Story 3 & `data-model.md` `UserEditsApplied`. |
| **Principle II: Feasibility & Adaptive Recovery** | Starter fallback for low-confidence/lapse returns (5-min actions); no punitive streak resets or XP deductions on skips. | **PASS** | `IsStarterFallback` flag and neutral state transitions (`Skipped`, `Expired`). |
| **Principle III: Evidence-Based AI & Safety** | Task selector operates strictly over reviewed `TaskTemplate` catalog; zero runtime LLM exercise invention; hard safety exclusion filters. | **PASS** | `TaskRankingEngine` is 100% deterministic rules-first algorithm. |
| **Principle IV: Privacy & UAE Compliance** | No health telemetry exported overseas; granular consent respected; completion records use UTC timestamps with client UUIDs. | **PASS** | `CompletionEvent` architecture uses tenant-scoped `UserId` with audit logs. |
| **Principle V: Inclusive Equity & Device Parity** | Manual completion and Health Connect sync both award exactly 10 XP; Arabic RTL support with localized action titles. | **PASS** | Hardcoded invariant in `TaskInstance.Complete()` for both evidence sources. |
| **Principle VI: Test-First & Offline Resilience** | Clean CQRS modular monolith; PostgreSQL relational integrity; local persistent offline queue; idempotent HTTP sync. | **PASS** | Contracts specify `Idempotency-Key` and batch `sync-offline` endpoints. |

---

## Project Structure

### Documentation (this feature)

```text
specs/002-daily-plan-and-action-engine/
├── spec.md              # Feature specification & user stories
├── plan.md              # This file (/speckit-plan output)
├── research.md          # Multi-factor ranking & sync research (/speckit-plan output)
├── data-model.md        # Aggregate models, state machine & invariants (/speckit-plan output)
├── quickstart.md        # Validation scenarios & curl commands (/speckit-plan output)
├── contracts/           # API and TypeScript interface contracts (/speckit-plan output)
│   ├── daily-plan.openapi.json
│   └── daily-plan.types.ts
└── checklists/          # Requirements verification checklists
    └── requirements.md
```

### Source Code Layout

```text
src/
├── backend/
│   ├── Gibo.Domain/
│   │   └── Aggregates/
│   │       ├── PlanAggregate/
│   │       │   ├── DailyPlan.cs              # Aggregate Root
│   │       │   ├── TaskInstance.cs           # Scheduled daily action entity
│   │       │   ├── TaskTemplate.cs           # Curated master template entity
│   │       │   ├── CompletionEvent.cs        # Immutable completion audit record
│   │       │   └── PlanEnums.cs              # TaskState, EvidenceSource, Difficulty, etc.
│   │       └── Events/
│   │           ├── TaskCompletedEvent.cs
│   │           └── PlanGeneratedEvent.cs
│   ├── Gibo.Application/
│   │   └── Features/
│   │       └── Planning/
│   │           ├── Commands/
│   │           │   ├── GenerateDailyPlan/
│   │           │   ├── CompleteTask/
│   │           │   ├── SwapTask/
│   │           │   └── SyncOfflineEvents/
│   │           ├── Queries/
│   │           │   ├── GetTodayPlan/
│   │           │   └── GetSwapAlternatives/
│   │           └── Services/
│   │               ├── ITaskRankingEngine.cs
│   │               └── TaskRankingEngine.cs  # 5-factor deterministic ranking implementation
│   ├── Gibo.Infrastructure/
│   │   └── Persistence/
│   │       ├── Configurations/
│   │       │   ├── DailyPlanConfiguration.cs
│   │       │   ├── TaskInstanceConfiguration.cs
│   │       │   ├── TaskTemplateConfiguration.cs
│   │       │   └── CompletionEventConfiguration.cs
│   │       └── Seeding/
│   │           └── TaskCatalogSeed.cs        # 30 curated Abu Dhabi action templates
│   └── Gibo.Api/
│       └── Controllers/v1/
│           ├── PlansController.cs
│           └── TasksController.cs
└── mobile/
    └── src/
        ├── features/
        │   └── dailyPlan/
        │       ├── components/
        │       │   ├── ActionCard.tsx         # Primary daily micro-habit card
        │       │   ├── TaskDetailModal.tsx    # "Why this fits you" & target details
        │       │   └── TaskSwapSheet.tsx      # Alternative selection bottom sheet
        │       ├── screens/
        │       │   └── TodayPlanScreen.tsx    # Home dashboard daily plan surface
        │       ├── services/
        │       │   ├── dailyPlanApiService.ts
        │       │   └── offlineSyncService.ts  # Background queue & reconnect flusher
        │       └── state/
        │           └── useDailyPlanStore.ts   # Zustand offline-first store
        └── navigation/
            └── RootNavigator.tsx
```

---

## Complexity Tracking

*No constitutional violations. Design strictly adheres to modular monolith and offline resilience guidelines.*

| Decision | Why Needed | Alternative Rejected |
| :--- | :--- | :--- |
| Curated `TaskTemplate` catalog in PostgreSQL | Principle III compliance: eliminates runtime LLM hallucination risk for health habits. | Dynamic LLM generation rejected due to clinical safety hazards and lack of determinism. |
| Relational `CompletionEvent` ledger | Principle IV & VI compliance: Provides auditable proof of completion and duplicate prevention. | In-memory counter rejected because it loses auditability and drops offline events. |
| Client-generated UUIDs (`ClientEventId`) | Required for idempotent offline synchronization over cellular network drops. | Server-generated IDs alone rejected because offline actions cannot obtain server IDs while disconnected. |
