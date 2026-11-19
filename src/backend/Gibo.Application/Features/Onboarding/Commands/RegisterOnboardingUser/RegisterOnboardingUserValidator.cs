using FluentValidation;
using Gibo.Domain.Aggregates.UserAggregate;

namespace Gibo.Application.Features.Onboarding.Commands.RegisterOnboardingUser;

public sealed class RegisterOnboardingUserValidator : AbstractValidator<RegisterOnboardingUserCommand>
{
    public RegisterOnboardingUserValidator()
    {
        RuleFor(x => x.IsAdultConfirmed)
            .Equal(true)
            .WithMessage("Gibo is currently restricted to adults (18+). Underage registration is not permitted.");

        RuleFor(x => x.PreferredLocale)
            .NotEmpty()
            .Must(code => code == "en" || code == "ar" || code == "hi")
            .WithMessage("Supported locales are 'en', 'ar', and 'hi'.");

        RuleFor(x => x.MaxDailyActions)
            .InclusiveBetween(1, 3)
            .WithMessage("Max daily actions must be between 1 and 3.");

        RuleFor(x => x.Consents)
            .NotEmpty()
            .Must(consents => consents != null && consents.Any(c => c.Category == ConsentCategory.CoreProfile && c.Granted))
            .WithMessage("Consent for CoreProfile is required to initialize an account under UAE PDPL.");
    }
}
