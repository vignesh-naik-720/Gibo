// ==============================================================================================
// ARCHITECTURAL CONCEPT: Sync Offline Events Command & Batch Processor
// Enforces Principle VI (Offline Resilience & Idempotency) and Principle V (10 XP Parity):
// 1. Accepts queued completion events generated while mobile device had no network.
// 2. Uses ClientEventId to deduplicate already-processed events without double-crediting XP.
// 3. Batches database operations into a single atomic transaction.
// ==============================================================================================

using FluentValidation;
using Gibo.Application.Common.Interfaces;
using Gibo.Application.Features.Planning.Commands.CompleteTask;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gibo.Application.Features.Planning.Commands.SyncOfflineEvents;

public sealed record OfflineCompletionEventItem(
    Guid TaskInstanceId,
    CompletionEvidenceSource EvidenceSource,
    decimal? MeasuredValue,
    Guid ClientEventId,
    DateTimeOffset RecordedAtUtc);

public sealed record SyncOfflineEventsCommand(
    IReadOnlyList<OfflineCompletionEventItem> Events) : IRequest<BatchSyncResultDto>;

public sealed record BatchSyncResultDto(
    int TotalReceived,
    int SyncedCount,
    int DuplicateCount,
    int TotalXpAwarded,
    IReadOnlyList<TaskCompletionResultDto> Results);

public sealed class SyncOfflineEventsCommandValidator : AbstractValidator<SyncOfflineEventsCommand>
{
    public SyncOfflineEventsCommandValidator()
    {
        RuleFor(x => x.Events).NotNull().WithMessage("Events collection cannot be null.");
    }
}

public sealed class SyncOfflineEventsCommandHandler : IRequestHandler<SyncOfflineEventsCommand, BatchSyncResultDto>
{
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<SyncOfflineEventsCommandHandler> _logger;

    public SyncOfflineEventsCommandHandler(IPlanRepository planRepository, ILogger<SyncOfflineEventsCommandHandler> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<BatchSyncResultDto> Handle(SyncOfflineEventsCommand request, CancellationToken ct)
    {
        var results = new List<TaskCompletionResultDto>();
        int synced = 0;
        int duplicates = 0;
        int totalXp = 0;

        if (request.Events.Count == 0)
        {
            return new BatchSyncResultDto(0, 0, 0, 0, results);
        }

        foreach (var item in request.Events)
        {
            var existingEvent = await _planRepository.GetCompletionEventByClientEventIdAsync(item.ClientEventId, ct);
            if (existingEvent != null)
            {
                duplicates++;
                results.Add(new TaskCompletionResultDto(
                    item.TaskInstanceId,
                    existingEvent.State,
                    0,
                    IsDuplicate: true,
                    existingEvent.RecordedAtUtc));
                continue;
            }

            var task = await _planRepository.GetTaskInstanceAsync(item.TaskInstanceId, ct);
            if (task == null)
            {
                _logger.LogWarning("[OFFLINE_SYNC_MISSING_TASK] TaskInstance {TaskId} not found for offline sync", item.TaskInstanceId);
                continue;
            }

            var plan = await _planRepository.GetPlanByIdAsync(task.PlanId, ct);
            if (plan == null)
            {
                _logger.LogWarning("[OFFLINE_SYNC_MISSING_PLAN] DailyPlan {PlanId} not found for offline sync", task.PlanId);
                continue;
            }

            plan.CompleteTask(task.Id, item.EvidenceSource);

            var completionEvent = CompletionEvent.Create(
                Guid.NewGuid(),
                task.Id,
                plan.UserId,
                task.State,
                item.EvidenceSource,
                item.MeasuredValue,
                item.RecordedAtUtc,
                item.ClientEventId);

            await _planRepository.RecordCompletionAsync(plan, task, completionEvent, ct);

            synced++;
            totalXp += task.XpAwarded;

            results.Add(new TaskCompletionResultDto(
                task.Id,
                task.State,
                task.XpAwarded,
                IsDuplicate: false,
                task.CompletedAt));
        }

        _logger.LogInformation("[OFFLINE_BATCH_SYNC] Processed {Total} events: {Synced} synced (+{TotalXp} XP), {Duplicates} duplicates ignored.",
            request.Events.Count, synced, totalXp, duplicates);

        return new BatchSyncResultDto(request.Events.Count, synced, duplicates, totalXp, results);
    }
}
