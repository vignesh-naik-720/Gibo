# Data Model & Schema Specification: Onboarding & Consent Governance

**Feature Branch**: `001-onboarding-and-consent` | **Date**: 2026-11-15

---

## 1. Domain Entities & Class Architecture

```csharp
// ==============================================================================================
// ARCHITECTURAL CONCEPT: Rich Domain Model with Value Objects & Encapsulation
// In Clean Architecture and Domain-Driven Design (DDD), entity state is protected from external
// corruption by keeping setters private and exposing explicit business methods.
// Value objects (such as Locale, TimeZoneCode, and ConsentCategory) guarantee that invalid
// data representations (e.g. invalid locale strings or unauthorized categories) cannot exist.
// ==============================================================================================

namespace Gibo.Domain.Aggregates.UserAggregate;

// Aggregate Root: User
public sealed class User : AggregateRoot<UserId>
{
    // Unique system identifier (UUIDv4)
    public UserId Id { get; private set; }

    // Enforces the 18+ adult eligibility requirement mandated by UAE digital services policy
    public bool IsAdultConfirmed { get; private set; }

    // Selected language: "en", "ar", or "hi"
    public Locale PreferredLocale { get; private set; }

    // User's IANA timezone (e.g., "Asia/Dubai") for computing daily cutoffs and quiet hours
    public string TimeZoneId { get; private set; }

    // Account creation timestamp in UTC
    public DateTimeOffset CreatedAt { get; private set; }

    // Current lifecycle status of the account
    public UserAccountStatus Status { get; private set; }

    // Associated lifestyle and contextual feasibility preferences
    public UserPreference Preferences { get; private set; }

    // Granular consent ledger (one entry per processing purpose)
    private readonly List<ConsentGrant> _consentGrants = new();
    public IReadOnlyCollection<ConsentGrant> ConsentGrants => _consentGrants.AsReadOnly();

    // User's active health goals (Movement, Sleep, Nutrition)
    private readonly List<Goal> _goals = new();
    public IReadOnlyCollection<Goal> Goals => _goals.AsReadOnly();

    private User() { } // Required by EF Core

    public static User Create(
        UserId id, 
        bool isAdult, 
        Locale locale, 
        string timeZoneId,
        UserPreference preferences)
    {
        if (!isAdult)
            throw new DomainValidationException("Gibo is restricted to adults (18+) for wellness habit support.");

        var user = new User
        {
            Id = id,
            IsAdultConfirmed = true,
            PreferredLocale = locale ?? Locale.DefaultEnglish,
            TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "Asia/Dubai" : timeZoneId,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = UserAccountStatus.Active,
            Preferences = preferences ?? throw new ArgumentNullException(nameof(preferences))
        };

        return user;
    }

    public void AddConsentGrant(ConsentCategory category, string purpose, string policyVersion)
    {
        // Principle IV: Prevent duplicate active grants for the exact same category
        var existingActive = _consentGrants.FirstOrDefault(c => c.Category == category && c.RevokedAt == null);
        if (existingActive != null)
        {
            existingActive.Revoke(); // Revoke previous version before applying updated version
        }

        var grant = new ConsentGrant(ConsentId.New(), Id, category, purpose, policyVersion, DateTimeOffset.UtcNow);
        _consentGrants.Add(grant);
    }

    public void AddInitialGoal(GoalDomain domain, string desiredOutcome)
    {
        var goal = new Goal(GoalId.New(), Id, domain, desiredOutcome, DateTimeOffset.UtcNow);
        _goals.Add(goal);
    }
}

// Entity: ConsentGrant (UAE PDPL Compliance Ledger)
public sealed class ConsentGrant : Entity<ConsentId>
{
    public ConsentId Id { get; private set; }
    public UserId UserId { get; private set; }
    public ConsentCategory Category { get; private set; }
    public string Purpose { get; private set; }
    public string PolicyVersion { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt == null;

    private ConsentGrant() { }

    public ConsentGrant(ConsentId id, UserId userId, ConsentCategory category, string purpose, string policyVersion, DateTimeOffset grantedAt)
    {
        Id = id;
        UserId = userId;
        Category = category;
        Purpose = purpose;
        PolicyVersion = policyVersion;
        GrantedAt = grantedAt;
    }

    public void Revoke()
    {
        RevokedAt = DateTimeOffset.UtcNow;
    }
}

// Entity: UserPreference (Contextual Feasibility Parameters)
public sealed class UserPreference : Entity<PreferenceId>
{
    public PreferenceId Id { get; private set; }
    public UserId UserId { get; private set; }
    public GoalDomain PrimaryGoalDomain { get; private set; }
    public WorkSchedulePattern WorkPattern { get; private set; }
    public bool IndoorPreferred { get; private set; }
    public int MaxDailyActions { get; private set; } // Constrained: 1, 2, or 3 (default 1)
    public TimeSpan QuietHoursStart { get; private set; }
    public TimeSpan QuietHoursEnd { get; private set; }
    public BaselineFitnessLevel BaselineLevel { get; private set; }
    public List<string> AccessibilityTags { get; private set; } = new();

    private UserPreference() { }

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
        IEnumerable<string> accessibilityTags)
    {
        Id = id;
        UserId = userId;
        PrimaryGoalDomain = primaryGoalDomain;
        WorkPattern = workPattern;
        IndoorPreferred = indoorPreferred;
        MaxDailyActions = Math.Clamp(maxDailyActions, 1, 3);
        QuietHoursStart = quietHoursStart;
        QuietHoursEnd = quietHoursEnd;
        BaselineLevel = baselineLevel;
        AccessibilityTags = accessibilityTags?.ToList() ?? new List<string>();
    }
}

// Entity: Goal
public sealed class Goal : Entity<GoalId>
{
    public GoalId Id { get; private set; }
    public UserId UserId { get; private set; }
    public GoalDomain Domain { get; private set; }
    public string DesiredOutcome { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public GoalStatus Status { get; private set; }

    private Goal() { }

    public Goal(GoalId id, UserId userId, GoalDomain domain, string desiredOutcome, DateTimeOffset startDate)
    {
        Id = id;
        UserId = userId;
        Domain = domain;
        DesiredOutcome = desiredOutcome;
        StartDate = startDate;
        Status = GoalStatus.Active;
    }
}
```

