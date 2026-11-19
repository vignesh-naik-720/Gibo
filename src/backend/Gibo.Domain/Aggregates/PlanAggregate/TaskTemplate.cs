// ==============================================================================================
// ARCHITECTURAL CONCEPT: Curated Task Template (Evidence-Based Catalog)
// Adheres to Principle III (Evidence Before AI Fluency). Actions are NEVER generated randomly
// by an LLM at runtime; they are drawn from clinician-reviewed TaskTemplate definitions.
// Supports multi-language localization (English, Arabic, Hindi) and hard safety exclusions.
// ==============================================================================================

using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Domain.Common;

namespace Gibo.Domain.Aggregates.PlanAggregate;

public sealed class TaskTemplate : Entity<Guid>
{
    public int Version { get; private set; }
    public GoalDomain Domain { get; private set; }
    public string ActionVerb { get; private set; } = string.Empty;
    public MeasurementType MeasurementType { get; private set; }
    public TaskDifficulty Difficulty { get; private set; }
    public decimal DefaultTargetValue { get; private set; }
    public string TargetUnit { get; private set; } = string.Empty;
    public int EstimatedMinutes { get; private set; }
    public EvidenceMode AllowedEvidenceModes { get; private set; }
    public bool RequiresOutdoor { get; private set; }
    public List<string> SafetyTags { get; private set; } = new();
    public string BCTCategory { get; private set; } = string.Empty;

    // Trilingual localized instructions
    public string TitleEn { get; private set; } = string.Empty;
    public string TitleAr { get; private set; } = string.Empty;
    public string TitleHi { get; private set; } = string.Empty;

    // Trilingual "Why this fits you" rationales
    public string RationaleEn { get; private set; } = string.Empty;
    public string RationaleAr { get; private set; } = string.Empty;
    public string RationaleHi { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private TaskTemplate() { }

    public static TaskTemplate Create(
        Guid id,
        int version,
        GoalDomain domain,
        string actionVerb,
        MeasurementType measurementType,
        TaskDifficulty difficulty,
        decimal defaultTargetValue,
        string targetUnit,
        int estimatedMinutes,
        EvidenceMode allowedEvidenceModes,
        bool requiresOutdoor,
        IEnumerable<string> safetyTags,
        string bctCategory,
        string titleEn,
        string titleAr,
        string titleHi,
        string rationaleEn,
        string rationaleAr,
        string rationaleHi)
    {
        return new TaskTemplate
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            Version = version > 0 ? version : 1,
            Domain = domain,
            ActionVerb = actionVerb,
            MeasurementType = measurementType,
            Difficulty = difficulty,
            DefaultTargetValue = defaultTargetValue,
            TargetUnit = targetUnit,
            EstimatedMinutes = estimatedMinutes,
            AllowedEvidenceModes = allowedEvidenceModes,
            RequiresOutdoor = requiresOutdoor,
            SafetyTags = new List<string>(safetyTags),
            BCTCategory = bctCategory,
            TitleEn = titleEn,
            TitleAr = titleAr,
            TitleHi = titleHi,
            RationaleEn = rationaleEn,
            RationaleAr = rationaleAr,
            RationaleHi = rationaleHi,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public string GetTitle(string locale)
    {
        return locale?.ToLowerInvariant() switch
        {
            "ar" or "ar-ae" => TitleAr,
            "hi" or "hi-in" => TitleHi,
            _ => TitleEn
        };
    }

    public string GetRationale(string locale)
    {
        return locale?.ToLowerInvariant() switch
        {
            "ar" or "ar-ae" => RationaleAr,
            "hi" or "hi-in" => RationaleHi,
            _ => RationaleEn
        };
    }
}
