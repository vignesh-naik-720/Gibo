using Gibo.Application.Common.Interfaces;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Gibo.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core Database Context for the Gibo Modular Monolith.
/// Enforces relational foreign key cascades, unique constraints, and PostgreSQL column types.
/// </summary>
public sealed class GiboDbContext : DbContext, IGiboDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<ConsentGrant> ConsentGrants => Set<ConsentGrant>();
    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<DailyPlan> DailyPlans => Set<DailyPlan>();
    public DbSet<TaskInstance> TaskInstances => Set<TaskInstance>();
    public DbSet<TaskTemplate> TaskTemplates => Set<TaskTemplate>();
    public DbSet<CompletionEvent> CompletionEvents => Set<CompletionEvent>();

    public GiboDbContext(DbContextOptions<GiboDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiboDbContext).Assembly);

        // Seed curated task templates
        modelBuilder.Entity<TaskTemplate>().HasData(TaskCatalogSeed.CuratedTemplates);
    }
}
