// ==============================================================================================
// ARCHITECTURAL CONCEPT: Complete Task Command & Deduplication Handler
// Enforces Principle V (Device and Manual Parity) and Principle VI (Idempotency):
// 1. Checks if a CompletionEvent with ClientEventId already exists to prevent duplicate XP awards.
// 2. Invokes domain method plan.CompleteTask(taskId, source), enforcing exactly 10 XP award.
// 3. Persists an immutable CompletionEvent receipt for auditability.
// ==============================================================================================

using FluentValidation;
using Gibo.Application.Common.Interfaces;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gibo.Application.Features.Planning.Commands.CompleteTask;

public sealed record CompleteTaskCommand(
    Guid TaskInstanceId,
    CompletionEvidenceSource EvidenceSource,
    decimal? MeasuredValue,
    Guid ClientEventId,
    DateTimeOffset RecordedAtUtc) : IRequest<TaskCompletionResultDto>;

public sealed record TaskCompletionResultDto(
    Guid TaskInstanceId,
    TaskState State,
    int XpAwarded,
    bool IsDuplicate,
    DateTimeOffset? CompletedAtUtc);

public sealed class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public CompleteTaskCommandValidator()
    {
        RuleFor(x => x.TaskInstanceId)
            .NotEmpty()
            .WithMessage("TaskInstanceId is required.");

        RuleFor(x => x.ClientEventId)
            .NotEmpty()
            .WithMessage("ClientEventId is required for offline deduplication.");
    }
}

public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, TaskCompletionResultDto>
{
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<CompleteTaskCommandHandler> _logger;

    public CompleteTaskCommandHandler(
        IPlanRepository planRepository,
        ILogger<CompleteTaskCommandHandler> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<TaskCompletionResultDto> Handle(CompleteTaskCommand request, CancellationToken ct)
    {
        // 1. Check for duplicate completion via ClientEventId (Idempotency Guarantee)
        var existingEvent = await _planRepository.GetCompletionEventByClientEventIdAsync(request.ClientEventId, ct);

        if (existingEvent != null)
        {
            _logger.LogInformation("[COMPLETION_IDEMPOTENT_HIT] Duplicate completion ignored for ClientEventId {ClientEventId}", request.ClientEventId);
            return new TaskCompletionResultDto(
                request.TaskInstanceId,
                existingEvent.State,
                0, // 0 extra XP on duplicate
                IsDuplicate: true,
                existingEvent.RecordedAtUtc);
        }

        // 2. Find task instance and parent plan
        var task = await _planRepository.GetTaskInstanceAsync(request.TaskInstanceId, ct);

        if (task == null)
            throw new DomainValidationException($"Task instance {request.TaskInstanceId} was not found.");

        var plan = await _planRepository.GetPlanByIdAsync(task.PlanId, ct);

        if (plan == null)
            throw new DomainValidationException($"Parent plan for task {request.TaskInstanceId} was not found.");

        // 3. Execute domain completion (Strict 10 XP award)
        plan.CompleteTask(task.Id, request.EvidenceSource);

        // 4. Create immutable completion audit event
        var completionEvent = CompletionEvent.Create(
            Guid.NewGuid(),
            task.Id,
            plan.UserId,
            task.State,
            request.EvidenceSource,
            request.MeasuredValue,
            request.RecordedAtUtc,
            request.ClientEventId);

        await _planRepository.RecordCompletionAsync(plan, task, completionEvent, ct);

        _logger.LogInformation("[TASK_COMPLETED] Task {TaskId} completed via {Source} (+{Xp} XP)", task.Id, request.EvidenceSource, task.XpAwarded);

        return new TaskCompletionResultDto(
            task.Id,
            task.State,
            task.XpAwarded,
            IsDuplicate: false,
            task.CompletedAt);
    }
}
