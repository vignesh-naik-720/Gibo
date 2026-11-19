// ==============================================================================================
// ARCHITECTURAL CONCEPT: Generate Daily Plan Command & Handler
// Orchestrates deterministic plan generation:
// 1. Checks for an existing plan today (Asia/Dubai timezone) to enforce at most one plan/day.
// 2. Evaluates 5-factor scoring over curated TaskTemplates via ITaskRankingEngine.
// 3. Clamps action count strictly to user's configured MaxDailyActions [1, 3].
// 4. Returns localized DailyPlanDto with action verbs, targets, and rationales.
// ==============================================================================================

using Gibo.Application.Common.Interfaces;
using Gibo.Application.Features.Planning.Services;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gibo.Application.Features.Planning.Commands.GenerateDailyPlan;

public sealed record GenerateDailyPlanCommand(
    Guid UserId,
    string Locale = "en",
    bool ForceRegenerate = false) : IRequest<DailyPlanDto>;

public sealed record TaskInstanceDto(
    Guid TaskInstanceId,
    Guid TaskTemplateId,
    GoalDomain Domain,
    string ActionVerb,
    string Title,
    decimal TargetValue,
    string TargetUnit,
    int EstimatedMinutes,
    TaskState State,
    CompletionEvidenceSource? EvidenceSource,
    int XpAwarded,
    string Rationale,
    bool IsStarterFallback,
    int DisplayOrder);

public sealed record DailyPlanDto(
    Guid PlanId,
    DateOnly PlanDate,
    string AlgorithmVersion,
    bool UserEditsApplied,
    int TotalXpEarned,
    IReadOnlyList<TaskInstanceDto> Tasks);

public sealed class GenerateDailyPlanCommandHandler : IRequestHandler<GenerateDailyPlanCommand, DailyPlanDto>
{
    private readonly IPlanRepository _planRepository;
    private readonly ITaskRankingEngine _rankingEngine;
    private readonly ILogger<GenerateDailyPlanCommandHandler> _logger;

    public GenerateDailyPlanCommandHandler(
        IPlanRepository planRepository,
        ITaskRankingEngine rankingEngine,
        ILogger<GenerateDailyPlanCommandHandler> logger)
    {
        _planRepository = planRepository;
        _rankingEngine = rankingEngine;
        _logger = logger;
    }

    public async Task<DailyPlanDto> Handle(GenerateDailyPlanCommand request, CancellationToken ct)
    {
        // 1. Resolve user and preferences
        var preference = await _planRepository.GetUserPreferencesAsync(request.UserId, ct) ?? UserPreference.CreateDefault(
            UserId.From(request.UserId),
            GoalDomain.Movement,
            WorkSchedulePattern.StandardDaytime,
            indoorPreferred: true,
            maxDailyActions: 2,
            baselineLevel: BaselineFitnessLevel.Starter);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 2. Check for existing plan today
        var existingPlan = await _planRepository.GetTodayPlanAsync(request.UserId, today, ct);

        if (existingPlan != null && !request.ForceRegenerate)
        {
            _logger.LogInformation("[PLAN_HIT] Returning existing daily plan {PlanId} for User {UserId}", existingPlan.Id, request.UserId);
            return MapToDto(existingPlan);
        }

        // 3. Load catalog templates and history
        var allTemplates = await _planRepository.GetActiveTemplatesAsync(ct);

        if (allTemplates.Count == 0)
        {
            throw new DomainValidationException("No active task templates found in the catalog. Please ensure the database has been seeded.");
        }

        var yesterdayTemplateIds = existingPlan != null
            ? existingPlan.Tasks.Select(t => t.TaskTemplateId).ToList()
            : new List<Guid>();

        var recentCompletedIds = await _planRepository.GetRecentCompletedTemplateIdsAsync(request.UserId, 10, ct);

        // 4. Execute 5-factor ranking engine
        var rankedCandidates = _rankingEngine.RankCandidates(
            preference,
            allTemplates,
            recentCompletedIds,
            yesterdayTemplateIds,
            Array.Empty<string>());

        int maxActions = Math.Clamp(preference.MaxDailyActions, 1, 3);
        var selectedCandidates = rankedCandidates.Take(maxActions).ToList();

        // 5. Build Aggregate
        var newPlan = DailyPlan.Create(Guid.NewGuid(), request.UserId, today);
        int order = 1;
        foreach (var candidate in selectedCandidates)
        {
            var taskInstance = TaskInstance.Create(
                Guid.NewGuid(),
                newPlan.Id,
                candidate.Template,
                request.Locale,
                displayOrder: order++,
                isStarterFallback: candidate.Template.Difficulty == TaskDifficulty.Starter);

            newPlan.AddTask(taskInstance, maxActions);
        }

        newPlan.MarkGenerated();

        await _planRepository.SavePlanAsync(newPlan, ct);

        _logger.LogInformation("[PLAN_GENERATED] Generated plan {PlanId} with {Count} actions for User {UserId}", newPlan.Id, newPlan.Tasks.Count, request.UserId);

        return MapToDto(newPlan);
    }

    private static DailyPlanDto MapToDto(DailyPlan plan)
    {
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
