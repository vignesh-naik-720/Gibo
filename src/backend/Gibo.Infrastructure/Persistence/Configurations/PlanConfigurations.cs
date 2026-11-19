// ==============================================================================================
// ARCHITECTURAL CONCEPT: EF Core Mappings for Daily Plan Aggregate & Catalog
// Maps domain invariants into relational constraints:
// - Unique index on (UserId, PlanDate) ensures at most one plan per calendar day.
// - Index on ClientEventId guarantees fast offline sync deduplication.
// - JSON/Array conversion for TaskTemplate SafetyTags and localized properties.
// ==============================================================================================

using Gibo.Domain.Aggregates.PlanAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gibo.Infrastructure.Persistence.Configurations;

public sealed class DailyPlanConfiguration : IEntityTypeConfiguration<DailyPlan>
{
    public void Configure(EntityTypeBuilder<DailyPlan> builder)
    {
        builder.ToTable("DailyPlans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PlanDate)
            .IsRequired();

        builder.Property(p => p.AlgorithmVersion)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.UserEditsApplied)
            .IsRequired();

        // Enforce at most one plan per user per calendar day
        builder.HasIndex(p => new { p.UserId, p.PlanDate })
            .IsUnique();

        builder.HasMany(p => p.Tasks)
            .WithOne()
            .HasForeignKey(t => t.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Tasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class TaskInstanceConfiguration : IEntityTypeConfiguration<TaskInstance>
{
    public void Configure(EntityTypeBuilder<TaskInstance> builder)
    {
        builder.ToTable("TaskInstances");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ActionVerb)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(t => t.TargetValue)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(t => t.TargetUnit)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.Rationale)
            .HasMaxLength(500);

        builder.HasIndex(t => t.PlanId);
        builder.HasIndex(t => t.TaskTemplateId);
    }
}

public sealed class TaskTemplateConfiguration : IEntityTypeConfiguration<TaskTemplate>
{
    public void Configure(EntityTypeBuilder<TaskTemplate> builder)
    {
        builder.ToTable("TaskTemplates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ActionVerb)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.TargetUnit)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.BCTCategory)
            .HasMaxLength(100);

        builder.Property(t => t.TitleEn).HasMaxLength(250).IsRequired();
        builder.Property(t => t.TitleAr).HasMaxLength(250).IsRequired();
        builder.Property(t => t.TitleHi).HasMaxLength(250).IsRequired();

        builder.Property(t => t.RationaleEn).HasMaxLength(500);
        builder.Property(t => t.RationaleAr).HasMaxLength(500);
        builder.Property(t => t.RationaleHi).HasMaxLength(500);

        var stringListComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(t => t.SafetyTags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(stringListComparer);
    }
}

public sealed class CompletionEventConfiguration : IEntityTypeConfiguration<CompletionEvent>
{
    public void Configure(EntityTypeBuilder<CompletionEvent> builder)
    {
        builder.ToTable("CompletionEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MeasuredValue)
            .HasPrecision(10, 2);

        // Fast lookup for idempotent offline event synchronization
        builder.HasIndex(e => e.ClientEventId)
            .IsUnique();

        builder.HasIndex(e => e.TaskInstanceId);
        builder.HasIndex(e => e.UserId);
    }
}
