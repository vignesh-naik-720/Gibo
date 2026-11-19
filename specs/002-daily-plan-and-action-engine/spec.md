# Feature Specification: Daily Plan Generation & Action Engine

**Feature Branch**: `002-daily-plan-and-action-engine`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "Approved task template catalog; rules-first transparent ranking and scoring engine; daily plan generation (up to 3 small measurable actions with starter fallback); action cards with clear verb, target, rationale, timing, and evidence mode; dual-mode completion supporting manual confirmation and optional Health Connect background sync; task editing, swapping, and goal pausing; offline resilience with idempotent event synchronization."

---

## Code Concepts & Architectural Commentary for Programmers

```csharp
// ==============================================================================================
// ARCHITECTURAL CONCEPT: Rules-First Multi-Factor Task Ranking & Scoring Engine
// To adhere to Principle III (Evidence Before AI Fluency) and prevent arbitrary or risky task
// generation, tasks are strictly selected from a curated catalog of reviewed templates.
// Scoring is deterministic and transparent:
//   FinalScore = (GoalRelevance * 0.30) + (Feasibility * 0.25) + (BarrierFit * 0.20) 
//                + (UserPreference * 0.15) + (NoveltyContinuity * 0.10)
// Hard safety exclusions (contraindications, environmental heat constraints) operate as absolute
// boolean filters, NEVER as soft score penalties.
// ==============================================================================================

namespace Gibo.Application.Planning.Services;

public sealed class TaskRankingEngine : ITaskRankingEngine
{
    private const double WeightGoalRelevance = 0.30;
    private const double WeightFeasibility = 0.25;
    private const double WeightBarrierFit = 0.20;
    private const double WeightUserPreference = 0.15;
    private const double WeightContinuityNovelty = 0.10;

    public IEnumerable<ScoredTaskCandidate> RankCandidates(
        UserPreference preferences, 
        IReadOnlyList<TaskTemplate> library, 
        IReadOnlyList<BarrierFeedback> recentBarriers,
        IReadOnlyList<TaskInstance> recentHistory)
    {
        var eligibleCandidates = new List<ScoredTaskCandidate>();

        foreach (var template in library)
        {
            // STEP 1: Hard Safety & Exclusion Filter
            // If the user has a mobility limitation or quiet hours conflict, exclude immediately.
            if (template.SafetyTags.Any(tag => preferences.AccessibilityTags.Contains(tag)))
            {
                // Task is contraindicated for this user profile - drop from candidate pool
                continue;
            }

            // Summer heat / weather constraint filter:
            // If the user preferred indoor activities, suppress outdoor-only templates.
            if (preferences.IndoorPreferred && template.RequiresOutdoor)
            {
                continue;
            }

            // STEP 2: Multi-Factor Scoring
            // 2a. Goal Relevance (0.0 to 1.0): Matches user's primary active goal domain
            double goalRelevance = (template.Domain == preferences.PrimaryGoalDomain) ? 1.0 : 0.4;

            // 2b. Feasibility (0.0 to 1.0): Aligns with starting baseline (Starter vs Advanced)
            double feasibility = CalculateFeasibilityScore(template.Difficulty, preferences.CurrentBaselineLevel);

            // 2c. Barrier Fit (0.0 to 1.0): If past barriers were "too hard" or "wrong time", boost shorter/flexible tasks
            double barrierFit = CalculateBarrierFitScore(template, recentBarriers);

            // 2d. User Preference (0.0 to 1.0): Matches available time window and preferred equipment
            double userPref = CalculatePreferenceScore(template, preferences);

            // 2e. Novelty vs Continuity (0.0 to 1.0): Prevent task repetition on consecutive days
            double novelty = CalculateNoveltyScore(template.Id, recentHistory);

            // STEP 3: Composite Transparent Score Calculation
            double totalScore = (goalRelevance * WeightGoalRelevance)
                              + (feasibility * WeightFeasibility)
                              + (barrierFit * WeightBarrierFit)
                              + (userPref * WeightUserPreference)
                              + (novelty * WeightContinuityNovelty);

            eligibleCandidates.Add(new ScoredTaskCandidate(template, totalScore));
        }

        // Return candidates ordered by highest score descending
        return eligibleCandidates.OrderByDescending(c => c.Score);
    }
}
```

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Personalized Daily Action Card View (Priority: P1)

As an active Gibo user opening the app in the morning,
I want to see my customized daily plan containing one to three small, clear actions with explicit targets, estimated duration, evidence mode, and a "Why this fits you" explanation,
So that I know exactly what to do without cognitive overload or confusion.

**Why this priority**: The daily plan is the primary engagement surface of the application. It translates abstract intentions into concrete micro-actions.

**Independent Test**: Can be tested independently by creating a user with a "Movement" goal and viewing the home screen; verifying that 1-3 cards appear with clear action verbs (e.g., "Take a 7-minute indoor walk after lunch"), explicit targets (7 minutes), and contextual rationale.

**Acceptance Scenarios**:

