// ==============================================================================================
// ARCHITECTURAL CONCEPT: Plan Enums & Measurement Invariants
// Governs task execution types, evidence verification sources, and state transitions.
// Enforces Principle V (Manual and Wearable Device Equality) by explicitly treating
// manual confirmation and passive wearable synchronization as equal evidence sources.
// ==============================================================================================

namespace Gibo.Domain.Aggregates.PlanAggregate;

/// <summary>
/// Defines how a micro-action's target is measured and tracked.
/// </summary>
public enum MeasurementType
{
    DurationMinutes = 1, // e.g. 10 minutes walking or stretching
    StepCount = 2,       // e.g. 1,000 steps
    Repetitions = 3,     // e.g. 10 chair squats
    BooleanCheck = 4,    // e.g. "Dim lights 1 hour before bed"
    ServingCount = 5     // e.g. 2 glasses of water
}

/// <summary>
/// Bitwise or categorical modes of verification allowed for a task template.
/// </summary>
public enum EvidenceMode
{
    Manual = 1,        // One-tap checkbox in mobile client
    HealthConnect = 2, // Background step/exercise sync from Android Health Connect
    Both = 3           // Can be fulfilled via either manual tap or automated sensor sync
}

/// <summary>
/// Cognitive and physical exertion tier of a task template.
/// Feasibility ranking prioritizes Starter tasks during lapse re-entry.
/// </summary>
public enum TaskDifficulty
{
    Starter = 1,   // 2-5 minutes, minimal barrier, immediate feasibility
    Standard = 2,  // 10-15 minutes, baseline daily habit
    Advanced = 3   // 20+ minutes or higher intensity
}

/// <summary>
/// Finite state machine for a scheduled task instance in a daily plan.
/// </summary>
public enum TaskState
{
    Scheduled = 1, // Pending completion today
    Completed = 2, // Successfully completed (awards full 10 XP)
    Partial = 3,   // Part of the milestone fulfilled (awards 5 XP)
    Skipped = 4,   // Skipped by user without penalty; triggers barrier check-in
    Expired = 5,   // Day rollover completed without action (neutral transition, 0 XP lost)
    Paused = 6     // Domain paused by user or clinical safety halt
}

/// <summary>
/// Provenance indicator of how the task completion was recorded.
/// Principle V mandates that both sources award identical 10 XP.
/// </summary>
public enum CompletionEvidenceSource
{
    Manual = 1,
    HealthConnect = 2
}
