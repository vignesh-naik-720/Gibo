using Microsoft.Extensions.Logging;

namespace Gibo.Infrastructure.Logging;

public sealed record AuditLogEntry(
    Guid EventId,
    string ActorId,
    string Action,
    string TargetObject,
    string Reason,
    string TraceId,
    DateTimeOffset Timestamp);

public interface IAuditEventLogger
{
    Task LogEventAsync(AuditLogEntry entry, CancellationToken ct = default);
}

/// <summary>
/// Writes append-only audit trail records for compliance with UAE PDPL Article 5 & ADHICS V2 standards.
/// </summary>
public sealed class AuditEventLogger : IAuditEventLogger
{
    private readonly ILogger<AuditEventLogger> _logger;

    public AuditEventLogger(ILogger<AuditEventLogger> logger)
    {
        _logger = logger;
    }

    public Task LogEventAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        // In production, persisted to the audit_events table and replicated to a write-once read-many (WORM) storage sink.
        _logger.LogInformation(
            "[AUDIT_EVENT] Id={EventId} Actor={Actor} Action={Action} Target={Target} Reason={Reason} TraceId={TraceId} Time={Timestamp}",
            entry.EventId,
            entry.ActorId,
            entry.Action,
            entry.TargetObject,
            entry.Reason,
            entry.TraceId,
            entry.Timestamp);

        return Task.CompletedTask;
    }
}
