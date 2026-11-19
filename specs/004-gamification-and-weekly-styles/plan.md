# Implementation Plan: Purposeful Gamification & Weekly Gibo Styles

**Branch**: `004-gamification-and-weekly-styles` | **Date**: 2026-11-18 | **Spec**: [spec.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/specs/004-gamification-and-weekly-styles/spec.md)

**Input**: Feature specification from `specs/004-gamification-and-weekly-styles/spec.md`  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## Summary

Implement an immutable, append-only XP transaction ledger where deductions and streak resets are strictly banned by domain invariants. Introduce the non-clinical, encouraging weekly Gibo style classifier celebrating 5 constructive behavioral patterns (`ComebackChampion`, `CuriousExplorer`, `MindfulBalancer`, `SteadyBuilder`, `MomentumMaker`) with user agency toggles to hide or disable gamification. Deliver monthly trend visualizations that clearly separate wearable sensor metrics from manual self-reports with neutral observational language.

---

## Technical Context

**Language/Version**: C# 13 on .NET 10 LTS & TypeScript 5.6+ on React Native.  
**Storage**: PostgreSQL tables `XpTransactions`, `WeeklyInsights`, `MonthlyMetricAggregates`.  
**Testing**: Domain unit tests (`XpTransactionTests`, `WeeklyStyleClassifierTests`), API integration tests.  
**Constraints**: 
- Invariant: XP deductions (`amount <= 0`) are prohibited at the domain entity level.
- No public leaderboards or sharing of user activity data.

---

## Constitution Check

| Principle | Check | Status | Verification Detail |
| :--- | :--- | :---: | :--- |
| **Principle I: User Agency** | Users can hide weekly styles or disable gamification entirely; no public leaderboards. | **PASS** | `ToggleStyleVisibility` endpoint and private tenant scoping. |
| **Principle II: Recovery Over Streaks** | No streak-loss penalties or negative XP; celebrates recovery with "Comeback Champion" style. | **PASS** | Style classifier prioritizes recovery after barriers. |
| **Principle III: Evidence-Based AI** | Weekly styles use deterministic classification over 7-day signals; zero clinical diagnostic claims. | **PASS** | Banned vocabulary check prevents medical or shaming terminology. |
| **Principle V: Equity & Provenance** | 10 XP awarded equally for manual and Health Connect; monthly trends clearly label data sources. | **PASS** | Monthly trend schema includes separate `sensorUnits` and `selfReportedCount`. |

---

## Project Structure

### Documentation
```text
specs/004-gamification-and-weekly-styles/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── gamification.openapi.json
│   └── gamification.types.ts
├── tasks.md
└── checklists/
    └── requirements.md
```

### Source Code
```text
src/
├── backend/
│   ├── Gibo.Domain/Aggregates/GamificationAggregate/
│   │   ├── XpTransaction.cs
│   │   ├── WeeklyInsight.cs
│   │   ├── MonthlyMetricAggregate.cs
│   │   └── GamificationEnums.cs
│   ├── Gibo.Application/Features/Gamification/
│   │   ├── Queries/GetXpBalance/
│   │   ├── Queries/GetWeeklyStyle/
│   │   ├── Queries/GetMonthlyTrends/
│   │   └── Services/WeeklyStyleClassifier.cs
│   ├── Gibo.Infrastructure/Persistence/Configurations/GamificationConfigurations.cs
│   └── Gibo.Api/Controllers/v1/GamificationController.cs
└── mobile/src/features/gamification/
    ├── components/
    │   ├── XpCounterBadge.tsx
    │   ├── WeeklyStyleCard.tsx
    │   └── ProvenanceTrendChart.tsx
    ├── screens/
    │   └── WeeklyRecapScreen.tsx
    ├── services/gamificationApiService.ts
    └── state/useGamificationStore.ts
```
