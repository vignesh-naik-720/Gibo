// ==============================================================================================
// ARCHITECTURAL CONCEPT: Daily Plan Aggregate Root
// The boundary of consistency for a user's scheduled health actions on a given calendar day.
// Enforces Principle I (User Agency): task counts are strictly bounded to [1, 3] actions.
// Enforces UserEditsApplied: once a user adjusts a target or swaps a task, background engine
// recalculations are forbidden from overriding user changes.
// ==============================================================================================

using Gibo.Domain.Common;
using Gibo.Domain.Events;
using Gibo.Domain.Exceptions;

namespace Gibo.Domain.Aggregates.PlanAggregate;

public sealed class DailyPlan : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public DateOnly PlanDate { get; private set; }
    public string AlgorithmVersion { get; private set; } = "v2.1-5factor";
    public bool UserEditsApplied { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<TaskInstance> _tasks = new();
    public IReadOnlyCollection<TaskInstance> Tasks => _tasks.AsReadOnly();

    public int TotalXpEarned => _tasks.Sum(t => t.XpAwarded);

    private DailyPlan() { }

    public static DailyPlan Create(
        Guid id,
        Guid userId,
        DateOnly planDate,
        string algorithmVersion = "v2.1-5factor")
    {
        if (userId == Guid.Empty)
            throw new DomainValidationException("Daily plan must belong to a valid user.");

        var plan = new DailyPlan
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            UserId = userId,
            PlanDate = planDate,
            AlgorithmVersion = algorithmVersion,
            UserEditsApplied = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return plan;
    }

    public void AddTask(TaskInstance task, int maxDailyActions)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        int clampedMax = Math.Clamp(maxDailyActions, 1, 3);
        if (_tasks.Count >= clampedMax)
            throw new DomainValidationException($"Cannot add more than {clampedMax} daily actions to this plan.");

        _tasks.Add(task);
    }

    public TaskInstance CompleteTask(Guid taskInstanceId, CompletionEvidenceSource source)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskInstanceId);
        if (task == null)
            throw new DomainValidationException($"Task instance {taskInstanceId} not found in this plan.");

        task.Complete(source);

        AddDomainEvent(new TaskCompletedEvent(
            task.Id,
            Id,
            UserId,
            source,
            task.XpAwarded,
            DateTimeOffset.UtcNow));

        return task;
    }

    public TaskInstance SwapTask(Guid taskInstanceId, TaskTemplate replacementTemplate, string locale)
    {
        var existingIndex = _tasks.FindIndex(t => t.Id == taskInstanceId);
        if (existingIndex == -1)
            throw new DomainValidationException($"Task instance {taskInstanceId} not found in this plan.");

        var existing = _tasks[existingIndex];
        if (existing.State == TaskState.Completed)
            throw new DomainValidationException("Cannot swap an already completed task.");

        var replacementInstance = TaskInstance.Create(
            Guid.NewGuid(),
            Id,
            replacementTemplate,
            locale,
            existing.DisplayOrder,
            isStarterFallback: false);

        _tasks[existingIndex] = replacementInstance;
        UserEditsApplied = true;

        return replacementInstance;
    }

    public void AdjustTaskTarget(Guid taskInstanceId, decimal newTarget)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskInstanceId);
        if (task == null)
            throw new DomainValidationException($"Task instance {taskInstanceId} not found in this plan.");

        task.AdjustTarget(newTarget);
        UserEditsApplied = true;
    }

    public void MarkGenerated()
    {
        AddDomainEvent(new PlanGeneratedEvent(Id, UserId, PlanDate, _tasks.Count, DateTimeOffset.UtcNow));
    }
}