1. **Given** a user with a preference cap of 2 daily actions,  
   **When** the daily plan is generated for the current calendar date,  
   **Then** exactly 2 distinct action cards appear on the home screen, each displaying a clear verb, measurable target, estimated time, and evidence mode.
2. **Given** an action card on the home screen,  
   **When** the user taps the card or the information icon,  
   **Then** an explanation appears detailing why Gibo recommended this task (e.g., "Fits your 10-minute afternoon break and preference for indoor activity").
3. **Given** a user with low baseline confidence or returning from a lapse,  
   **When** the plan is calculated,  
   **Then** the engine respects the principle of "Feasibility before ambition" and defaults to exactly 1 micro-task (5-7 minutes duration).

---

### User Story 2 - Dual-Mode Completion: Manual Parity & Health Connect (Priority: P1)

As a user who has completed a daily micro-action,
I want to record completion either via a single manual tap or automatically via background Health Connect step synchronization,
So that my effort is recognized immediately and awarded equal XP regardless of whether I wear a smartwatch.

**Why this priority**: Core to Principle V (Equity by Design). Wearable users enjoy passive convenience, but manual check-in users receive equal status and rewards.

**Independent Test**: Can be tested by manually completing a 7-minute walk and verifying 10 XP is awarded with evidence source "Manual"; then simulating Health Connect syncing 1,200 steps and verifying the corresponding step task completes with evidence source "HealthConnect" and awards the same 10 XP.

**Acceptance Scenarios**:

1. **Given** an active daily action card,  
   **When** the user taps "Complete",  
   **Then** the task state transitions to `Completed`, a confirmation animation plays, 10 XP is credited, and the completion event records `evidenceSource: "Manual"` with a UTC timestamp.
2. **Given** a user who has granted Health Connect step permissions,  
   **When** Health Connect records steps exceeding the task target within the scheduled window,  
   **Then** the background synchronization service marks the task as `Completed`, credits 10 XP, and records `evidenceSource: "HealthConnect"`.
3. **Given** a multi-step task supporting partial milestones (e.g., drink 2 glasses of water, 1 completed),  
   **When** the user marks 1 item,  
   **Then** the task state transitions to `Partial`, awarding 5 XP without misrepresenting total completion.

---

### User Story 3 - User-Driven Plan Adjustments: Edit, Swap & Pause (Priority: P2)

As a user whose daily circumstances have unexpectedly changed,
I want to reduce a task's target, swap it for an alternative, or pause a domain for the day,
So that the plan bends to my day rather than breaking my routine.

**Why this priority**: Preserves user agency. A rigid plan forces skips; an adjustable plan keeps the user actively engaged.

**Independent Test**: Can be tested by selecting "Swap" on an indoor walk task, selecting a 5-minute seated mobility alternative, and verifying the replacement task updates the active plan and records the swap event for future ranking refinement.

**Acceptance Scenarios**:

1. **Given** an active task card,  
   **When** the user selects "Reduce target",  
   **Then** the system presents a smaller approved variation (e.g., reducing from 10 minutes to 5 minutes) and updates the task instance accordingly.
2. **Given** an active task card,  
   **When** the user selects "Swap task",  
   **Then** the app displays 2-3 alternative approved candidates from the same domain, allowing the user to substitute the task without penalty.
3. **Given** a user override (edit or swap),  
   **When** subsequent background synchronization runs,  
   **Then** the engine MUST NOT silently revert or overwrite the user's manual adjustments.

---

### User Story 4 - Offline Resilience & Idempotent Synchronization (Priority: P2)

As a commuter or worker in an area with intermittent cellular coverage,
I want to open my daily plan, mark tasks complete, or record notes while offline,
So that my actions are safely queued and synchronized without data loss or duplicate records once my device reconnects.

**Why this priority**: Guarantees reliability and trust across real-world connectivity disruptions in transit or remote locations.

**Independent Test**: Can be tested by disconnecting device network, marking an action complete, re-establishing network connectivity, and verifying that the completion event is submitted with its client idempotency key and received exactly once by the backend.

**Acceptance Scenarios**:

1. **Given** a device with no active internet connection,  
   **When** the user launches Gibo,  
   **Then** the app renders today's cached plan from local encrypted storage without displaying blocking error dialogs.
2. **Given** an offline state,  
   **When** the user marks a task completed,  
   **Then** the UI updates optimistically, records the completion event in a local persistent queue with a unique `ClientEventId` (UUID) and timestamp.
3. **Given** queued offline completion events,  
   **When** network connectivity is restored,  
   **Then** the background synchronization client flushes the queue to `/api/v1/tasks/{id}/completion` with idempotency headers, ensuring duplicate requests are deduplicated and XP is credited exactly once.

---

## Edge Cases

