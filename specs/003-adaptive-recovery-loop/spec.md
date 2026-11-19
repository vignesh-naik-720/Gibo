# Feature Specification: Adaptive Recovery Loop & Disruption Handling

**Feature Branch**: `003-adaptive-recovery-loop`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "The core product differentiator: an empathetic, closed-loop recovery system for missed actions and life disruptions. Detects friction; prompts non-judgmental barrier check-ins across key friction categories (timing, difficulty, low energy, access, memory, health); adapts subsequent plans with smaller targets, time shifts, indoor weather options, or temporary pauses; handles guilt-free return-after-gap re-entry without lecturing; provides non-punitive reflection rewards; and safely redirects potential clinical or emergency concerns away from coaching."

---

## Code Concepts & Architectural Commentary for Programmers

```csharp
// ==============================================================================================
// ARCHITECTURAL CONCEPT: The Adaptive Recovery State Machine & Barrier Resolution Engine
// This feature implements Gibo's core innovation: transforming a missed action from a "failure"
// into a learning signal. In traditional habit apps, a missed day breaks a streak and damages
// motivation. In Gibo, a missed action triggers the Recovery Loop:
//
//   [Task Missed / Skipped] 
//          │
//          ▼
//   [Empathetic Friction Check-in] ──(User chooses barrier)──► [Adaptation Decision Matrix]
//          │                                                               │
//          ▼                                                               ▼
//   [Reflection Reward: +2 XP]                                    [Modified Next Plan]
//                                                            (Smaller / Alternate / Paused)
//
// In adherence to Principle II (Recovery Before Streaks), no XP is deducted and no streaks are reset.
// If the barrier indicates a clinical or emergency issue ("unwell", "chest pain", "fever"), the
// coaching engine halts and immediately routes to local medical resources.
// ==============================================================================================

namespace Gibo.Application.Recovery.Services;

public sealed class RecoveryAdaptationService : IRecoveryAdaptationService
{
    private readonly ITaskTemplateRepository _templateRepository;
    private readonly IPlanRepository _planRepository;

    public async Task<PlanAdaptationProposal> EvaluateBarrierAndProposeAdjustmentAsync(
        UserId userId,
        TaskInstanceId taskInstanceId,
        BarrierCategory barrierCategory,
        string? userNote,
        CancellationToken ct)
    {
        // 1. SAFETY INTERCEPT: If the barrier reports acute health distress, halt coaching immediately.
        if (barrierCategory == BarrierCategory.UnwellOrPain)
        {
            return PlanAdaptationProposal.CreateSafetyHalt(
                safetyMessage: "Your wellbeing comes first. Coaching has been paused. If you are experiencing concerning symptoms, please contact your doctor or call 998 (Abu Dhabi Emergency Services).",
                requiresMedicalReferral: true);
        }

        // 2. Fetch the target task instance and historical barrier frequency
        var taskInstance = await _planRepository.GetTaskInstanceByIdAsync(taskInstanceId, ct);
        
        // 3. Adaptation Decision Matrix:
        // Map the real-life barrier to a targeted, friction-lowering modification:
        return barrierCategory switch
        {
            // Barrier: "Timing did not work" / "Shift changed"
            // Adaptation: Shift the scheduled time window or offer an asynchronous micro-action.
            BarrierCategory.TimingConflict => new PlanAdaptationProposal(
                AdaptationType.RescheduleWindow,
                suggestedChange: "Shift action to evening wind-down or morning routine"),

            // Barrier: "Too difficult" / "Low stamina"
            // Adaptation: Step down to a 50% target or starter template (Graded Task BCT).
            BarrierCategory.TooDifficult => new PlanAdaptationProposal(
                AdaptationType.ReduceTargetSize,
                suggestedChange: $"Step down target from {taskInstance.Target} to {Math.Max(3, taskInstance.Target / 2)} minutes"),

            // Barrier: "Low energy / Exhausted"
            // Adaptation: Propose a 3-minute gentle recovery or breathing micro-action.
            BarrierCategory.LowEnergy => new PlanAdaptationProposal(
                AdaptationType.ReplaceWithLowEnergyAlternative,
                suggestedChange: "Switch to gentle 3-minute seated stretching or pacing"),

            // Barrier: "No access / Too hot outside"
            // Adaptation: Switch instantly to an indoor cooling alternative.
            BarrierCategory.AccessOrWeather => new PlanAdaptationProposal(
                AdaptationType.ConvertToIndoorAlternative,
                suggestedChange: "Replace outdoor route with living room or indoor mall walking"),

            // Barrier: "Life paused / Extended absence"
            // Adaptation: Pause active expectations without penalty; suggest gentle restart.
            BarrierCategory.NeedsPause => new PlanAdaptationProposal(
                AdaptationType.TemporaryPause,
                suggestedChange: "Pause plan for 48 hours; return whenever you are ready"),

            _ => new PlanAdaptationProposal(
                AdaptationType.MaintainCurrentPlan,
                suggestedChange: "Keep current plan; Gibo will monitor ease over the next 2 days")
        };
    }
}
```

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Gentle Barrier Check-in After a Missed Action (Priority: P1)

