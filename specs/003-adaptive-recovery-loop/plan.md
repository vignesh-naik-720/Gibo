# Implementation Plan: Adaptive Recovery Loop & Disruption Handling

**Branch**: `003-adaptive-recovery-loop` | **Date**: 2026-11-18 | **Spec**: [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/003-adaptive-recovery-loop/spec.md)

**Input**: Feature specification from `specs/003-adaptive-recovery-loop/spec.md`  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Summary

Implement the core product differentiator of Gibo: transforming missed actions from guilt-inducing failures into diagnostic signals. Build the `AdaptationDecisionMatrix` that maps 7 friction categories into targeted plan adjustments (smaller targets, time shifts, indoor weather options, or pauses), provides non-punitive +2 XP reflection rewards, delivers a welcoming re-entry flow after extended absences without lecturing, and enforces immediate safety intercepts (routing to Abu Dhabi emergency 998 and teleconsultation 800-50) when acute symptoms are disclosed.

---

## Technical Context

**Language/Version**: C# 13 on .NET 10 LTS & TypeScript 5.6+ on React Native (Expo SDK 54).  
**Primary Dependencies**: MediatR, FluentValidation, EF Core 10, Zustand, i18next.  
**Storage**: PostgreSQL tables `BarrierFeedbacks`, `PlanAdaptations`, `SafetyEscalationEvents`.  
**Testing**: Domain unit tests (`BarrierAdaptationTests`), API integration tests (`RecoveryEndpointsTests`).  
**Constraints**: 
- Absolute zero streak resets or XP deductions on missed actions.
- AES-256 envelope encryption on optional free-text user notes.
- Safety halts must execute in $<100\text{ms}$.

---

## Constitution Check

| Principle | Check | Status | Verification Detail |
| :--- | :--- | :---: | :--- |
| **Principle I: User Agency** | Users voluntarily select barriers; can accept, customize, or decline proposed adaptations. | **PASS** | `RespondAdaptationRequestDto` preserves full user decision authority. |
| **Principle II: Adaptive Recovery Over Streaks** | Missed tasks earn +2 XP reflection reward; no guilt-inducing copy; warm re-entry after absence. | **PASS** | `XpAwarded = 2` hardcoded for barrier check-ins; re-entry preserves 100% past XP. |
| **Principle III: Evidence-Based AI & Safety** | Decision matrix is rules-based; acute health distress immediately halts coaching and displays official UAE emergency contacts. | **PASS** | Hard-coded `SafetyHalt` triggered on `UnwellOrPain` or clinical keywords. |
| **Principle IV: Privacy & Data Protection** | Free-text notes encrypted via AES-256 envelope provider; audit trail for safety escalations. | **PASS** | `EncryptedUserNote` field uses `AesEncryptionProvider`. |
| **Principle V: Inclusive Multilingual Parity** | All check-in prompts, friction cards, and recovery explanations provided in English, Arabic (RTL), and Hindi. | **PASS** | Included in i18n locale bundles. |
| **Principle VI: Test-First & Clean CQRS** | Domain models encapsulated; MediatR pipeline commands; comprehensive unit test suites. | **PASS** | Clean boundary separation in `Gibo.Application.Features.Recovery`. |

---

## Project Structure

### Documentation
```text
specs/003-adaptive-recovery-loop/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── recovery.openapi.json
│   └── recovery.types.ts
├── tasks.md
└── checklists/
    └── requirements.md
```

### Source Code
```text
src/
├── backend/
│   ├── Gibo.Domain/Aggregates/RecoveryAggregate/
│   │   ├── BarrierFeedback.cs
│   │   ├── PlanAdaptation.cs
│   │   ├── SafetyEscalationEvent.cs
│   │   └── RecoveryEnums.cs
│   ├── Gibo.Application/Features/Recovery/
│   │   ├── Commands/SubmitBarrier/
│   │   ├── Commands/RespondAdaptation/
│   │   ├── Queries/GetReEntryStatus/
│   │   └── Services/RecoveryAdaptationService.cs
│   ├── Gibo.Infrastructure/Persistence/Configurations/RecoveryConfigurations.cs
│   └── Gibo.Api/Controllers/v1/RecoveryController.cs
└── mobile/src/features/recovery/
    ├── components/
    │   ├── BarrierCheckInModal.tsx
    │   ├── AdaptationProposalCard.tsx
    │   └── EmergencyGuidanceBanner.tsx
    ├── screens/
    │   └── ReEntryWelcomeScreen.tsx
    ├── services/recoveryApiService.ts
    └── state/useRecoveryStore.ts
```

---

## Complexity Tracking

| Decision | Why Needed | Alternative Rejected |
| :--- | :--- | :--- |
| Predefined 7-category taxonomy | Fast, low-friction tap selection; enables deterministic rules-based adaptation without unconstrained LLM parsing. | Open-ended text prompt alone rejected due to high cognitive load during moments of user fatigue or stress. |
| Dedicated `SafetyEscalationEvent` audit table | Required by UAE Health ICT regulations to prove non-clinical compliance and immediate disengagement upon reported health risks. | Ephemeral error logging rejected because regulatory compliance requires persistent auditable proof of safety halts. |
