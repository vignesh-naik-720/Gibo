using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Domain.Common;

namespace Gibo.Domain.Events;

/// <summary>
/// Emitted when a legally binding consent grant or revocation is recorded.
/// </summary>
public sealed record ConsentRecordedEvent(
    UserId UserId,
    ConsentCategory Category,
    string PolicyVersion,
    DateTimeOffset Timestamp) : IDomainEvent
{
    public DateTimeOffset OccurredOn => Timestamp;
}
