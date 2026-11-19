using Gibo.Domain.Common;

namespace Gibo.Domain.Aggregates.UserAggregate;

/// <summary>
/// Domain Entity: UserPreference
/// Encapsulates lifestyle context, environmental constraints, quiet hours, and feasibility parameters.
/// Drives the rules-first task ranking algorithm to ensure tasks fit real schedules in Abu Dhabi.
/// </summary>
public sealed class UserPreference : Entity<PreferenceId>
{
    public UserId UserId { get; private set; }

    // Primary wellness domain (Movement, Sleep Routine, Nutrition)
    public GoalDomain PrimaryGoalDomain { get; private set; }

    // Shift or workday pattern (StandardDaytime, ShiftWorker, Flexible)
    public WorkSchedulePattern WorkPattern { get; private set; }

    // Gulf climate accommodation: True prioritizes indoor/air-conditioned activities
    public bool IndoorPreferred { get; private set; }

    // Daily action cap: Constrained strictly between 1 and 3 (Principle II: Feasibility before ambition)
    public int MaxDailyActions { get; private set; }

    // Time boundary when reminders and notifications are suppressed
    public TimeSpan QuietHoursStart { get; private set; }
    public TimeSpan QuietHoursEnd { get; private set; }

    // Starting activity comfort baseline
    public BaselineFitnessLevel BaselineLevel { get; private set; }

    // Accessibility considerations (e.g. "LowImpact", "SeatedOnly")
    public List<string> AccessibilityTags { get; private set; } = new();

    private UserPreference() { } // Required for EF Core

    public UserPreference(
        PreferenceId id,
        UserId userId,
        GoalDomain primaryGoalDomain,
        WorkSchedulePattern workPattern,
        bool indoorPreferred,
        int maxDailyActions,
        TimeSpan quietHoursStart,
        TimeSpan quietHoursEnd,
        BaselineFitnessLevel baselineLevel,
        IEnumerable<string>? accessibilityTags)
    {
        Id = id;
        UserId = userId;
        PrimaryGoalDomain = primaryGoalDomain;
        WorkPattern = workPattern;
        IndoorPreferred = indoorPreferred;
        // Enforce invariant: daily action cap must be between 1 and 3
        MaxDailyActions = Math.Clamp(maxDailyActions, 1, 3);
        QuietHoursStart = quietHoursStart;
        QuietHoursEnd = quietHoursEnd;
        BaselineLevel = baselineLevel;
        AccessibilityTags = accessibilityTags?.ToList() ?? new List<string>();
    }

    public static UserPreference CreateDefault(
        UserId userId,
        GoalDomain primaryGoalDomain = GoalDomain.Movement,
        WorkSchedulePattern workPattern = WorkSchedulePattern.StandardDaytime,
        bool indoorPreferred = true,
        int maxDailyActions = 2,
        BaselineFitnessLevel baselineLevel = BaselineFitnessLevel.Starter)
    {
        return new UserPreference(
            PreferenceId.New(),
            userId,
            primaryGoalDomain,
            workPattern,
            indoorPreferred,
            maxDailyActions,
            TimeSpan.FromHours(22),
            TimeSpan.FromHours(6),
            baselineLevel,
            new List<string>());
    }

    public void UpdateCap(int newCap)
    {
        MaxDailyActions = Math.Clamp(newCap, 1, 3);
    }
}
