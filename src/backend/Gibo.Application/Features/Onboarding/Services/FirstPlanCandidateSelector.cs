using Gibo.Domain.Aggregates.UserAggregate;

namespace Gibo.Application.Features.Onboarding.Services;

public sealed record PlanCandidateDto(
    Guid TaskTemplateId,
    string Instruction,
    string Verb,
    string Target,
    int EstimatedMinutes,
    string EvidenceMode,
    string WhyItFits);

public interface IFirstPlanCandidateSelector
{
    PlanCandidateDto SelectStarterCandidate(
        GoalDomain domain, 
        BaselineFitnessLevel baseline, 
        bool indoorPreferred, 
        IReadOnlyList<string> accessibilityTags,
        string locale);
}

/// <summary>
/// Domain Service: FirstPlanCandidateSelector
/// Selects an achievable, curated starter micro-task (< 10 mins) tailored to the user's
/// contextual constraints, mobility comfort, and climate preferences.
/// </summary>
public sealed class FirstPlanCandidateSelector : IFirstPlanCandidateSelector
{
    public PlanCandidateDto SelectStarterCandidate(
        GoalDomain domain,
        BaselineFitnessLevel baseline,
        bool indoorPreferred,
        IReadOnlyList<string> accessibilityTags,
        string locale)
    {
        bool hasJointSensitivity = accessibilityTags.Any(t => t.Equals("LowImpact", StringComparison.OrdinalIgnoreCase));
        bool isArabic = locale.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        return domain switch
        {
            GoalDomain.Movement => SelectMovementCandidate(indoorPreferred, hasJointSensitivity, isArabic),
            GoalDomain.SleepRoutine => SelectSleepCandidate(isArabic),
            GoalDomain.Nutrition => SelectNutritionCandidate(isArabic),
            _ => SelectMovementCandidate(true, false, isArabic)
        };
    }

    private static PlanCandidateDto SelectMovementCandidate(bool indoor, bool jointSensitivity, bool isArabic)
    {
        if (jointSensitivity)
        {
            return new PlanCandidateDto(
                TaskTemplateId: Guid.Parse("11111111-2222-3333-4444-555555555551"),
                Instruction: isArabic
                    ? "ممارسة 5 دقائق من حركات الإطالة الخفيفة أثناء الجلوس بعد الغداء"
                    : "Do 5 minutes of gentle seated stretching after lunch",
                Verb: isArabic ? "إطالة" : "Stretch",
                Target: isArabic ? "5 دقائق" : "5 minutes",
                EstimatedMinutes: 5,
                EvidenceMode: "Manual",
                WhyItFits: isArabic
                    ? "يناسب حساسيتك للمفاصل ويوفر حركة آمنة ومنعشة في الأماكن المغلقة."
                    : "Gentle on joints, providing a safe seated routine that fits your indoor preference.");
        }

        return new PlanCandidateDto(
            TaskTemplateId: Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
            Instruction: isArabic
                ? "المشي الخفيف لمدة 7 دقائق داخل المنزل أو المركز التجاري بعد الغداء"
                : "Take a 7-minute gentle indoor walk after lunch",
            Verb: isArabic ? "مشي" : "Walk",
            Target: isArabic ? "7 دقائق" : "7 minutes",
            EstimatedMinutes: 7,
            EvidenceMode: "Manual",
            WhyItFits: isArabic
                ? "نشاط بسيط وممتع في مكان مكيف يناسب جدولك اليومي في أبوظبي."
                : "Matches your indoor preference and gives you an easy win to establish the habit.");
    }

    private static PlanCandidateDto SelectSleepCandidate(bool isArabic)
    {
        return new PlanCandidateDto(
            TaskTemplateId: Guid.Parse("22222222-3333-4444-5555-666666666662"),
            Instruction: isArabic
                ? "بدء إشارة استرخاء لمدة 10 دقائق بدون شاشات قبل موعد النوم"
                : "Start a 10-minute screen-free wind-down cue before bed",
            Verb: isArabic ? "استرخاء" : "Wind-down",
            Target: isArabic ? "10 دقائق" : "10 minutes",
            EstimatedMinutes: 10,
            EvidenceMode: "Manual",
            WhyItFits: isArabic
                ? "يساعد عقلك وجسدك على الاستعداد لنوم عميق ومريح دون إجهاد."
                : "Prepares your body for deep rest by reducing blue light and late stimulation.");
    }

    private static PlanCandidateDto SelectNutritionCandidate(bool isArabic)
    {
        return new PlanCandidateDto(
            TaskTemplateId: Guid.Parse("33333333-4444-5555-6666-777777777773"),
            Instruction: isArabic
                ? "إضافة حصة واحدة من الخضار الطازجة أو الورقيات إلى وجبة العشاء"
                : "Add one serving of vegetables or salad to dinner",
            Verb: isArabic ? "إضافة" : "Add",
            Target: isArabic ? "حصة واحدة" : "1 serving",
            EstimatedMinutes: 5,
            EvidenceMode: "Manual",
            WhyItFits: isArabic
                ? "يركز على إضافة عناصر مغذية ولذيذة بدلاً من فرض قيود غذائية قاسية."
                : "Focuses on positive addition of nourishing foods rather than restrictive dieting.");
    }
}
