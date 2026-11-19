using Gibo.Application.Common.Behaviors;
using Gibo.Application.Features.Onboarding.Services;
using Gibo.Domain.Aggregates.UserAggregate;
using MediatR;

namespace Gibo.Application.Features.Onboarding.Commands.RegisterOnboardingUser;

public sealed record ConsentItemDto(
    ConsentCategory Category,
    bool Granted,
    string PolicyVersion);

public sealed record RegisterOnboardingUserCommand(
    string? IdempotencyKey,
    bool IsAdultConfirmed,
    string PreferredLocale,
    string TimezoneId,
    GoalDomain PrimaryGoalDomain,
    WorkSchedulePattern WorkPattern,
    bool IndoorPreferred,
    int MaxDailyActions,
    TimeSpan QuietHoursStart,
    TimeSpan QuietHoursEnd,
    BaselineFitnessLevel BaselineLevel,
    IReadOnlyList<string> AccessibilityTags,
    IReadOnlyList<ConsentItemDto> Consents) : IRequest<OnboardingResultDto>, IIdempotentRequest;

public sealed record OnboardingResultDto(
    Guid UserId,
    string Status,
    DateTimeOffset RegisteredAt,
    PlanCandidateDto FirstPlan);
