using Gibo.Domain.Common;
using Gibo.Domain.Events;
using Gibo.Domain.Exceptions;

namespace Gibo.Domain.Aggregates.UserAggregate;

/// <summary>
/// Aggregate Root: User
/// The central boundary enforcing business rules and compliance invariants for user identity,
/// onboarding verification, privacy consent grants, and active wellness goals.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    // Invariant: The MVP is strictly restricted to adults (18+)
    public bool IsAdultConfirmed { get; private set; }

    // User's preferred communication language: "en", "ar" (RTL), or "hi"
    public Locale PreferredLocale { get; private set; }

    // IANA Timezone ID (e.g. "Asia/Dubai")
    public string TimeZoneId { get; private set; }

    // UTC timestamp when the user profile was initialized
    public DateTimeOffset CreatedAt { get; private set; }

    // Account state (Active, Suspended, DeletionRequested)
    public UserAccountStatus Status { get; private set; }

    // Contextual preferences controlling daily action feasibility
    public UserPreference Preferences { get; private set; }

    // Granular consent ledger (one entry per processing purpose)
    private readonly List<ConsentGrant> _consentGrants = new();
    public IReadOnlyCollection<ConsentGrant> ConsentGrants => _consentGrants.AsReadOnly();

    // User's active health goals (Movement, Sleep, Nutrition)
    private readonly List<Goal> _goals = new();
    public IReadOnlyCollection<Goal> Goals => _goals.AsReadOnly();

#pragma warning disable CS8618
    private User() { } // Required for EF Core
#pragma warning restore CS8618

    /// <summary>
    /// Factory method enforcing all prerequisite onboarding checks prior to instantiation.
    /// </summary>
    public static User CreateInitial(
        UserId id, 
        bool isAdult, 
        Locale locale, 
        string timeZoneId,
        UserPreference preferences)
    {
        // Enforce 18+ gate
        if (!isAdult)
        {
            throw new DomainValidationException(
                "Gibo is currently restricted to adults (18+). Underage registration is not permitted.",
                "ELIGIBILITY_UNDERAGE");
        }

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

    /// <summary>
    /// Records an auditable, unbundled consent grant.
    /// Under UAE PDPL, each processing purpose requires explicit consent.
    /// </summary>
    public void RecordConsent(ConsentCategory category, string purpose, string policyVersion)
    {
        // Revoke any previous version for the exact same category
        var existingActive = _consentGrants.FirstOrDefault(c => c.Category == category && c.RevokedAt == null);
        if (existingActive != null)
        {
            existingActive.Revoke();
        }

        var grant = new ConsentGrant(
            ConsentId.New(), 
            Id, 
            category, 
            purpose, 
            policyVersion, 
            DateTimeOffset.UtcNow);

        _consentGrants.Add(grant);

        // Raise domain event for audit logging
        AddDomainEvent(new ConsentRecordedEvent(Id, category, policyVersion, grant.GrantedAt));
    }

    /// <summary>
    /// Revokes consent for a specific category (e.g., disconnecting wearable sync).
    /// </summary>
    public void RevokeConsent(ConsentCategory category)
    {
        var activeGrants = _consentGrants.Where(c => c.Category == category && c.RevokedAt == null);
        foreach (var grant in activeGrants)
        {
            grant.Revoke();
        }
    }

    /// <summary>
    /// Binds an initial primary wellness goal.
    /// </summary>
    public void AddGoal(GoalDomain domain, string desiredOutcome)
    {
        var goal = new Goal(GoalId.New(), Id, domain, desiredOutcome, DateTimeOffset.UtcNow);
        _goals.Add(goal);
    }
}