As a user who missed a scheduled daily action due to a hectic workday,
I want Gibo to greet me with a friendly, non-punitive prompt asking what got in the way,
So that I can reflect without guilt, earn reflection credit, and receive an immediate adjustment that fits my reality.

**Why this priority**: The barrier check-in is the entry point to the recovery engine. It prevents user disengagement and captures real behavioral friction.

**Independent Test**: Can be tested independently by allowing a scheduled task to lapse, opening the app, confirming that the prompt "Did this fit today?" appears, selecting "Timing did not work", and verifying that 2 XP is awarded for honest reflection and an adjusted time slot is offered.

**Acceptance Scenarios**:

1. **Given** a user who did not complete a scheduled micro-action yesterday,  
   **When** they open the app today,  
   **Then** the app displays an empathetic check-in: *"Looks like this plan may not have fitted your day. That happens. What got in the way?"* with zero streak reset warnings and zero XP deductions.
2. **Given** the barrier prompt,  
   **When** the user reviews the options,  
   **Then** the app presents structured friction categories: *"Timing / Too busy"*, *"Felt too hard"*, *"Low energy"*, *"Access / Weather (Too hot)"*, *"Forgot"*, *"Unwell"*, and *"Prefer not to say"*.
3. **Given** the selection of a barrier,  
   **When** the user taps submit,  
   **Then** the system logs the `BarrierFeedback` record, credits +2 XP for reflection, and displays one concrete adaptation proposal (e.g., *"Would a 5-minute version or a different time work better?"*).

---

### User Story 2 - Dynamic Plan Adaptation & Acceptance (Priority: P1)

As a user who indicated a specific barrier,
I want Gibo to propose a tailored adjustment to my upcoming plan that directly removes that friction,
So that I can resume progress at a comfortable pace with a single tap.

**Why this priority**: Without actionable adaptation, barrier data collection is useless. Adapting the plan restores the user's belief in their ability to succeed.

**Independent Test**: Can be tested by submitting "Felt too hard" on a 15-minute walk, receiving a proposal to step down to a 7-minute indoor walk, tapping "Accept", and verifying the new task instance is generated for today with the adjusted target.

**Acceptance Scenarios**:

1. **Given** a user barrier indicating "Felt too difficult",  
   **When** the adaptation engine processes the feedback,  
   **Then** it generates a smaller target proposal (e.g., reduces duration from 15 minutes to 7 minutes) with options: "Accept Adjustment", "Customize Target", or "Keep Original".
2. **Given** a user barrier indicating "Access / Weather (Too hot outside)",  
   **When** the adaptation engine processes the feedback,  
   **Then** it immediately replaces the outdoor walking template with an approved indoor alternative (e.g., indoor pacing, stairs in building, or mall walking).
