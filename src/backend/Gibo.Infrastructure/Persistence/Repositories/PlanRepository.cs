using Gibo.Application.Common.Interfaces;
using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Gibo.Infrastructure.Persistence.Repositories;

public sealed class PlanRepository : IPlanRepository
{
    private readonly IGiboDbContext _dbContext;

    public PlanRepository(IGiboDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserPreference?> GetUserPreferencesAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == UserId.From(userId), ct);

        return user?.Preferences;
    }

    public async Task<DailyPlan?> GetTodayPlanAsync(Guid userId, DateOnly planDate, CancellationToken ct = default)
    {
        return await _dbContext.DailyPlans
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PlanDate == planDate, ct);
    }

    public async Task<DailyPlan?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await _dbContext.DailyPlans
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == planId, ct);
    }

    public async Task<IReadOnlyList<TaskTemplate>> GetActiveTemplatesAsync(CancellationToken ct = default)
    {
        return await _dbContext.TaskTemplates
            .Where(t => t.IsActive)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetRecentCompletedTemplateIdsAsync(Guid userId, int count, CancellationToken ct = default)
    {
        return await _dbContext.CompletionEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.RecordedAtUtc)
            .Take(count)
            .Select(e => e.TaskInstanceId)
            .ToListAsync(ct);
    }

    public async Task SavePlanAsync(DailyPlan plan, CancellationToken ct = default)
    {
        var exists = await _dbContext.DailyPlans.AnyAsync(p => p.Id == plan.Id, ct);
        if (!exists)
        {
            _dbContext.DailyPlans.Add(plan);
        }
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<TaskInstance?> GetTaskInstanceAsync(Guid taskInstanceId, CancellationToken ct = default)
    {
        return await _dbContext.TaskInstances
            .FirstOrDefaultAsync(t => t.Id == taskInstanceId, ct);
    }

    public async Task<TaskTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken ct = default)
    {
        return await _dbContext.TaskTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive, ct);
    }

    public async Task<CompletionEvent?> GetCompletionEventByClientEventIdAsync(Guid clientEventId, CancellationToken ct = default)
    {
        return await _dbContext.CompletionEvents
            .FirstOrDefaultAsync(e => e.ClientEventId == clientEventId, ct);
    }

    public async Task RecordCompletionAsync(DailyPlan plan, TaskInstance task, CompletionEvent completionEvent, CancellationToken ct = default)
    {
        _dbContext.CompletionEvents.Add(completionEvent);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SwapTaskAsync(DailyPlan plan, TaskInstance oldTask, TaskInstance replacementTask, CancellationToken ct = default)
    {
        _dbContext.TaskInstances.Remove(oldTask);
        _dbContext.TaskInstances.Add(replacementTask);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task AdjustTargetAsync(DailyPlan plan, TaskInstance task, decimal newTarget, CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
