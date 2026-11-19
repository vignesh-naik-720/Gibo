// ==============================================================================================
// ARCHITECTURAL CONCEPT: Adjust Task Target Command & Handler
// Enforces Principle II (Feasibility Before Ambition) and Principle I (User Agency):
// 1. Allows users to downscale action targets (e.g. 10m walk -> 5m walk) without penalty.
// 2. Preserves UserEditsApplied = true to lock in the user's manual customization.
// 3. Forbids target adjustment on already-completed actions.
// ==============================================================================================

using FluentValidation;
using Gibo.Application.Common.Interfaces;
using Gibo.Application.Features.Planning.Commands.GenerateDailyPlan;
using Gibo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gibo.Application.Features.Planning.Commands.AdjustTaskTarget;

public sealed record AdjustTaskTargetCommand(
    Guid TaskInstanceId,
    decimal NewTarget) : IRequest<TaskInstanceDto>;

public sealed class AdjustTaskTargetCommandValidator : AbstractValidator<AdjustTaskTargetCommand>
{
    public AdjustTaskTargetCommandValidator()
    {
        RuleFor(x => x.TaskInstanceId).NotEmpty().WithMessage("TaskInstanceId is required.");
        RuleFor(x => x.NewTarget).GreaterThan(0).WithMessage("NewTarget must be strictly greater than zero.");
    }
}

public sealed class AdjustTaskTargetCommandHandler : IRequestHandler<AdjustTaskTargetCommand, TaskInstanceDto>
{
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<AdjustTaskTargetCommandHandler> _logger;

    public AdjustTaskTargetCommandHandler(IPlanRepository planRepository, ILogger<AdjustTaskTargetCommandHandler> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<TaskInstanceDto> Handle(AdjustTaskTargetCommand request, CancellationToken ct)
    {
        // 1. Resolve task instance
        var task = await _planRepository.GetTaskInstanceAsync(request.TaskInstanceId, ct);

        if (task == null)
            throw new DomainValidationException($"Task instance {request.TaskInstanceId} was not found.");

        // 2. Resolve parent plan
        var plan = await _planRepository.GetPlanByIdAsync(task.PlanId, ct);

        if (plan == null)
            throw new DomainValidationException($"Parent plan for task {request.TaskInstanceId} was not found.");

        // 3. Execute domain target reduction
        plan.AdjustTaskTarget(task.Id, request.NewTarget);

        await _planRepository.AdjustTargetAsync(plan, task, request.NewTarget, ct);

        _logger.LogInformation("[TARGET_ADJUSTED] Task {TaskId} target adjusted to {NewTarget} {Unit}",
            task.Id, task.TargetValue, task.TargetUnit);

        return new TaskInstanceDto(
            task.Id,
            task.TaskTemplateId,
            task.Domain,
            task.ActionVerb,
            task.Title,
            task.TargetValue,
            task.TargetUnit,
            task.EstimatedMinutes,
            task.State,
            task.EvidenceSource,
            task.XpAwarded,
            task.Rationale,
            task.IsStarterFallback,
            task.DisplayOrder);
    }
}
