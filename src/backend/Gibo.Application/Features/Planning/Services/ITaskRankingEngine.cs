// ==============================================================================================
// ARCHITECTURAL CONCEPT: Multi-Factor Task Ranking Engine Interface & Candidate DTO
// Defines the contract for the rules-first, deterministic task selector.
// Adheres strictly to Principle III: tasks are scored via transparent mathematical weights,
// completely eliminating generative LLM hallucinations in health habit selection.
// ==============================================================================================

using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;

namespace Gibo.Application.Features.Planning.Services;

public sealed record ScoredTaskCandidate(
    TaskTemplate Template,
    double FinalScore,
    double GoalRelevanceScore,
    double FeasibilityScore,
    double BarrierFitScore,
    double PreferenceScore,
    double NoveltyScore);

public interface ITaskRankingEngine
{
    IReadOnlyList<ScoredTaskCandidate> RankCandidates(
        UserPreference preferences,
        IReadOnlyList<TaskTemplate> library,
        IReadOnlyList<Guid> recentCompletedTemplateIds,
        IReadOnlyList<Guid> yesterdayTemplateIds,
        IReadOnlyList<string> recentBarrierCategories);
}
