// ==============================================================================================
// ARCHITECTURAL CONCEPT: Completion Event Audit Entity
// Represents an immutable, auditable receipt of task execution.
// Supports offline deduplication via client-generated ClientEventId UUIDs, ensuring that
// intermittent network retries never double-credit XP or corrupt streak analytics.
// ==============================================================================================

using Gibo.Domain.Common;
using Gibo.Domain.Exceptions;

namespace Gibo.Domain.Aggregates.PlanAggregate;

public sealed class CompletionEvent : Entity<Guid>
{
    public Guid TaskInstanceId { get; private set; }
    public Guid UserId { get; private set; }
    public TaskState State { get; private set; }
    public CompletionEvidenceSource EvidenceSource { get; private set; }
    public decimal? MeasuredValue { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }
    public Guid ClientEventId { get; private set; }
    public DateTimeOffset SyncedAtUtc { get; private set; }

    private CompletionEvent() { }

    public static CompletionEvent Create(
        Guid id,
        Guid taskInstanceId,
        Guid userId,
        TaskState state,
        CompletionEvidenceSource evidenceSource,
        decimal? measuredValue,
        DateTimeOffset recordedAtUtc,
        Guid clientEventId)
    {
        if (taskInstanceId == Guid.Empty)
            throw new DomainValidationException("Completion event must reference a valid task instance.");

        if (userId == Guid.Empty)
            throw new DomainValidationException("Completion event must reference a valid user.");

        if (clientEventId == Guid.Empty)
            throw new DomainValidationException("ClientEventId is required for offline deduplication.");

        return new CompletionEvent
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            TaskInstanceId = taskInstanceId,
            UserId = userId,
            State = state,
            EvidenceSource = evidenceSource,
            MeasuredValue = measuredValue,
            RecordedAtUtc = recordedAtUtc,
            ClientEventId = clientEventId,
            SyncedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
