# Feature Specification: Purposeful Gamification & Weekly Gibo Styles

**Feature Branch**: `004-gamification-and-weekly-styles`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "XP with a purpose; strict non-punitive reward ledger awarding 10 XP for full completion (manual or Health Connect parity), 5 XP for explicit partial completion, 2 XP for barrier reflection, and zero deductions for skips or pauses; private weekly milestones without public leaderboards or biometric scoring; non-clinical, descriptive weekly Gibo styles ('Your Gibo style this week') celebrating diverse positive patterns (Steady Builder, Curious Explorer, Comeback Champion, Mindful Balancer, Momentum Maker) with option to hide; and monthly trend insights separating sensor-measured data from self-reported actions without overclaiming health improvements."

---

## Code Concepts & Architectural Commentary for Programmers

```csharp
// ==============================================================================================
// ARCHITECTURAL CONCEPT: Immutable XP Transaction Ledger & Non-Clinical Style Classifier
// In strict adherence to Principle I (User Agency) and Principle V (Equity), XP is never treated
// as a mutable balance variable vulnerable to race conditions or arbitrary deductions.
// Instead, it is an append-only ledger of immutable XpTransaction records:
//   - Balance = Sum(XpTransaction.Amount)
//   - Amount is ALWAYS > 0. Decrements, resets, or streak "penalties" are programmatically banned.
//
// The weekly style algorithm evaluates constructive behavioral signals over a 7-day rolling window:
//   - Steady Builder: Consistent completions across >= 4 days.
//   - Curious Explorer: Used >= 2 task swaps or varied action domains.
//   - Comeback Champion: Successfully completed an action after a missed day / barrier.
//   - Mindful Balancer: Scheduled rest, sleep routines, or pacing actions consistently.
//   - Momentum Maker: Feasibility rating or completion rate exceeded prior baseline.
// Any judgmental, medical, or body-shaming terminology is strictly banned at domain level.
// ==============================================================================================

namespace Gibo.Domain.Aggregates.GamificationAggregate;

public sealed class XpTransaction : Entity<TransactionId>
{
    public UserId UserId { get; private set; }
    public int Amount { get; private set; } // Enforced: Must be > 0
    public XpReason Reason { get; private set; }
    public string? ReferenceId { get; private set; } // TaskInstanceId or FeedbackId
    public DateTimeOffset CreatedAt { get; private set; }

    private XpTransaction() { }

    public static XpTransaction CreateAward(
        UserId userId, 
        int amount, 
        XpReason reason, 
        string? referenceId = null)
    {
        // Guardrail: Under no circumstances can negative XP be awarded or deducted
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "XP award must be strictly positive. Deductions are prohibited.");

        return new XpTransaction
        {
            Id = TransactionId.New(),
            UserId = userId,
            Amount = amount,
            Reason = reason,
            ReferenceId = referenceId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

public sealed class WeeklyStyleClassifier : IWeeklyStyleClassifier
{
    public WeeklyStyleResult ClassifyStyle(
        IReadOnlyList<TaskInstance> weekTasks, 
        IReadOnlyList<BarrierFeedback> weekBarriers,
        IReadOnlyList<CompletionEvent> weekCompletions)
    {
        // Signal 1: Comeback Champion (Recovered after a barrier or pause)
        if (weekBarriers.Any() && weekCompletions.Any(c => c.RecordedAt > weekBarriers.Max(b => b.SubmittedAt)))
        {
            return new WeeklyStyleResult(
                WeeklyStyleType.ComebackChampion,
                Title: "Comeback Champion",
                Encouragement: "You found a way back after a pause. Progress survives real life.");
        }

        // Signal 2: Curious Explorer (Engaged with diverse task types or swapped templates)
        var distinctTemplates = weekTasks.Select(t => t.TaskTemplateId).Distinct().Count();
        if (distinctTemplates >= 3)
        {
            return new WeeklyStyleResult(
                WeeklyStyleType.CuriousExplorer,
                Title: "Curious Explorer",
                Encouragement: "You experimented with different actions and found what fits your week.");
        }

        // Signal 3: Mindful Balancer (Consistent rest, sleep routines, or pacing)
        var sleepOrRestCount = weekCompletions.Count(c => c.IsSleepOrRestAction);
        if (sleepOrRestCount >= 3)
        {
            return new WeeklyStyleResult(
                WeeklyStyleType.MindfulBalancer,
                Title: "Mindful Balancer",
                Encouragement: "You prioritized rest, pacing, and sleep routines to sustain your energy.");
        }

        // Signal 4: Steady Builder (Regular rhythm of small daily completions)
        var distinctDays = weekCompletions.Select(c => c.RecordedAt.Date).Distinct().Count();
        if (distinctDays >= 4)
        {
            return new WeeklyStyleResult(
                WeeklyStyleType.SteadyBuilder,
                Title: "Steady Builder",
                Encouragement: "You kept returning to your small routine across the week.");
        }

        // Default Encouraging Style: Momentum Maker
        return new WeeklyStyleResult(
            WeeklyStyleType.MomentumMaker,
            Title: "Momentum Maker",
            Encouragement: "Your routine is becoming easier to repeat one small step at a time.");
    }
}
```

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Earning XP for Effort & Honest Reflection (Priority: P1)

