# Research: Purposeful Gamification & Weekly Gibo Styles

**Feature**: `004-gamification-and-weekly-styles`  
**Status**: Completed  
**Date**: 2026-11-18  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## 1. Immutable XP Ledger vs. Mutable Balance

### Context & Problem
In traditional gamification implementations, user XP is stored as a single mutable integer column (`Users.CurrentXp`). Under concurrent mobile requests, offline sync flushes, and background jobs, mutable balances suffer from lost updates, race conditions, and lack of auditability. Furthermore, traditional apps use streak resets and point deductions as negative reinforcement, which violates Principle I & II.

### Decision
Model XP as an append-only ledger of immutable `XpTransaction` events:
- User Balance = $\sum \text{Amount}$ where `Amount > 0` always.
- Domain guardrail: Any transaction with `Amount <= 0` throws a domain validation exception. Deductions, fines, and resets are banned at code level.
- Reasons: `TaskCompleted` (+10 XP), `PartialCompleted` (+5 XP), `ReflectionReward` (+2 XP), `WeeklyMilestone` (+25 XP).

---

## 2. Weekly Style Taxonomy & Classifier Algorithm

### Context & Problem
Users dislike being categorized into clinical diagnoses (e.g., "Inactive", "High Risk") or generic fitness personas ("Beast", "Warrior"). How do we describe constructive behavior patterns positively and non-judgmentally?

### Decision
Define 5 positive, temporary, non-clinical weekly styles evaluated over a rolling 7-day window:
1. **Comeback Champion**: Overcame a lapse or submitted a barrier and subsequently completed an action. Celebrates recovery resilience.
2. **Curious Explorer**: Swapped $\ge 2$ tasks or engaged across diverse domains. Celebrates adaptability.
3. **Mindful Balancer**: Completed $\ge 3$ sleep routines, wind-downs, or pacing actions. Celebrates restorative energy management.
4. **Steady Builder**: Completed micro-actions on $\ge 4$ distinct calendar days. Celebrates rhythmic consistency.
5. **Momentum Maker** (Default): Engaged with daily actions and building momentum one small step at a time.

### User Agency Guardrail (Principle I)
Users have a prominent toggle in Settings: *"Hide Weekly Style"*. If enabled, the style badge is completely omitted from the UI without affecting core habit tracking.

---

## 3. Data Provenance Separation in Monthly Trends

### Context & Problem
Fitness apps frequently conflate smart watch sensor data with manual self-reports, misleading users and clinical stakeholders. Principle V mandates device parity while ensuring data provenance transparency.

### Decision
- Monthly trend charts render **two distinct data series**:
  1. *Wearable-measured series* (e.g. Health Connect steps count).
  2. *Self-reported actions series* (e.g. completed walk sessions, hydration check-ins).
- Microcopy is strictly observational (e.g., "Active on 4 out of 7 days on average") and prohibited from claiming clinical biomarker outcomes (e.g. "Reduced hypertension risk by 12%").
