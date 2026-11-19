using Gibo.Domain.Common;

namespace Gibo.Domain.Aggregates.UserAggregate;

/// <summary>
/// Domain Entity: ConsentGrant
/// Represents an unbundled, auditable consent grant under UAE PDPL Federal Decree-Law No. 45 of 2021.
/// Every purpose for data collection is tracked independently with explicit grant and revocation timestamps.
/// </summary>
public sealed class ConsentGrant : Entity<ConsentId>
{
    // The parent user identity
    public UserId UserId { get; private set; }

    // Processing purpose category (e.g. CoreProfile, HealthConnectSync, AssistantChatContext)
    public ConsentCategory Category { get; private set; }

    // Plain-language description of what this consent allows
    public string Purpose { get; private set; }

    // Version identifier of the privacy notice displayed to the user
    public string PolicyVersion { get; private set; }

    // UTC timestamp when consent was granted
    public DateTimeOffset GrantedAt { get; private set; }

    // Nullable UTC timestamp when consent was revoked (null = currently active)
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt == null;

#pragma warning disable CS8618
    private ConsentGrant() { } // Required for EF Core materialization
#pragma warning restore CS8618

    public ConsentGrant(
        ConsentId id, 
        UserId userId, 
        ConsentCategory category, 
        string purpose, 
        string policyVersion, 
        DateTimeOffset grantedAt)
    {
        Id = id;
        UserId = userId;
        Category = category;
        Purpose = purpose ?? throw new ArgumentNullException(nameof(purpose));
        PolicyVersion = policyVersion ?? throw new ArgumentNullException(nameof(policyVersion));
        GrantedAt = grantedAt;
    }

    /// <summary>
    /// Revokes the consent grant immediately.
    /// Under UAE PDPL Article 5, revocation must be as effortless as granting.
    /// </summary>
    public void Revoke()
    {
        if (RevokedAt != null) return; // Idempotent revocation
        RevokedAt = DateTimeOffset.UtcNow;
    }
}
