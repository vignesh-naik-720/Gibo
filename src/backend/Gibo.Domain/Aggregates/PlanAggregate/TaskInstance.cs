// ==============================================================================================
// ARCHITECTURAL CONCEPT: Task Instance Entity & State Transition Invariants
// Represents an instantiated, actionable health micro-habit for a specific calendar day.
// Enforces Principle V (Device and Manual Parity): both manual completion and Health Connect
// wearable sync award identical 10 XP with zero second-class UX.
// Enforces Principle II: skips and expirations are neutral transitions with zero XP penalties.
// ==============================================================================================

using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Domain.Common;
using Gibo.Domain.Exceptions;

namespace Gibo.Domain.Aggregates.PlanAggregate;

public sealed class TaskInstance : Entity<Guid>
{
    public Guid PlanId { get; private set; }
    public Guid TaskTemplateId { get; private set; }
    public GoalDomain Domain { get; private set; }
    public string ActionVerb { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public decimal TargetValue { get; private set; }
    public string TargetUnit { get; private set; } = string.Empty;
    public int EstimatedMinutes { get; private set; }
    public TaskState State { get; private set; }
    public CompletionEvidenceSource? EvidenceSource { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int XpAwarded { get; private set; }
    public string Rationale { get; private set; } = string.Empty;
    public bool IsStarterFallback { get; private set; }
    public int DisplayOrder { get; private set; }

    private TaskInstance() { }

    public static TaskInstance Create(
        Guid id,
        Guid planId,
        TaskTemplate template,
        string locale,
        int displayOrder,
        bool isStarterFallback = false,
        decimal? customTarget = null)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));

        decimal target = customTarget ?? template.DefaultTargetValue;

        return new TaskInstance
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            PlanId = planId,
            TaskTemplateId = template.Id,
            Domain = template.Domain,
            ActionVerb = template.ActionVerb,
            Title = template.GetTitle(locale),
            TargetValue = target,
            TargetUnit = template.TargetUnit,
            EstimatedMinutes = template.EstimatedMinutes,
            State = TaskState.Scheduled,
            EvidenceSource = null,
            CompletedAt = null,
            XpAwarded = 0,
            Rationale = template.GetRationale(locale),
            IsStarterFallback = isStarterFallback,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Records completion with strict 10 XP award parity regardless of evidence source.
    /// </summary>
    public void Complete(CompletionEvidenceSource source)
    {
        if (State == TaskState.Completed)
            return; // Idempotent no-op

        State = TaskState.Completed;
        EvidenceSource = source;
        CompletedAt = DateTimeOffset.UtcNow;
        XpAwarded = 10; // Invariant: Exactly 10 XP for all complete actions
    }

    /// <summary>
    /// Records partial milestone completion awarding 5 XP.
    /// </summary>
    public void MarkPartial()
    {
        if (State == TaskState.Completed)
            return;

        State = TaskState.Partial;
        XpAwarded = 5; // Invariant: Exactly 5 XP for partial actions
    }

    /// <summary>
    /// Non-punitive skip transition. Zero XP lost; triggers empathetic recovery check-in.
    /// </summary>
    public void Skip()
    {
        if (State == TaskState.Completed)
            throw new DomainValidationException("Cannot skip an already completed task.");

        State = TaskState.Skipped;
        XpAwarded = 0;
    }

    /// <summary>
    /// Day rollover expiration. Completely neutral; no streak penalty assessed.
    /// </summary>
    public void Expire()
    {
        if (State == TaskState.Completed)
            return;

        State = TaskState.Expired;
    }

    public void Pause()
    {
        State = TaskState.Paused;
    }

    public void AdjustTarget(decimal newTarget)
    {
        if (newTarget <= 0)
            throw new DomainValidationException("Target value must be strictly positive.");

        if (State == TaskState.Completed)
            throw new DomainValidationException("Cannot adjust target on an already completed task.");

        TargetValue = newTarget;
    }
}
