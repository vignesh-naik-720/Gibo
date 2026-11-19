using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Gibo.Application.Common.Interfaces;

public interface IGiboDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserPreference> UserPreferences { get; }
    DbSet<ConsentGrant> ConsentGrants { get; }
    DbSet<Goal> Goals { get; }

    DbSet<DailyPlan> DailyPlans { get; }
    DbSet<TaskInstance> TaskInstances { get; }
    DbSet<TaskTemplate> TaskTemplates { get; }
    DbSet<CompletionEvent> CompletionEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
