// ==============================================================================================
// Unit Tests: Task Completion & XP Award Parity
// Enforces Principle V (Device and Manual Parity): Both manual check-offs and Health Connect
// background sync MUST award exactly 10 XP with zero disparity.
// ==============================================================================================

using System;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Domain.Events;
using Gibo.Infrastructure.Persistence.Seeding;

namespace Gibo.Domain.UnitTests;

public static class TaskCompletionTests
{
    public static void RunAll()
    {
        Console.WriteLine("\nRunning TaskCompletionTests...");

        Test_ManualCompletion_AwardsExactly10Xp();
        Test_HealthConnectCompletion_AwardsExactly10Xp();
        Test_PartialCompletion_AwardsExactly5Xp();
        Test_SkipTask_Awards0Xp();

        Console.WriteLine("ALL TaskCompletionTests PASSED SUCCESSFULLY (4/4)!");
    }

    private static void Test_ManualCompletion_AwardsExactly10Xp()
    {
        var plan = DailyPlan.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));
        var template = TaskCatalogSeed.CuratedTemplates[0];
        var task = TaskInstance.Create(Guid.NewGuid(), plan.Id, template, "en", 1);
        plan.AddTask(task, 2);

        plan.CompleteTask(task.Id, CompletionEvidenceSource.Manual);

        Assert(task.State == TaskState.Completed, "Task state must be Completed.");
        Assert(task.XpAwarded == 10, "Manual completion must award exactly 10 XP.");
        Assert(task.EvidenceSource == CompletionEvidenceSource.Manual, "EvidenceSource must be Manual.");
        Assert(plan.TotalXpEarned == 10, "Plan TotalXpEarned must be 10.");

        var completedEvent = plan.DomainEvents.OfType<TaskCompletedEvent>().FirstOrDefault();
        Assert(completedEvent != null, "TaskCompletedEvent domain event must be raised.");
        Assert(completedEvent!.XpAwarded == 10, "Event XpAwarded must be 10.");
        Console.WriteLine("✓ Test_ManualCompletion_AwardsExactly10Xp passed.");
    }

    private static void Test_HealthConnectCompletion_AwardsExactly10Xp()
    {
        var plan = DailyPlan.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));
        var template = TaskCatalogSeed.CuratedTemplates[0];
        var task = TaskInstance.Create(Guid.NewGuid(), plan.Id, template, "en", 1);
        plan.AddTask(task, 2);

        plan.CompleteTask(task.Id, CompletionEvidenceSource.HealthConnect);

        Assert(task.State == TaskState.Completed, "Task state must be Completed.");
        Assert(task.XpAwarded == 10, "Health Connect completion must award exactly 10 XP.");
        Assert(task.EvidenceSource == CompletionEvidenceSource.HealthConnect, "EvidenceSource must be HealthConnect.");
        Console.WriteLine("✓ Test_HealthConnectCompletion_AwardsExactly10Xp passed.");
    }

    private static void Test_PartialCompletion_AwardsExactly5Xp()
    {
        var template = TaskCatalogSeed.CuratedTemplates[0];
        var task = TaskInstance.Create(Guid.NewGuid(), Guid.NewGuid(), template, "en", 1);

        task.MarkPartial();

        Assert(task.State == TaskState.Partial, "Task state must be Partial.");
        Assert(task.XpAwarded == 5, "Partial milestone completion must award exactly 5 XP.");
        Console.WriteLine("✓ Test_PartialCompletion_AwardsExactly5Xp passed.");
    }

    private static void Test_SkipTask_Awards0Xp()
    {
        var template = TaskCatalogSeed.CuratedTemplates[0];
        var task = TaskInstance.Create(Guid.NewGuid(), Guid.NewGuid(), template, "en", 1);

        task.Skip();

        Assert(task.State == TaskState.Skipped, "Task state must be Skipped.");
        Assert(task.XpAwarded == 0, "Skipped task must award 0 XP.");
        Console.WriteLine("✓ Test_SkipTask_Awards0Xp passed.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"ASSERTION FAILED: {message}");
    }
}
