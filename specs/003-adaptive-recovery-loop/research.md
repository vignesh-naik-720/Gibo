# Research: Adaptive Recovery Loop & Disruption Handling

**Feature**: `003-adaptive-recovery-loop`  
**Status**: Completed  
**Date**: 2026-11-18  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## 1. Friction Taxonomy & Barrier Classification

### Context & Problem
Habit adherence literature (e.g. Michie's COM-B model) demonstrates that habits lapse because of capability, opportunity, or motivation friction. Traditional apps punish lapses by breaking streaks. In Gibo, a missed action is diagnostic information. How do we categorize user barriers cleanly and empathetically?

### Decision
Implement 7 explicit barrier categories in `BarrierCategory` enum:
1. `TimingConflict` (Opportunity: time budget, shift work, unexpected meetings).
2. `TooDifficult` (Capability: stamina, complexity, target too ambitious).
3. `LowEnergy` (Capability/Motivation: fatigue, burnout, mental exhaustion).
4. `AccessOrWeather` (Opportunity: midday summer heat in Abu Dhabi, outdoor closure).
5. `Forgot` (Prompt/Memory: routine interrupted, no reminder anchor).
6. `UnwellOrPain` (Safety: acute health distress, illness, injury).
7. `PreferNotToSay` (Agency: user wants adjustment without disclosure).

### Reflection Incentive (Principle II)
Submitting a barrier check-in awards **+2 XP**. This reframes honest self-reflection as an active habit-support behavior rather than an admission of failure.

---

## 2. Adaptation Decision Matrix

### Context & Problem
How does the system deterministically map a submitted barrier into a concrete, low-friction plan modification?

### Decision
The `AdaptationDecisionMatrix` in `RecoveryAdaptationService` applies rules-first mapping:
- **`TimingConflict`** $\to$ Shift scheduled time window (e.g., Afternoon $\to$ Evening) or switch to a 3-minute asynchronous micro-action.
- **`TooDifficult`** $\to$ Step-down target by 50% (e.g. 15 mins $\to$ 7 mins; Graded Task BCT).
- **`LowEnergy`** $\to$ Replace with a gentle low-exertion recovery template (e.g., 3-minute breathwork, seated neck/shoulder stretch).
- **`AccessOrWeather`** $\to$ Immediately swap outdoor templates with indoor air-conditioned templates (e.g., Mall Walking, Living Room Pacing).
- **`Forgot`** $\to$ Prompt reminder anchor optimization (linking action to an existing daily event like after breakfast or before shift).
- **`UnwellOrPain`** $\to$ Trigger immediate **Safety Halt** (see Section 3).

---

## 3. Clinical Safety Intercept & Escalation Protocol

### Context & Problem
Adherence to Principle III (Evidence Before AI Fluency) and UAE Health ICT Law requires absolute safeguards against coaching an injured or acutely unwell user.

### Decision
- When `UnwellOrPain` is selected or user note matches red-flag clinical keywords (e.g., `chest pain`, `shortness of breath`, `dizziness`, `fainting`, `fever`):
  1. All physical activity and nutrition tasks are immediately **suspended** (`State = Paused`).
  2. The home screen replaces action cards with an **Emergency Guidance Banner**:
     - **Abu Dhabi Emergency Services (Ambulance)**: **998**
     - **SEHA Healthcare Teleconsultation**: **800 50**
     - **Estijaba Health Services Center**: **800 1717**
  3. No XP rewards or gamification milestones are shown.
  4. Reactivating standard tasks in subsequent days requires an explicit self-attestation: *"I am feeling well and cleared for light physical activity."*

---

## 4. Guilt-Free Re-entry After Absence (The "Warm Welcome" Flow)

### Context & Problem
When users return after 7–14+ days of absence, apps that display broken streaks, missed alerts, and negative feedback cause cognitive dissonance and permanent abandonment.

### Decision
- **Inactivity Detection**: If `DaysSinceLastActive >= 7`:
  - Suppress notification backlogs.
  - Display non-judgmental re-entry screen: *"Welcome back! Life gets full. Your past progress and XP are right where you left them."*
  - Automatically reset today's plan to **Starter baseline** (exactly 1 micro-task, 3–5 minutes) to ensure immediate psychological win.
  - Zero historical XP deducted.
