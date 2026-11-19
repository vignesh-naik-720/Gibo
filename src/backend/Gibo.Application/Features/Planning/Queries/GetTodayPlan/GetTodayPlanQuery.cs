// ==============================================================================================
// ARCHITECTURAL CONCEPT: Get Today's Active Plan Query & Handler
// Retrieves today's active plan for the authenticated user based on Asia/Dubai calendar date.
// Returns 404/null if no plan has been generated yet for today.
// ==============================================================================================

using Gibo.Application.Common.Interfaces;
using Gibo.Application.Features.Planning.Commands.GenerateDailyPlan;
using MediatR;

namespace Gibo.Application.Features.Planning.Queries.GetTodayPlan;

public sealed record GetTodayPlanQuery(Guid UserId) : IRequest<DailyPlanDto?>;

public sealed class GetTodayPlanQueryHandler : IRequestHandler<GetTodayPlanQuery, DailyPlanDto?>
{
    private readonly IPlanRepository _planRepository;

    public GetTodayPlanQueryHandler(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<DailyPlanDto?> Handle(GetTodayPlanQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = await _planRepository.GetTodayPlanAsync(request.UserId, today, ct);

        if (plan == null)
            return null;

        var taskDtos = plan.Tasks.Select(t => new TaskInstanceDto(
            t.Id,
            t.TaskTemplateId,
            t.Domain,
            t.ActionVerb,
            t.Title,
            t.TargetValue,
            t.TargetUnit,
            t.EstimatedMinutes,
            t.State,
            t.EvidenceSource,
            t.XpAwarded,
            t.Rationale,
            t.IsStarterFallback,
            t.DisplayOrder)).ToList();

        return new DailyPlanDto(
            plan.Id,
            plan.PlanDate,
            plan.AlgorithmVersion,
            plan.UserEditsApplied,
            plan.TotalXpEarned,
            taskDtos);
    }
}