As a user engaging with my daily health actions and reflecting on challenges,
I want to earn XP for completing tasks (manual or Health Connect), for partial milestones, and for honest barrier check-ins,
So that my constructive effort is consistently validated without any fear of having XP taken away when life gets busy.

**Why this priority**: Motivates sustained engagement through positive reinforcement and psychological safety. Eliminates the anxiety associated with streak-loss penalties.

**Independent Test**: Can be tested by verifying: (1) completing a manual task adds 10 XP; (2) completing a Health Connect step task adds 10 XP; (3) completing a barrier reflection adds 2 XP; (4) skipping a task adds 0 XP and deducts 0 XP; and (5) total XP balance matches the sum of transaction records.

**Acceptance Scenarios**:

1. **Given** a user completing an action manually,  
   **When** the action is confirmed,  
   **Then** an `XpTransaction` of +10 XP is created, the user's total XP increases by 10, and an in-app celebration toast displays *"10 XP earned"*.
2. **Given** a task marked as partial (e.g., 1 of 2 glasses of water),  
   **When** the user marks partial completion,  
   **Then** an `XpTransaction` of +5 XP is created, provided the underlying template explicitly allows partial credit.
3. **Given** a user submitting a barrier check-in (e.g., selecting "Too busy today"),  
   **When** the check-in is saved,  
   **Then** an `XpTransaction` of +2 XP is awarded with reason `ReflectionReward`, validating honest self-awareness.
4. **Given** a user who skips a task or misses an entire week,  
   **When** the app reviews their record,  
   **Then** zero XP is deducted, zero penalties are assessed, and no streak break notifications are generated.

---

### User Story 2 - Viewing "Your Gibo Style This Week" (Priority: P2)

As an active user opening the app at the end of the week,
I want to view a personalized, descriptive summary of my habit pattern ("Your Gibo style this week") with positive wording,
So that I feel understood and recognized for my personal style without feeling clinically categorized or judged.

**Why this priority**: Builds emotional connection and identity-based habit formation without relying on stigmatizing labels or rigid performance metrics.

**Independent Test**: Can be tested by completing actions across 4 days in a synthetic week, viewing the weekly summary tab, verifying that "Steady Builder" is displayed with the text *"You kept returning to your routine"*, and verifying a toggle exists in settings to hide the style card if desired.

**Acceptance Scenarios**:

1. **Given** the weekly summary screen on Sunday evening,  
   **When** the user navigates to the Weekly Insight tab,  
   **Then** the header reads *"Your Gibo style this week: Steady Builder"* with the caption *"You kept returning to your routine."*
2. **Given** a user who experienced a lapse and accepted an adaptation on Friday,  
   **When** the style classifier runs,  
   **Then** it classifies the style as *"Comeback Champion"* with the text *"You found a way back after a pause."*
3. **Given** the settings menu,  
   **When** a user toggles *"Hide Weekly Style"*,  
   **Then** the style badge and title are hidden from their dashboard, leaving only plain behavior metrics.
4. **Given** the classification engine,  
   **When** generating summaries,  
   **Then** it is programmatically barred from outputting negative, medical, or shaming terms such as "lazy", "failed", "unmotivated", "sedentary", or "athlete".

---

### User Story 3 - Monthly Trend Insights with Data Provenance Separation (Priority: P2)

As a health-conscious resident reviewing my monthly progress,
I want to see trend lines of my completed movement, sleep routine, and nutrition actions, with clear separation between sensor-recorded data and self-reports,
So that I can see my actual consistency over time without false clinical promises.

**Why this priority**: Satisfies the hackathon core evaluation criterion for sustained outcome measurement while maintaining complete honesty regarding data provenance.

**Independent Test**: Can be tested by generating 30 days of mixed synthetic data (manual walks, Health Connect steps, sleep cues), opening the monthly insights screen, and verifying that sensor steps and manual check-offs are plotted as separate, clearly labeled series with neutral descriptive copy (e.g., "Completed on 3 days per week on average").

**Acceptance Scenarios**:

1. **Given** the monthly insights screen,  
   **When** viewing the Movement domain chart,  
   **Then** the visualization clearly distinguishes between *"Steps from Health Connect"* and *"Self-reported walk completions"*, displaying data provenance legends.
