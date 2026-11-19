// ==============================================================================================
// ARCHITECTURAL CONCEPT: Deterministic 5-Factor Task Ranking Engine
// Implements the core mathematical formulation from the Gibo specification:
//   FinalScore = (S_Goal * 0.30) + (S_Feas * 0.25) + (S_Barr * 0.20) + (S_Pref * 0.15) + (S_Nov * 0.10)
//
// Hard safety exclusions (physical contraindications & summer heat constraints) operate as
// absolute boolean gates, NEVER as soft score penalties.
// ==============================================================================================

using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;

namespace Gibo.Application.Features.Planning.Services;

public sealed class TaskRankingEngine : ITaskRankingEngine
{
    private const double WeightGoalRelevance = 0.30;
    private const double WeightFeasibility = 0.25;
    private const double WeightBarrierFit = 0.20;
    private const double WeightUserPreference = 0.15;
    private const double WeightNovelty = 0.10;

    public IReadOnlyList<ScoredTaskCandidate> RankCandidates(
        UserPreference preferences,
        IReadOnlyList<TaskTemplate> library,
        IReadOnlyList<Guid> recentCompletedTemplateIds,
        IReadOnlyList<Guid> yesterdayTemplateIds,
        IReadOnlyList<string> recentBarrierCategories)
    {
        if (preferences == null)
            throw new ArgumentNullException(nameof(preferences));

        if (library == null || library.Count == 0)
            return Array.Empty<ScoredTaskCandidate>();

        var candidates = new List<ScoredTaskCandidate>();

        foreach (var template in library)
        {
            if (!template.IsActive)
                continue;

            // STEP 1: Hard Safety & Accessibility Filter
            // If the template contains any contraindication tag present in user's accessibility constraints, drop immediately.
            if (template.SafetyTags.Any(tag => preferences.AccessibilityTags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
            {
                continue;
            }

            // Summer heat / weather constraint:
            // If user explicitly prefers indoor activity, drop outdoor-only templates.
            if (preferences.IndoorPreferred && template.RequiresOutdoor)
            {
                continue;
            }

            // STEP 2: Multi-Factor Scoring

            // 2a. Goal Relevance (Weight = 0.30)
            double sGoal = (template.Domain == preferences.PrimaryGoalDomain) ? 1.0 : 0.4;

            // 2b. Feasibility (Weight = 0.25)
            double sFeas = CalculateFeasibility(template.Difficulty, preferences.BaselineLevel);

            // 2c. Barrier Fit (Weight = 0.20)
            double sBarr = CalculateBarrierFit(template, recentBarrierCategories);

            // 2d. User Preference (Weight = 0.15)
            double sPref = CalculatePreference(template, preferences);

            // 2e. Novelty vs Continuity (Weight = 0.10)
            double sNov = CalculateNovelty(template.Id, yesterdayTemplateIds, recentCompletedTemplateIds);

            // STEP 3: Composite Transparent Score
            double finalScore = (sGoal * WeightGoalRelevance)
                              + (sFeas * WeightFeasibility)
                              + (sBarr * WeightBarrierFit)
                              + (sPref * WeightUserPreference)
                              + (sNov * WeightNovelty);

            candidates.Add(new ScoredTaskCandidate(
                template,
                Math.Round(finalScore, 4),
                sGoal,
                sFeas,
                sBarr,
                sPref,
                sNov));
        }

        return candidates
            .OrderByDescending(c => c.FinalScore)
            .ThenBy(c => c.Template.EstimatedMinutes)
            .ToList();
    }

    private static double CalculateFeasibility(TaskDifficulty difficulty, BaselineFitnessLevel baseline)
    {
        return baseline switch
        {
            BaselineFitnessLevel.Starter => difficulty switch
            {
                TaskDifficulty.Starter => 1.0,
                TaskDifficulty.Standard => 0.5,
                TaskDifficulty.Advanced => 0.1,
                _ => 0.5
            },
            BaselineFitnessLevel.Intermediate => difficulty switch
            {
                TaskDifficulty.Starter => 0.7,
                TaskDifficulty.Standard => 1.0,
                TaskDifficulty.Advanced => 0.6,
                _ => 0.7
            },
            BaselineFitnessLevel.Active => difficulty switch
            {
                TaskDifficulty.Starter => 0.5,
                TaskDifficulty.Standard => 0.8,
                TaskDifficulty.Advanced => 1.0,
                _ => 0.8
            },
            _ => 0.5
        };
    }

    private static double CalculateBarrierFit(TaskTemplate template, IReadOnlyList<string> recentBarriers)
    {
        if (recentBarriers == null || recentBarriers.Count == 0)
            return 0.7; // Neutral baseline

        // If user reported "TooDifficult" or "LowEnergy", boost Starter templates
        if (recentBarriers.Any(b => b.Contains("Difficult", StringComparison.OrdinalIgnoreCase) || b.Contains("Energy", StringComparison.OrdinalIgnoreCase)))
        {
            return template.Difficulty == TaskDifficulty.Starter ? 1.0 : 0.4;
        }

        // If user reported "TimingConflict", boost shorter duration tasks (<10 minutes)
        if (recentBarriers.Any(b => b.Contains("Timing", StringComparison.OrdinalIgnoreCase) || b.Contains("Time", StringComparison.OrdinalIgnoreCase)))
        {
            return template.EstimatedMinutes <= 10 ? 1.0 : 0.5;
        }

        // If user reported "AccessOrWeather", boost indoor templates
        if (recentBarriers.Any(b => b.Contains("Weather", StringComparison.OrdinalIgnoreCase) || b.Contains("Access", StringComparison.OrdinalIgnoreCase)))
        {
            return !template.RequiresOutdoor ? 1.0 : 0.2;
        }

        return 0.7;
    }

    private static double CalculatePreference(TaskTemplate template, UserPreference preferences)
    {
        double score = 0.6;

        // Indoor preference match
        if (preferences.IndoorPreferred && !template.RequiresOutdoor)
            score += 0.2;
        else if (!preferences.IndoorPreferred && template.RequiresOutdoor)
            score += 0.2;

        // Time window feasibility (estimated minutes aligns with micro-habit threshold)
        if (template.EstimatedMinutes <= 15)
            score += 0.2;

        return Math.Min(1.0, score);
    }

    private static double CalculateNovelty(
        Guid templateId,
        IReadOnlyList<Guid> yesterdayIds,
        IReadOnlyList<Guid> recentCompletedIds)
    {
        // Suppress exact repetition on consecutive days
        if (yesterdayIds.Contains(templateId))
            return 0.3;

        // Reward established habit completed 2-3 days ago (routine stability)
        if (recentCompletedIds.Contains(templateId))
            return 0.8;

        // Fresh unexplored candidate
        return 0.7;
    }
}
