// ==============================================================================================
// ARCHITECTURAL CONCEPT: Swap Task Command & Handler
// Enforces Principle I (User Agency) and Principle III (Rules-First Substitution):
// 1. Replaces a scheduled action card with an approved alternative from the same health domain.
// 2. Marks DailyPlan.UserEditsApplied = true so automated engines do not override the user's choice.
// 3. Forbids swapping already-completed tasks.
// ==============================================================================================

using FluentValidation;
using Gibo.Application.Common.Interfaces;
using Gibo.Application.Features.Planning.Commands.GenerateDailyPlan;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gibo.Application.Features.Planning.Commands.SwapTask;

public sealed record SwapTaskCommand(
    Guid TaskInstanceId,
    Guid ReplacementTemplateId,
    string Locale = "en") : IRequest<TaskInstanceDto>;

public sealed class SwapTaskCommandValidator : AbstractValidator<SwapTaskCommand>
{
    public SwapTaskCommandValidator()
    {
        RuleFor(x => x.TaskInstanceId).NotEmpty().WithMessage("TaskInstanceId is required.");
        RuleFor(x => x.ReplacementTemplateId).NotEmpty().WithMessage("ReplacementTemplateId is required.");
    }
}

public sealed class SwapTaskCommandHandler : IRequestHandler<SwapTaskCommand, TaskInstanceDto>
{
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<SwapTaskCommandHandler> _logger;

    public SwapTaskCommandHandler(IPlanRepository planRepository, ILogger<SwapTaskCommandHandler> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<TaskInstanceDto> Handle(SwapTaskCommand request, CancellationToken ct)
    {
        // 1. Resolve current task instance
        var currentTask = await _planRepository.GetTaskInstanceAsync(request.TaskInstanceId, ct);

        if (currentTask == null)
            throw new DomainValidationException($"Task instance {request.TaskInstanceId} was not found.");

        // 2. Resolve parent daily plan
        var plan = await _planRepository.GetPlanByIdAsync(currentTask.PlanId, ct);

        if (plan == null)
            throw new DomainValidationException($"Parent plan for task {request.TaskInstanceId} was not found.");

        // 3. Resolve replacement template
        var replacementTemplate = await _planRepository.GetTemplateByIdAsync(request.ReplacementTemplateId, ct);

        if (replacementTemplate == null)
            throw new DomainValidationException($"Replacement template {request.ReplacementTemplateId} was not found or is inactive.");

        // 4. Domain swap operation
        var replacementInstance = plan.SwapTask(currentTask.Id, replacementTemplate, request.Locale);

        // 5. Update persistence tracking
        await _planRepository.SwapTaskAsync(plan, currentTask, replacementInstance, ct);

        _logger.LogInformation("[TASK_SWAPPED] User swapped task {OldId} for template {TemplateId} -> New task {NewId}",
            currentTask.Id, replacementTemplate.Id, replacementInstance.Id);

        return new TaskInstanceDto(
            replacementInstance.Id,
            replacementInstance.TaskTemplateId,
            replacementInstance.Domain,
            replacementInstance.ActionVerb,
            replacementInstance.Title,
            replacementInstance.TargetValue,
            replacementInstance.TargetUnit,
            replacementInstance.EstimatedMinutes,
            replacementInstance.State,
            replacementInstance.EvidenceSource,
            replacementInstance.XpAwarded,
            replacementInstance.Rationale,
            replacementInstance.IsStarterFallback,
            replacementInstance.DisplayOrder);
    }
}
