using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;

namespace Gibo.Application.Common.Interfaces;

public interface IPlanRepository
{
    Task<UserPreference?> GetUserPreferencesAsync(Guid userId, CancellationToken ct = default);
    Task<DailyPlan?> GetTodayPlanAsync(Guid userId, DateOnly planDate, CancellationToken ct = default);
    Task<DailyPlan?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskTemplate>> GetActiveTemplatesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetRecentCompletedTemplateIdsAsync(Guid userId, int count, CancellationToken ct = default);
    Task SavePlanAsync(DailyPlan plan, CancellationToken ct = default);

    Task<TaskInstance?> GetTaskInstanceAsync(Guid taskInstanceId, CancellationToken ct = default);
    Task<TaskTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken ct = default);
    Task<CompletionEvent?> GetCompletionEventByClientEventIdAsync(Guid clientEventId, CancellationToken ct = default);
    Task RecordCompletionAsync(DailyPlan plan, TaskInstance task, CompletionEvent completionEvent, CancellationToken ct = default);
    Task SwapTaskAsync(DailyPlan plan, TaskInstance oldTask, TaskInstance replacementTask, CancellationToken ct = default);
    Task AdjustTargetAsync(DailyPlan plan, TaskInstance task, decimal newTarget, CancellationToken ct = default);
}
