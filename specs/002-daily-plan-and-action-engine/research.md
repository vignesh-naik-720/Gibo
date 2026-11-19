# Research: Daily Plan Generation & Action Engine

**Feature**: `002-daily-plan-and-action-engine`  
**Status**: Completed  
**Date**: 2026-11-15  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## 1. Multi-Factor Task Ranking & Scoring Engine

### Context & Problem
Principle III mandates that Gibo MUST NOT allow an unconstrained AI model to invent exercises, diets, or medical routines. Daily micro-actions must be drawn from a clinician-reviewed task catalog. However, simply picking tasks at random produces poor adherence. How do we rank task templates deterministically and transparently to produce 1 to 3 actions per day that match user preferences, past barriers, and Abu Dhabi climate conditions?

### Decision
Implement a deterministic 5-factor ranking algorithm in `Gibo.Application.Features.Planning.Services.TaskRankingEngine`:

$$\text{FinalScore} = (S_{\text{Goal}} \times 0.30) + (S_{\text{Feas}} \times 0.25) + (S_{\text{Barr}} \times 0.20) + (S_{\text{Pref}} \times 0.15) + (S_{\text{Nov}} \times 0.10)$$

#### Component Breakdown:
1. **Hard Safety Filter (Pre-requisite Gate, Boolean)**:
   - If user has any physical restriction in `AccessibilityTags` (e.g. `JointFriendly`, `LowImpact`, `PregnancySafe`) that conflicts with `template.SafetyTags`, template is immediately excluded ($Score = 0$, excluded from candidate pool).
   - If user has `IndoorPreferred = true` and the template has `RequiresOutdoor = true`, template is suppressed during daytime hours to prevent summer heat exposure in Abu Dhabi.
2. **Goal Relevance ($S_{\text{Goal}}$, Weight = 0.30)**:
   - $1.0$ if `template.Domain == user.PrimaryGoalDomain`
   - $0.4$ if complementary cross-domain starter task (e.g., Hydration supporting Movement)
3. **Feasibility ($S_{\text{Feas}}$, Weight = 0.25)**:
   - Matches user baseline: Starter baseline gets Starter tasks ($1.0$), Standard tasks ($0.5$), Advanced ($0.1$). Returning from lapse automatically resets feasibility target to Starter ($1.0$).
4. **Barrier Fit ($S_{\text{Barr}}$, Weight = 0.20)**:
   - Evaluates user's recent barrier logs. If user previously logged "Too busy / no time", shorter tasks (<10 mins) receive $1.0$; if "Too tired / exhausted", low-exertion/stretching receives $1.0$.
5. **User Preference ($S_{\text{Pref}}$, Weight = 0.15)**:
   - Matches environment preference (Indoor AC / Outdoor cooler window) and scheduled preferred time-of-day window (Morning, Afternoon, Evening).
6. **Novelty vs Continuity ($S_{\text{Nov}}$, Weight = 0.10)**:
   - Penalizes tasks completed yesterday (penalty $0.3$) while rewarding weekly routine stability ($0.7$ if completed 2-3 days ago).

### Rationale
- Completely auditable, unit-testable, and deterministic.
- Zero AI hallucinations or unexpected clinical hazards.
- Guarantees instant plan computation (<5ms on server, <50ms end-to-end).

---

## 2. Curated Task Catalog Architecture & Seed Data

### Context & Problem
We need an initial repository of at least 30 reviewed starter/standard templates across Movement, Sleep Routine, and Nutrition that are culturally appropriate for Abu Dhabi (e.g., air-conditioned mall walks, post-Iftar hydration, shaded corniche steps).

### Decision
Store `TaskTemplate` in PostgreSQL via EF Core with JSONB localized instructions (`Title_En`, `Title_Ar`, `Title_Hi`, `Rationale_En`, `Rationale_Ar`, `Rationale_Hi`). Seed the database on startup using `IAsyncMigration` or `ModelBuilder.HasData` with 30 reviewed templates:
- **Movement (12 templates)**:
  - 10-minute Air-Conditioned Mall Walk
  - 5-minute Desk Mobility & Shoulder Relief
  - 15-minute Evening Shaded Stroll
  - 2-minute Stand & Stretch Break
  - 10-minute Stair Climber (2 floors)
  - 7-minute Living Room Low-Impact Circuit
  - 1,000 Step Midday Errand Walk
  - 15-minute Morning Indoor March
  - 5-minute Posture Alignment & Hamstring Stretch
  - 10-minute Evening Corniche Walk (Cool hours)
  - 8-minute Seated Chair Yoga
  - 12-minute Resistance Band Basics
