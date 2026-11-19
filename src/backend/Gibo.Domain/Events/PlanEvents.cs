using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Common;

namespace Gibo.Domain.Events;

public sealed record PlanGeneratedEvent(
    Guid PlanId,
    Guid UserId,
    DateOnly PlanDate,
    int TaskCount,
    DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record TaskCompletedEvent(
    Guid TaskInstanceId,
    Guid PlanId,
    Guid UserId,
    CompletionEvidenceSource EvidenceSource,
    int XpAwarded,
    DateTimeOffset OccurredOn) : IDomainEvent;
