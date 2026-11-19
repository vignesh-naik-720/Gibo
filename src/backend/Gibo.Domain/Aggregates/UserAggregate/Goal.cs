using Gibo.Domain.Common;

namespace Gibo.Domain.Aggregates.UserAggregate;

/// <summary>
/// Domain Entity: Goal
/// Captures the user's active habit intention in Movement, Sleep, or Nutrition.
/// Avoids weight loss or caloric restriction as default goals (Principle I & Principle III).
/// </summary>
public sealed class Goal : Entity<GoalId>
{
    public UserId UserId { get; private set; }
    public GoalDomain Domain { get; private set; }
    public string DesiredOutcome { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public GoalStatus Status { get; private set; }

#pragma warning disable CS8618
    private Goal() { } // Required for EF Core
#pragma warning restore CS8618

    public Goal(GoalId id, UserId userId, GoalDomain domain, string desiredOutcome, DateTimeOffset startDate)
    {
        Id = id;
        UserId = userId;
        Domain = domain;
        DesiredOutcome = desiredOutcome ?? throw new ArgumentNullException(nameof(desiredOutcome));
        StartDate = startDate;
        Status = GoalStatus.Active;
    }

    public void Pause()
    {
        Status = GoalStatus.Paused;
    }

    public void Resume()
    {
        Status = GoalStatus.Active;
    }
}
