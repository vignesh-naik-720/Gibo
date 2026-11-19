using Gibo.Application.Features.Onboarding.Services;
using Gibo.Domain.Aggregates.UserAggregate;
using MediatR;

namespace Gibo.Application.Features.Onboarding.Queries.GetFirstPlanPreview;

public sealed record GetFirstPlanPreviewQuery(
    GoalDomain GoalDomain,
    BaselineFitnessLevel BaselineLevel,
    bool IndoorPreferred,
    string Locale) : IRequest<PlanCandidateDto>;

public sealed class GetFirstPlanPreviewQueryHandler : IRequestHandler<GetFirstPlanPreviewQuery, PlanCandidateDto>
{
    private readonly IFirstPlanCandidateSelector _planSelector;

    public GetFirstPlanPreviewQueryHandler(IFirstPlanCandidateSelector planSelector)
    {
        _planSelector = planSelector;
    }

    public Task<PlanCandidateDto> Handle(GetFirstPlanPreviewQuery request, CancellationToken cancellationToken)
    {
        var plan = _planSelector.SelectStarterCandidate(
            request.GoalDomain,
            request.BaselineLevel,
            request.IndoorPreferred,
            Array.Empty<string>(),
            request.Locale);

        return Task.FromResult(plan);
    }
}
