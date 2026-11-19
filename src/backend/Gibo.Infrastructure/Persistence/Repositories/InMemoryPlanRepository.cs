using System.Collections.Concurrent;
using Gibo.Application.Common.Interfaces;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Infrastructure.Persistence.Seeding;

namespace Gibo.Infrastructure.Persistence.Repositories;

public sealed class InMemoryPlanRepository : IPlanRepository
{
    private readonly ConcurrentDictionary<Guid, DailyPlan> _plans = new();
    private readonly ConcurrentDictionary<Guid, TaskInstance> _tasks = new();
    private readonly ConcurrentDictionary<Guid, TaskTemplate> _templates = new();
    private readonly ConcurrentDictionary<Guid, CompletionEvent> _completionEventsByClientEventId = new();
    private readonly ConcurrentDictionary<Guid, UserPreference> _preferences = new();

    public InMemoryPlanRepository()
    {
        // Seed curated Abu Dhabi templates
        foreach (var t in TaskCatalogSeed.CuratedTemplates)
        {
            _templates[t.Id] = t;
        }
    }

    public Task<UserPreference?> GetUserPreferencesAsync(Guid userId, CancellationToken ct = default)
    {
        _preferences.TryGetValue(userId, out var pref);
        return Task.FromResult(pref);
    }

    public Task<DailyPlan?> GetTodayPlanAsync(Guid userId, DateOnly planDate, CancellationToken ct = default)
    {
        var plan = _plans.Values.FirstOrDefault(p => p.UserId == userId && p.PlanDate == planDate);
        return Task.FromResult(plan);
    }

    public Task<DailyPlan?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default)
    {
        _plans.TryGetValue(planId, out var plan);
        return Task.FromResult(plan);
    }

    public Task<IReadOnlyList<TaskTemplate>> GetActiveTemplatesAsync(CancellationToken ct = default)
    {
        var active = _templates.Values.Where(t => t.IsActive).ToList();
        return Task.FromResult<IReadOnlyList<TaskTemplate>>(active);
    }

    public Task<IReadOnlyList<Guid>> GetRecentCompletedTemplateIdsAsync(Guid userId, int count, CancellationToken ct = default)
    {
        var recent = _completionEventsByClientEventId.Values
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.RecordedAtUtc)
            .Take(count)
            .Select(e => e.TaskInstanceId)
            .ToList();

        return Task.FromResult<IReadOnlyList<Guid>>(recent);
    }

    public Task SavePlanAsync(DailyPlan plan, CancellationToken ct = default)
    {
        _plans[plan.Id] = plan;
        foreach (var task in plan.Tasks)
        {
            _tasks[task.Id] = task;
        }
        return Task.CompletedTask;
    }

    public Task<TaskInstance?> GetTaskInstanceAsync(Guid taskInstanceId, CancellationToken ct = default)
    {
        _tasks.TryGetValue(taskInstanceId, out var task);
        return Task.FromResult(task);
    }

    public Task<TaskTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken ct = default)
    {
        _templates.TryGetValue(templateId, out var template);
        return Task.FromResult(template);
    }

    public Task<CompletionEvent?> GetCompletionEventByClientEventIdAsync(Guid clientEventId, CancellationToken ct = default)
    {
        _completionEventsByClientEventId.TryGetValue(clientEventId, out var evt);
        return Task.FromResult(evt);
    }

    public Task RecordCompletionAsync(DailyPlan plan, TaskInstance task, CompletionEvent completionEvent, CancellationToken ct = default)
    {
        _completionEventsByClientEventId[completionEvent.ClientEventId] = completionEvent;
        _tasks[task.Id] = task;
        _plans[plan.Id] = plan;
        return Task.CompletedTask;
    }

    public Task SwapTaskAsync(DailyPlan plan, TaskInstance oldTask, TaskInstance replacementTask, CancellationToken ct = default)
    {
        _tasks.TryRemove(oldTask.Id, out _);
        _tasks[replacementTask.Id] = replacementTask;
        _plans[plan.Id] = plan;
        return Task.CompletedTask;
    }

    public Task AdjustTargetAsync(DailyPlan plan, TaskInstance task, decimal newTarget, CancellationToken ct = default)
    {
        _tasks[task.Id] = task;
        _plans[plan.Id] = plan;
        return Task.CompletedTask;
    }
}
