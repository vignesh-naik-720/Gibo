using FluentValidation;
using Gibo.Application.Features.Onboarding.Services;
using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gibo.Application.Features.Onboarding.Commands.RegisterOnboardingUser;

public sealed class RegisterOnboardingUserCommandHandler : IRequestHandler<RegisterOnboardingUserCommand, OnboardingResultDto>
{
    private readonly IValidator<RegisterOnboardingUserCommand> _validator;
    private readonly IFirstPlanCandidateSelector _planSelector;
    private readonly ILogger<RegisterOnboardingUserCommandHandler> _logger;

    public RegisterOnboardingUserCommandHandler(
        IValidator<RegisterOnboardingUserCommand> validator,
        IFirstPlanCandidateSelector planSelector,
        ILogger<RegisterOnboardingUserCommandHandler> logger)
    {
        _validator = validator;
        _planSelector = planSelector;
        _logger = logger;
    }

    public Task<OnboardingResultDto> Handle(RegisterOnboardingUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Fluent Validation
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            throw new DomainValidationException(firstError.ErrorMessage, "VALIDATION_ERROR");
        }

        // 2. Initialize Preferences
        var userId = UserId.New();
        var preferences = new UserPreference(
            PreferenceId.New(),
            userId,
            request.PrimaryGoalDomain,
            request.WorkPattern,
            request.IndoorPreferred,
            request.MaxDailyActions,
            request.QuietHoursStart,
            request.QuietHoursEnd,
            request.BaselineLevel,
            request.AccessibilityTags);

        // 3. Create User Aggregate Root
        var user = User.CreateInitial(
            userId,
            request.IsAdultConfirmed,
            Locale.From(request.PreferredLocale),
            request.TimezoneId,
            preferences);

        // 4. Record Granular Consent Ledger (UAE PDPL Compliance)
        foreach (var consent in request.Consents.Where(c => c.Granted))
        {
            user.RecordConsent(consent.Category, $"User granted {consent.Category} during onboarding", consent.PolicyVersion);
        }

        // 5. Bind Initial Goal
        string initialGoalText = request.PrimaryGoalDomain switch
        {
            GoalDomain.Movement => "Build consistent gentle daily walking and movement",
            GoalDomain.SleepRoutine => "Establish a calming evening wind-down routine",
            GoalDomain.Nutrition => "Add mindful hydration and nourishing foods to meals",
            _ => "Build healthier daily habits"
        };
        user.AddGoal(request.PrimaryGoalDomain, initialGoalText);

        // 6. Generate First Achievable Daily Plan Preview
        var starterPlan = _planSelector.SelectStarterCandidate(
            request.PrimaryGoalDomain,
            request.BaselineLevel,
            request.IndoorPreferred,
            request.AccessibilityTags,
            request.PreferredLocale);

        _logger.LogInformation(
            "[ONBOARDING_COMPLETED] Created User {UserId} with Goal {Domain} in Timezone {Timezone}",
            user.Id,
            request.PrimaryGoalDomain,
            request.TimezoneId);

        return Task.FromResult(new OnboardingResultDto(
            UserId: user.Id.Value,
            Status: user.Status.ToString(),
            RegisteredAt: user.CreatedAt,
            FirstPlan: starterPlan));
    }
}
