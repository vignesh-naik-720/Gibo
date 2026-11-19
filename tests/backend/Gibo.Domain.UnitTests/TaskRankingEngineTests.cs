// ==============================================================================================
// Unit Tests: Deterministic 5-Factor Task Ranking Engine
// Validates mathematical weighting, hard safety exclusions, and weather constraint filtering.
// ==============================================================================================

using System;
using System.Linq;
using Gibo.Application.Features.Planning.Services;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Infrastructure.Persistence.Seeding;

namespace Gibo.Domain.UnitTests;

public static class TaskRankingEngineTests
{
    public static void RunAll()
    {
        Console.WriteLine("\nRunning TaskRankingEngineTests...");

        Test_HardSafetyFilter_ExcludesContraindicatedTemplates();
        Test_IndoorPreference_ExcludesOutdoorTemplates();
        Test_GoalRelevance_AwardsHigherScoreToMatchingDomain();
        Test_Feasibility_PrioritizesStarterTasksForStarterBaseline();
        Test_Novelty_PenalizesTasksCompletedYesterday();

        Console.WriteLine("ALL TaskRankingEngineTests PASSED SUCCESSFULLY (5/5)!");
    }

    private static UserPreference CreatePref(
        GoalDomain domain,
        bool indoor,
        BaselineFitnessLevel baseline,
        int cap = 2,
        string[]? tags = null)
    {
        return new UserPreference(
            PreferenceId.New(),
            UserId.New(),
            domain,
            WorkSchedulePattern.StandardDaytime,
            indoorPreferred: indoor,
            maxDailyActions: cap,
            quietHoursStart: TimeSpan.FromHours(22),
            quietHoursEnd: TimeSpan.FromHours(7),
            baselineLevel: baseline,
            accessibilityTags: tags);
    }

    private static void Test_HardSafetyFilter_ExcludesContraindicatedTemplates()
    {
        var engine = new TaskRankingEngine();
        var pref = CreatePref(GoalDomain.Movement, indoor: false, BaselineFitnessLevel.Starter, tags: new[] { "Seated" });

        var templates = TaskCatalogSeed.CuratedTemplates;
        var ranked = engine.RankCandidates(pref, templates, Array.Empty<Guid>(), Array.Empty<Guid>(), Array.Empty<string>());

        // Any template tagged with "Seated" MUST be excluded
        bool containsSeated = ranked.Any(c => c.Template.SafetyTags.Contains("Seated"));
        Assert(!containsSeated, "Templates tagged with contraindicated tags must be excluded from ranking.");
        Console.WriteLine("✓ Test_HardSafetyFilter_ExcludesContraindicatedTemplates passed.");
    }

    private static void Test_IndoorPreference_ExcludesOutdoorTemplates()
    {
        var engine = new TaskRankingEngine();
        var pref = CreatePref(GoalDomain.Movement, indoor: true, BaselineFitnessLevel.Intermediate);

        var templates = TaskCatalogSeed.CuratedTemplates;
        var ranked = engine.RankCandidates(pref, templates, Array.Empty<Guid>(), Array.Empty<Guid>(), Array.Empty<string>());

        // Outdoor templates (e.g. Corniche stroll) MUST be excluded
        bool containsOutdoor = ranked.Any(c => c.Template.RequiresOutdoor);
        Assert(!containsOutdoor, "Outdoor-only templates must be excluded when indoor preference is active.");
        Console.WriteLine("✓ Test_IndoorPreference_ExcludesOutdoorTemplates passed.");
    }

    private static void Test_GoalRelevance_AwardsHigherScoreToMatchingDomain()
    {
        var engine = new TaskRankingEngine();
        var pref = CreatePref(GoalDomain.SleepRoutine, indoor: false, BaselineFitnessLevel.Starter);

        var templates = TaskCatalogSeed.CuratedTemplates;
        var ranked = engine.RankCandidates(pref, templates, Array.Empty<Guid>(), Array.Empty<Guid>(), Array.Empty<string>());

        // Top ranked item must be from the Sleep Routine domain
        Assert(ranked.First().Template.Domain == GoalDomain.SleepRoutine, "Top ranked candidate must match primary goal domain.");
        Console.WriteLine("✓ Test_GoalRelevance_AwardsHigherScoreToMatchingDomain passed.");
    }

    private static void Test_Feasibility_PrioritizesStarterTasksForStarterBaseline()
    {
        var engine = new TaskRankingEngine();
        var pref = CreatePref(GoalDomain.Movement, indoor: false, BaselineFitnessLevel.Starter, cap: 1);

        var templates = TaskCatalogSeed.CuratedTemplates;
        var ranked = engine.RankCandidates(pref, templates, Array.Empty<Guid>(), Array.Empty<Guid>(), Array.Empty<string>());

        var top = ranked.First();
        Assert(top.Template.Difficulty == TaskDifficulty.Starter, "Starter baseline user must receive Starter difficulty top task.");
        Console.WriteLine("✓ Test_Feasibility_PrioritizesStarterTasksForStarterBaseline passed.");
    }

    private static void Test_Novelty_PenalizesTasksCompletedYesterday()
    {
        var engine = new TaskRankingEngine();
        var pref = CreatePref(GoalDomain.Movement, indoor: false, BaselineFitnessLevel.Starter);

        var templates = TaskCatalogSeed.CuratedTemplates;
        var mallWalkId = Guid.Parse("10000000-0000-0000-0000-000000000001");

        // Rank with yesterday having mallWalkId
        var rankedYesterday = engine.RankCandidates(pref, templates, Array.Empty<Guid>(), new[] { mallWalkId }, Array.Empty<string>());
        var candidateYesterday = rankedYesterday.First(c => c.Template.Id == mallWalkId);

        // Rank without yesterday having mallWalkId
        var rankedFresh = engine.RankCandidates(pref, templates, Array.Empty<Guid>(), Array.Empty<Guid>(), Array.Empty<string>());
        var candidateFresh = rankedFresh.First(c => c.Template.Id == mallWalkId);

        Assert(candidateYesterday.FinalScore < candidateFresh.FinalScore, "Task scheduled yesterday must receive lower novelty score than fresh task.");
        Console.WriteLine("✓ Test_Novelty_PenalizesTasksCompletedYesterday passed.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"ASSERTION FAILED: {message}");
    }
}