- **Sleep Routine (10 templates)**:
  - Digital Sundown (Phone away 30m before target sleep)
  - Bedroom Temperature Check (Set AC to optimal 20-22°C)
  - 5-minute Mindful Evening Decompression
  - Chamomile / Herbal Tea Ritual (No caffeine after 4 PM)
  - Consistent Bedtime Anchor (In bed by target window)
  - Dim Ambient Lighting 1 Hour Before Bed
  - 5-minute Brain Dump Journaling
  - 4-7-8 Breathing Relaxation
  - Silk Sleep Mask / Blackout Curtain Check
  - Warm Shower Wind-Down (60m pre-bed)
- **Nutrition & Hydration (8 templates)**:
  - Morning 500ml Hydration Kickstart
  - Afternoon Water Bottle Refill (Combat AC dehydration)
  - Add One Fresh Salad / Fruit Serving to Lunch
  - 10-minute Mindful Meal (No screens while eating)
  - Pre-Shift Hydration Anchor (500ml before commute)
  - Evening Herbal Hydration (Replace sugary soda)
  - Swap Refined Snack for Almonds / Dates
  - Post-Workout Electrolyte Water

### Rationale
Clinically grounded, culturally attuned to Abu Dhabi lifestyle, and supports immediate fallback without runtime external API dependencies.

---

## 3. Dual-Mode Completion: Manual Tap & Health Connect Parity

### Context & Problem
Principle V dictates that wearable users (via Android Health Connect) and manual check-off users MUST have 100% equal standing: exactly 10 XP awarded per full completion, with no second-class UX or punitive scoring for manual users.

### Decision
- The domain model `CompletionEvent` records `EvidenceSource`:
  - `EvidenceSource.Manual`: Triggered by one-tap check-off on the action card.
  - `EvidenceSource.HealthConnect`: Triggered by background sync reading Android Health Connect `StepsRecord` or `ExerciseSessionRecord`.
- Both sources invoke the same domain command: `CompleteTaskCommand`, granting identical 10 XP and appending to the user's private progression ledger.
- For partial tasks (e.g. 2 glasses of water, 1 logged), `Partial` state grants 5 XP.
- Double-submission guard: If a manual tap occurs at 14:00 and Health Connect sync occurs at 14:05 for the same `TaskInstance`, the domain rejects the second event idempotently, returning the existing completion state with 0 additional XP.

### Alternatives Considered
- *Giving wearable users bonus XP for "verified" data*: Rejected as a violation of Principle V (discriminated against users without smartwatches).
- *Requiring photo evidence*: Rejected as intrusive, high friction, and privacy-violating.

---

## 4. Offline Resilience & Idempotent Synchronization

### Context & Problem
Users commuting through tunnels, parking basements, or remote desert roads may experience dropped cellular coverage while completing habits.

### Decision
- **Client Storage**: React Native caches today's `DailyPlan` in local encrypted storage (using `zustand/middleware` with MMKV or AsyncStorage).
- **Offline Action Queue**: When offline, marking a task as completed:
  1. Optimistically updates UI to `Completed` with animated feedback.
  2. Creates a `QueuedCompletionEvent` with a client-generated UUID (`ClientEventId`) and `RecordedAtUtc`.
  3. Appends the event to a persistent offline FIFO queue.
- **Background Sync**: When `NetInfo.isConnected` transitions to true:
  1. Mobile client dispatches `POST /api/v1/tasks/sync-offline` with the queued events.
  2. Each event includes its `Idempotency-Key` (matching `ClientEventId`).
  3. Backend deduplicates using the existing `IdempotencyBehavior` pipeline and records `SyncedAtUtc`.
  4. Client removes confirmed events from the offline queue.

---

## 5. User Overrides: Target Adjustment, Swapping & Pausing

### Context & Problem
Real life disrupts plans. If a user has a sudden meeting or feels unwell, rigid software forces them to fail the task. Principle II requires adaptive flexibility.

### Decision
- **Reduce Target**: Allows user to scale a 15-minute walk down to an approved 5-minute starter walk. Updates `TaskInstance.TargetValue = 5` and flags `UserEditsApplied = true`.
- **Swap Task**: Presents 2-3 ranked alternative templates from the same domain. Substituting a template archives the previous instance and creates the replacement instance, keeping plan count constant.
- **Pause Domain**: If user has a migraine or injury, they can pause the domain for today. Tasks transition to `Paused` (neutral, non-punitive state) and no notifications are sent.
- **Engine Invariant**: Background plan regeneration routines check `DailyPlan.UserEditsApplied`. If `true`, the automated engine will NEVER overwrite the user's manual adjustments.
