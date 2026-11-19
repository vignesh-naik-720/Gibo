namespace Gibo.Domain.Aggregates.UserAggregate;

/// <summary>
/// Granular consent categories adhering to UAE Federal Decree-Law No. 45 of 2021 (PDPL).
/// </summary>
public enum ConsentCategory
{
    CoreProfile = 1,          // Core account operations and plan feasibility
    HealthConnectSync = 2,    // Optional Android Health Connect step/activity read
    AssistantChatContext = 3, // Optional chat context retention for wellness assistant
    AnonymousResearch = 4     // Optional de-identified aggregated cohort research
}

public enum GoalDomain
{
    Movement = 1,
    SleepRoutine = 2,
    Nutrition = 3
}

public enum WorkSchedulePattern
{
    StandardDaytime = 1,
    ShiftWorker = 2,
    FlexibleOrIrregular = 3
}

public enum BaselineFitnessLevel
{
    Starter = 1,      // 5-10 minutes gentle movement
    Intermediate = 2, // 15-20 minutes standard daily activity
    Active = 3        // Regular physical exercise
}

public enum UserAccountStatus
{
    Active = 1,
    Suspended = 2,
    DeletionRequested = 3
}

public enum GoalStatus
{
    Active = 1,
    Paused = 2,
    Completed = 3
}