- **Midnight Day Rollover**: If the user is viewing the app at 23:59 and the clock strikes 00:00, uncompleted tasks from yesterday transition to `Expired` (not "Failed"), and today's new plan is calculated based on current preferences.
- **Clock Drifts / Timezone Changes**: When a user travels (e.g., from Dubai to London), the mobile client detects the timezone change, prompts the user to confirm their local timezone, and shifts daily windows accordingly without corrupting past UTC timestamps.
- **Zero Eligible Tasks in Library**: If extreme user exclusion filters eliminate all templates in a domain, the engine falls back to an approved universal gentle reflection task ("Take 3 quiet mindful breaths") and displays a friendly notice suggesting relaxing one constraint.
- **Concurrent Completion from Sensor & Manual Tap**: If the user taps "Complete" at the exact moment Health Connect syncs step counts, the backend idempotency and optimistic locking mechanism detect the duplicate and return the existing completed task state without double-awarding XP.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST maintain a versioned `TaskTemplate` catalog containing curated, clinician-reviewed wellness actions across Movement, Sleep Routine, and Nutrition.
- **FR-002**: Every `TaskTemplate` MUST specify: `TaskId`, `Version`, `Domain`, `Instruction`, `TargetValue`, `MeasurementType` (Duration, Count, Boolean, Checklist), `EvidenceModes` (Manual, HealthConnect, or Both), `DifficultyLevel` (Starter, Standard, Advanced), `SafetyTags`, `RequiresOutdoor` (bool), `BCTCategory` (Behaviour Change Technique), and `SourceReference`.
- **FR-003**: The ranking engine MUST apply a deterministic scoring algorithm: Goal Relevance (30%) + Feasibility (25%) + Barrier Fit (20%) + User Preference (15%) + Continuity/Novelty (10%).
- **FR-004**: The system MUST apply hard boolean filters to eliminate contraindicated tasks before scoring; safety exclusions MUST NOT be implemented as soft score penalties.
- **FR-005**: The system MUST generate a `DailyPlan` for each active user containing between 1 and 3 `TaskInstance` items based on the user's configured `MaxDailyActions` setting.
- **FR-006**: Each generated `TaskInstance` MUST display a clear action verb, measurable target, estimated duration, evidence source indicator, and an explanatory "Why this fits" rationale.
- **FR-007**: The system MUST provide equal completion verification for manual one-tap check-offs and automated Health Connect step/activity syncing, awarding identical XP (10 XP) for full completion.
- **FR-008**: The system MUST support partial completion (awarding 5 XP) exclusively for task templates that explicitly define meaningful partial thresholds.
- **FR-009**: Users MUST be able to reduce task targets, swap a task for an approved domain alternative, or pause a domain for the day. User overrides MUST NOT be overwritten by automated engine recalculations.
- **FR-010**: The mobile client MUST cache daily plans locally, allow offline completion and event recording, and synchronize via HTTP POST with `Idempotency-Key` headers when network connectivity returns.
- **FR-011**: The system MUST persist every completion event in a `CompletionEvent` table containing `EventId`, `TaskInstanceId`, `State`, `EvidenceSource` (Manual/HealthConnect), `MeasuredValue`, `Timestamp` (UTC), and `SyncStatus`.

### Key Entities

- **TaskTemplate**: Master content record. Attributes: `TaskTemplateId` (UUID), `Version` (int), `Domain` (Enum), `Instruction` (localized string), `TargetValue` (int/float), `MeasurementType` (Enum), `AllowedEvidenceModes` (Flag Enum), `Difficulty` (Starter, Standard, Advanced), `SafetyTags` (string array), `RequiresOutdoor` (bool), `SourceId` (FK).
- **DailyPlan**: The container of actions for a given calendar day. Attributes: `PlanId` (UUID), `UserId` (FK), `Date` (DateOnly), `AlgorithmVersion` (string), `CreatedAt` (UTC), `UserEditsApplied` (bool).
- **TaskInstance**: An instantiated task scheduled for a specific day. Attributes: `TaskInstanceId` (UUID), `PlanId` (FK), `TaskTemplateId` (FK), `Target` (int/float), `ScheduledTimeWindow` (TimeSpan nullable), `State` (Scheduled, Completed, Partial, Skipped, Expired), `CompletedAt` (UTC nullable).
- **CompletionEvent**: Auditable proof of action execution. Attributes: `EventId` (UUID), `TaskInstanceId` (FK), `State` (Completed/Partial), `EvidenceSource` (Enum: Manual, HealthConnect), `MeasuredValue` (decimal nullable), `RecordedAt` (UTC), `SyncedAt` (UTC).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Today's plan generates and renders in under 500ms on the mobile app upon launch.
- **SC-002**: 100% of generated task instances originate strictly from the reviewed `TaskTemplate` catalog; zero runtime generative AI hallucinations in task creation.
- **SC-003**: 100% of offline completion events sync successfully upon reconnection with zero duplicate XP transactions or orphaned records.
- **SC-004**: Manual completion check-ins and Health Connect synced check-ins show 100% XP award parity (10 XP each).
- **SC-005**: More than 80% of daily actions generated for users are rated "manageable" or "easy enough" in feasibility feedback.

---

## Assumptions

- Health Connect is available on Android 9+ devices with Google Play services; devices without Health Connect automatically use the manual completion path without degraded app utility.
- The task catalog contains at least 30 reviewed starter and standard templates across Movement, Nutrition, and Sleep Routines.
- Weather/temperature conditions in Abu Dhabi during peak midday hours require indoor walk/stretching alternatives for all movement goals.