---

## 2. Enumerations & Value Types

```csharp
public enum ConsentCategory
{
    CoreProfile = 1,          // Mandatory for account operations and personal plans
    HealthConnectSync = 2,    // Optional wearable step and activity sync
    AssistantChatContext = 3, // Optional conversational context retention
    AnonymousResearch = 4     // Optional anonymized cohort analytics for public health studies
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
    Starter = 1,      // Prefers < 10 mins, gentle pacing
    Intermediate = 2, // Standard daily walking or light activity
    Active = 3        // Regular physical activity
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
```

---

## 3. Relational Database Schema (PostgreSQL DDL)

```sql
-- ==============================================================================================
-- DATABASE SCHEMA DDL: PostgreSQL 16+
-- Implements relational foreign key constraints, indexes on lookups, and auditability.
-- All timestamps are TIMESTAMP WITH TIME ZONE (stored as UTC).
-- ==============================================================================================

CREATE TABLE users (
    user_id UUID PRIMARY KEY,
    is_adult_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    preferred_locale VARCHAR(10) NOT NULL DEFAULT 'en',
    timezone_id VARCHAR(50) NOT NULL DEFAULT 'Asia/Dubai',
    account_status VARCHAR(20) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_users_adult CHECK (is_adult_confirmed = TRUE)
);

CREATE TABLE user_preferences (
    preference_id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    primary_goal_domain VARCHAR(30) NOT NULL,
    work_pattern VARCHAR(30) NOT NULL DEFAULT 'StandardDaytime',
    indoor_preferred BOOLEAN NOT NULL DEFAULT TRUE,
    max_daily_actions INT NOT NULL DEFAULT 1 CHECK (max_daily_actions BETWEEN 1 AND 3),
    quiet_hours_start TIME NOT NULL DEFAULT '22:00:00',
    quiet_hours_end TIME NOT NULL DEFAULT '07:00:00',
    baseline_level VARCHAR(30) NOT NULL DEFAULT 'Starter',
    accessibility_tags JSONB NOT NULL DEFAULT '[]'::jsonb,
    CONSTRAINT uq_user_preferences_user UNIQUE (user_id)
);

CREATE TABLE consent_grants (
    consent_id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    category VARCHAR(50) NOT NULL,
    purpose TEXT NOT NULL,
    policy_version VARCHAR(20) NOT NULL,
    granted_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP WITH TIME ZONE NULL
);

CREATE INDEX idx_consent_grants_user_active 
ON consent_grants(user_id, category) 
WHERE revoked_at IS NULL;

CREATE TABLE goals (
    goal_id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    domain VARCHAR(30) NOT NULL,
    desired_outcome VARCHAR(255) NOT NULL,
    start_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) NOT NULL DEFAULT 'Active'
);

CREATE TABLE audit_events (
    event_id UUID PRIMARY KEY,
    actor_id VARCHAR(100) NOT NULL,
    action VARCHAR(100) NOT NULL,
    target_object VARCHAR(200) NOT NULL,
    reason TEXT NOT NULL,
    trace_id VARCHAR(100) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_events_created_at ON audit_events(created_at);
```

---

## 4. State Transitions & Invariant Rules

### Invariant Rules
1. **Adult Eligibility Gate**: `is_adult_confirmed` must be `true`. Attempts to persist a user with `is_adult_confirmed = false` violate the database check constraint `chk_users_adult`.
2. **Action Cap Limit**: `max_daily_actions` must strictly be 1, 2, or 3. Defaults to 1 for starter baseline users.
3. **Consent Immutability**: Existing `consent_grants` rows are never updated except for setting `revoked_at`. A new policy version creates a new row.
4. **Cascading Erasure**: Deleting a `users` record cascades automatically to `user_preferences`, `goals`, and `consent_grants`, ensuring clean right-to-erasure compliance under UAE PDPL Article 15.