2. **Given** any monthly summary insight card,  
   **When** presenting summary text,  
   **Then** the copy uses objective observational language (e.g., *"You completed this action on 4 days per week on average"*) and never claims disease risk reduction, weight loss, or clinical biomarker improvement.
3. **Given** a user who has completely disabled wearable permissions,  
   **When** viewing monthly insights,  
   **Then** all charts display their self-reported action consistency with full visual elegance and zero empty-state warnings prompting them to buy a smartwatch.

---

## Edge Cases

- **User toggles gamification off entirely**: If a user prefers an intrinsic, distraction-free experience, a setting toggle disables XP badges, animations, and sound effects across the app while still maintaining underlying event logs for weekly reviews.
- **Tied Style Scores**: If a user qualifies equally for "Steady Builder" and "Comeback Champion" in the same week, the engine prioritizes "Comeback Champion" to celebrate recovery resilience.
- **Partial week onboarding**: If a user joins on a Thursday, their first Sunday recap calculates rates based only on the 3 active days rather than penalizing them for the preceding 4 inactive days.
- **Device clock manipulation**: XP transactions rely on UTC server timestamps; manipulating the local device clock cannot generate duplicate daily XP rewards.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST implement an append-only `XpTransaction` ledger where every entry has an amount strictly greater than zero; deductions, negative balances, and streak resets are prohibited.
- **FR-002**: The system MUST award exactly 10 XP for a completed task, regardless of whether evidence was submitted manually or synchronized via Health Connect.
- **FR-003**: The system MUST award exactly 5 XP for partial completion only when the corresponding `TaskTemplate` explicitly supports partial milestones.
- **FR-004**: The system MUST award exactly 2 XP for completing a barrier reflection check-in or plan adjustment.
- **FR-005**: The system MUST allow users to disable gamification elements in settings; when disabled, XP counters and toasts are hidden from the UI.
- **FR-006**: The system MUST evaluate weekly behavior signals and assign a descriptive, temporary "Gibo style this week" chosen from: `SteadyBuilder`, `CuriousExplorer`, `ComebackChampion`, `MindfulBalancer`, and `MomentumMaker`.
- **FR-007**: The system MUST prohibit the generation of stigmatizing, diagnostic, or punitive persona classifications.
- **FR-008**: The system MUST allow users to hide or dismiss the weekly style feature from their dashboard.
- **FR-009**: The system MUST provide monthly trend visualizations that explicitly separate sensor-derived metrics from self-reported checklist completions.
- **FR-010**: The system MUST state behavior trends using objective frequency metrics (e.g., "Active on 4 out of 7 days") and MUST NOT claim clinical efficacy, diagnostic status, or medical outcomes.
- **FR-011**: All XP and milestone data MUST remain strictly private to the user account; the system MUST NOT publish public leaderboards or share scores with employers, insurers, or third parties.

### Key Entities

- **XpTransaction**: Immutable ledger entry. Attributes: `TransactionId` (UUID), `UserId` (FK), `Amount` (int > 0), `Reason` (Enum: TaskCompleted, PartialCompleted, ReflectionReward, WeeklyMilestone), `ReferenceId` (string nullable), `CreatedAt` (UTC).
- **WeeklyInsight**: Aggregated weekly summary. Attributes: `InsightId` (UUID), `UserId` (FK), `WeekStartDate` (DateOnly), `ActionsPlanned` (int), `ActionsCompleted` (int), `BarriersLogged` (int), `RecoveryRate` (decimal), `AssignedStyle` (Enum), `SummaryText` (localized string), `IsHiddenByUser` (bool).
- **MonthlyMetricAggregate**: Long-term trend record. Attributes: `AggregateId` (UUID), `UserId` (FK), `MonthYear` (string: YYYY-MM), `Domain` (Enum), `SensorMeasuredUnits` (int nullable), `SelfReportedCompletions` (int), `DaysActiveCount` (int).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 0% of user profiles experience an XP deduction, negative adjustment, or punitive streak reset under any circumstance.
- **SC-002**: 100% of weekly styles use positive, descriptive terminology aligned with approved style templates.
- **SC-003**: 100% of monthly trend screens distinctly render manual check-in series and wearable sensor series with clear provenance labels.
- **SC-004**: 90% of pilot participants report that the weekly Gibo style felt encouraging and non-judgmental.
- **SC-005**: 100% of gamification and milestone data are verified to be private, with zero exposure on network APIs to unauthorized third parties.

---

## Assumptions

- XP is an intrinsic engagement motivator within Gibo and carries no monetary or healthcare-reimbursement value.
- Weekly insights are calculated in the background on Sunday nights according to the user's registered local timezone.
- The UI gracefully renders both compact mobile charts and accessible table views for screen readers.