3. **Given** an adaptation proposal,  
   **When** the user taps "Accept Adjustment",  
   **Then** today's plan is updated immediately with the adapted task, and the user's preference weights are updated to favor lower friction in subsequent plan generations.

---

### User Story 3 - Guilt-Free Re-entry After an Extended Gap (Priority: P2)

As a user returning to Gibo after a two-week absence,
I want the app to welcome me back warmly without lectures, unread notification backlogs, or lost progress history,
So that I feel encouraged to restart rather than ashamed of being away.

**Why this priority**: Habit abandonment typically happens after a 7-14 day break. Warm re-entry eliminates restart anxiety.

**Independent Test**: Can be tested by simulating a 14-day inactivity window, launching the app, verifying that a "Welcome Back" screen appears with zero penalty notices, preserving all previously earned XP, and offering a gentle "starter restart" size.

**Acceptance Scenarios**:

1. **Given** a user who has not opened the app for 10 or more consecutive days,  
   **When** they launch the app,  
   **Then** the system displays a welcoming, lecture-free screen: *"Welcome back! Life gets full. Your past effort and XP are right where you left them. Ready for a small restart?"*
2. **Given** the return-after-gap flow,  
   **When** the user chooses to restart,  
   **Then** the app offers them a choice of restart size: *"Gentle (1 small 5-minute action)"*, *"Standard (2 actions)"*, or *"Gibo Suggests"*, defaulting to the lowest friction option.
3. **Given** the restart confirmation,  
   **When** today's plan renders,  
   **Then** all historical completed actions, past XP, and previous weekly styles remain intact in the user's profile history.

---

### User Story 4 - Safety Escalation for Reported Health Concerns (Priority: P1)

As a user who reports being unwell, in pain, or experiencing concerning physical symptoms,
I want Gibo to immediately halt coaching, suppress physical activity tasks, and provide emergency and clinical contact resources,
So that my health and safety are protected without risky automated advice.

**Why this priority**: Mandatory adherence to Principle III and clinical safety guardrails. An AI coaching app must never push exercise or lifestyle changes on a sick or injured user.

**Independent Test**: Can be tested by selecting "Unwell" or entering "Severe chest pain during walk" in the barrier note, verifying that all coaching tasks are instantly suspended, a prominent emergency care banner appears with Abu Dhabi numbers (998 / 800-50), and zero gamification rewards are shown.

**Acceptance Scenarios**:

1. **Given** the barrier check-in screen,  
   **When** the user selects the "Unwell" category,  
   **Then** the app immediately responds: *"Your body needs rest right now. Active coaching is paused. Take all the time you need to recover."*
2. **Given** a barrier note or symptom disclosure containing red-flag keywords (e.g., chest pain, shortness of breath, dizziness),  
   **When** the safety interceptor processes the text,  
   **Then** it instantly replaces the daily plan with an emergency guidance card displaying official UAE medical contact channels (Abu Dhabi Emergency 998, Teleconsultation 800-50) and advises seeking urgent in-person medical evaluation.
3. **Given** a safety pause state,  
   **When** the user returns to the app in subsequent days,  
   **Then** the app requires the user to confirm they are feeling well and ready to resume before reactivating normal movement tasks.

---

## Edge Cases

