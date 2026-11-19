using Gibo.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gibo.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, val => new UserId(val))
            .HasColumnName("user_id");

        builder.Property(u => u.IsAdultConfirmed)
            .IsRequired()
            .HasColumnName("is_adult_confirmed");

        builder.Property(u => u.PreferredLocale)
            .HasConversion(l => l.Code, code => Locale.From(code))
            .HasMaxLength(10)
            .HasColumnName("preferred_locale");

        builder.Property(u => u.TimeZoneId)
            .HasMaxLength(50)
            .HasColumnName("timezone_id");

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("account_status");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at");

        // Relationships
        builder.HasOne(u => u.Preferences)
            .WithOne()
            .HasForeignKey<UserPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ConsentGrants)
            .WithOne()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Goals)
            .WithOne()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ConsentGrantConfiguration : IEntityTypeConfiguration<ConsentGrant>
{
    public void Configure(EntityTypeBuilder<ConsentGrant> builder)
    {
        builder.ToTable("consent_grants");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, val => new ConsentId(val))
            .HasColumnName("consent_id");

        builder.Property(c => c.UserId)
            .HasConversion(id => id.Value, val => new UserId(val))
            .HasColumnName("user_id");

        builder.Property(c => c.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("category");

        builder.Property(c => c.Purpose)
            .IsRequired()
            .HasColumnName("purpose");

        builder.Property(c => c.PolicyVersion)
            .HasMaxLength(20)
            .HasColumnName("policy_version");

        builder.Property(c => c.GrantedAt)
            .HasColumnName("granted_at");

        builder.Property(c => c.RevokedAt)
            .HasColumnName("revoked_at");

        builder.HasIndex(c => new { c.UserId, c.Category });
    }
}

public sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, val => new PreferenceId(val))
            .HasColumnName("preference_id");

        builder.Property(p => p.UserId)
            .HasConversion(id => id.Value, val => new UserId(val))
            .HasColumnName("user_id");

        builder.Property(p => p.PrimaryGoalDomain)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("primary_goal_domain");

        builder.Property(p => p.WorkPattern)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("work_pattern");

        builder.Property(p => p.IndoorPreferred)
            .HasColumnName("indoor_preferred");

        builder.Property(p => p.MaxDailyActions)
            .HasColumnName("max_daily_actions");

        builder.Property(p => p.QuietHoursStart)
            .HasColumnName("quiet_hours_start");

        builder.Property(p => p.QuietHoursEnd)
            .HasColumnName("quiet_hours_end");

        builder.Property(p => p.BaselineLevel)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("baseline_level");
    }
}

public sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id)
            .HasConversion(id => id.Value, val => new GoalId(val))
            .HasColumnName("goal_id");

        builder.Property(g => g.UserId)
            .HasConversion(id => id.Value, val => new UserId(val))
            .HasColumnName("user_id");

        builder.Property(g => g.Domain)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("domain");

        builder.Property(g => g.DesiredOutcome)
            .HasMaxLength(255)
            .HasColumnName("desired_outcome");

        builder.Property(g => g.StartDate)
            .HasColumnName("start_date");

        builder.Property(g => g.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");
    }
}
