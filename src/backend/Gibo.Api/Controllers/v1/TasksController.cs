// ==============================================================================================
// ARCHITECTURAL CONCEPT: Tasks Controller
// Endpoints for task completion, user target reduction, task swapping, and offline batch synchronization.
// ==============================================================================================

using Gibo.Application.Features.Planning.Commands.AdjustTaskTarget;
using Gibo.Application.Features.Planning.Commands.CompleteTask;
using Gibo.Application.Features.Planning.Commands.GenerateDailyPlan;
using Gibo.Application.Features.Planning.Commands.SwapTask;
using Gibo.Application.Features.Planning.Commands.SyncOfflineEvents;
using Gibo.Domain.Aggregates.PlanAggregate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gibo.Api.Controllers.v1;

[ApiController]
[Route("api/v1/tasks")]
[Produces("application/json")]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Records completion of a scheduled daily action via manual tap or Health Connect sync.
    /// </summary>
    [HttpPost("{taskId:guid}/complete")]
    [ProducesResponseType(typeof(TaskCompletionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTask(
        [FromRoute] Guid taskId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] CompleteTaskRequest request,
        CancellationToken ct)
    {
        var clientEventId = Guid.TryParse(request.ClientEventId, out var parsedId) ? parsedId : Guid.NewGuid();
        var recordedAt = request.RecordedAtUtc ?? DateTimeOffset.UtcNow;

        var command = new CompleteTaskCommand(
            taskId,
            request.EvidenceSource,
            request.MeasuredValue,
            clientEventId,
            recordedAt);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Swaps a scheduled action with an approved alternative from the same health domain.
    /// </summary>
    [HttpPost("{taskId:guid}/swap")]
    [ProducesResponseType(typeof(TaskInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SwapTask(
        [FromRoute] Guid taskId,
        [FromBody] SwapTaskRequest request,
        CancellationToken ct)
    {
        var command = new SwapTaskCommand(taskId, request.ReplacementTemplateId, request.Locale ?? "en");
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Reduces the target value of a task (e.g. 10m walk -> 5m walk).
    /// </summary>
    [HttpPut("{taskId:guid}/target")]
    [ProducesResponseType(typeof(TaskInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustTarget(
        [FromRoute] Guid taskId,
        [FromBody] AdjustTargetRequest request,
        CancellationToken ct)
    {
        var command = new AdjustTaskTargetCommand(taskId, request.NewTarget);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Batches and synchronizes completion events queued while the device was offline.
    /// </summary>
    [HttpPost("sync-offline")]
    [ProducesResponseType(typeof(BatchSyncResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncOffline(
        [FromBody] SyncOfflineRequest request,
        CancellationToken ct)
    {
        var eventItems = request.Events.Select(e => new OfflineCompletionEventItem(
            e.TaskInstanceId,
            e.EvidenceSource,
            e.MeasuredValue,
            Guid.TryParse(e.ClientEventId, out var parsedId) ? parsedId : Guid.NewGuid(),
            e.RecordedAtUtc ?? DateTimeOffset.UtcNow)).ToList();

        var command = new SyncOfflineEventsCommand(eventItems);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

public sealed class CompleteTaskRequest
{
    public CompletionEvidenceSource EvidenceSource { get; set; } = CompletionEvidenceSource.Manual;
    public decimal? MeasuredValue { get; set; }
    public string ClientEventId { get; set; } = string.Empty;
    public DateTimeOffset? RecordedAtUtc { get; set; }
}

public sealed class SwapTaskRequest
{
    public Guid ReplacementTemplateId { get; set; }
    public string? Locale { get; set; }
}

public sealed class AdjustTargetRequest
{
    public decimal NewTarget { get; set; }
}

public sealed class SyncOfflineRequest
{
    public List<OfflineEventRequestItem> Events { get; set; } = new();
}

public sealed class OfflineEventRequestItem
{
    public Guid TaskInstanceId { get; set; }
    public CompletionEvidenceSource EvidenceSource { get; set; } = CompletionEvidenceSource.Manual;
    public decimal? MeasuredValue { get; set; }
    public string ClientEventId { get; set; } = string.Empty;
    public DateTimeOffset? RecordedAtUtc { get; set; }
}