- **User repeatedly skips without selecting a barrier**: If a user skips tasks 3 days in a row and ignores the barrier check-in prompt, the engine automatically steps down the daily action cap to 1 micro-task (5 minutes) and silently reduces difficulty, avoiding repeated nagging banners.
- **User rejects all proposed adaptations**: If the user declines the proposed smaller task and does not want an alternative, the app offers a "Take a 24-Hour Rest Day" option that pauses the schedule and preserves progress.
- **Intermittent network during barrier submission**: Barrier feedback and adaptation selections are cached in local SQLite/AsyncStorage and synchronized as an atomic unit when connection resumes.
- **User inputs ambiguous distress in note field**: If sentiment/classification analysis cannot rule out injury or severe illness, the system errs on the side of caution by applying the safety pause and medical disclaimer.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST detect uncompleted tasks from preceding days and classify them as missed/lapsed without decrementing XP or resetting progress counters.
- **FR-002**: The system MUST trigger an optional, non-judgmental barrier check-in interface prompting the user to identify what got in the way.
- **FR-003**: The barrier check-in MUST support predefined categories: `TimingConflict`, `TooDifficult`, `LowEnergy`, `AccessOrWeather`, `Forgot`, `UnwellOrPain`, and `PreferNotToSay`.
- **FR-004**: The system MUST award exactly 2 XP for submitting a barrier check-in to reward honest reflection and engagement with the adaptation process.
- **FR-005**: The system MUST evaluate the selected barrier through an `AdaptationDecisionMatrix` and present at least one concrete friction-lowering modification (reduce duration, change timing, indoor alternative, or temporary pause).
- **FR-006**: The user MUST be able to accept, edit, or decline the proposed adaptation. When accepted, today's `DailyPlan` MUST be updated immediately.
- **FR-007**: When an absence of 7 or more days is detected, the system MUST display a welcoming, lecture-free re-entry flow that restores the plan at a starter level (1 small action) while preserving 100% of historical XP.
- **FR-008**: The system MUST intercept any barrier submission categorized as `UnwellOrPain` or containing red-flag symptoms, immediately halt active physical task coaching, and display localized UAE emergency services (998) and medical teleconsultation guidance.
- **FR-009**: The system MUST record every barrier submission in a `BarrierFeedback` entity containing `TaskInstanceId`, `Category`, `UserNote` (optional), `SubmittedAt` (UTC), and `AdaptationApplied`.
- **FR-010**: All barrier notes MUST be encrypted at rest and sanitized to prevent SQL injection or cross-site scripting vulnerabilities.

### Key Entities

- **BarrierFeedback**: Represents the user's explanation for a lapse. Attributes: `FeedbackId` (UUID), `TaskInstanceId` (FK), `UserId` (FK), `Category` (Enum), `UserNote` (string encrypted nullable), `SubmittedAt` (UTC), `SelectedAdaptation` (Enum).
- **PlanAdaptation**: Represents a generated adjustment proposal. Attributes: `AdaptationId` (UUID), `FeedbackId` (FK), `UserId` (FK), `OriginalTarget` (int/float), `AdaptedTarget` (int/float), `Type` (TargetReduced, TimeShifted, IndoorAlternative, SafetyPause), `Status` (Proposed, Accepted, Rejected), `CreatedAt` (UTC).
- **SafetyEscalationEvent**: Audit log for health concern interceptions. Attributes: `EscalationId` (UUID), `UserId` (FK), `TriggerSource` (Enum: BarrierNote, SymptomReport), `ActionTaken` (CoachingHalted, ReferralShown), `Timestamp` (UTC).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Lapse recovery rate reaches at least 65% (users who miss an action and complete a restart action within 7 days).
- **SC-002**: Median time-to-restart drops from an industry average of 14 days to under 3 days following plan adaptation.
- **SC-003**: 0% of barrier check-ins or lapse flows emit punitive language, streak reset notifications, or XP deductions.
- **SC-004**: 100% of reported acute health concerns (`UnwellOrPain`) trigger the safety halt and emergency referral banner within 100ms.
- **SC-005**: At least 75% of users who encounter the barrier check-in accept the suggested plan adaptation.

---

## Assumptions

- Users may experience variable work schedules (e.g., rotating shifts or extended hours) and extreme summer temperatures in Abu Dhabi that necessitate frequent indoor activity substitutions.
- Barrier notes are optional; users are never forced to type text to receive a plan adaptation.
- Local Abu Dhabi emergency numbers (998 for Ambulance, 800-50 for SEHA Teleconsultation) are standard and up-to-date.
