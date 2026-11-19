using Gibo.Application.Features.Onboarding.Commands.RegisterOnboardingUser;
using Gibo.Application.Features.Onboarding.Queries.GetFirstPlanPreview;
using Gibo.Application.Features.Onboarding.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gibo.Api.Controllers.v1;

[ApiController]
[Route("api/v1/onboarding")]
[Produces("application/json")]
public sealed class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator _mediator)
    {
        this._mediator = _mediator;
    }

    /// <summary>
    /// Register a new user profile, record granular consent, and initialize daily preferences.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OnboardingResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] RegisterOnboardingUserRequest request,
        CancellationToken ct)
    {
        var command = new RegisterOnboardingUserCommand(
            IdempotencyKey: idempotencyKey,
            IsAdultConfirmed: request.IsAdultConfirmed,
            PreferredLocale: request.PreferredLocale,
            TimezoneId: request.TimezoneId,
            PrimaryGoalDomain: request.PrimaryGoalDomain,
            WorkPattern: request.WorkPattern,
            IndoorPreferred: request.IndoorPreferred,
            MaxDailyActions: request.MaxDailyActions,
            QuietHoursStart: TimeSpan.TryParse(request.QuietHoursStart, out var start) ? start : TimeSpan.FromHours(22),
            QuietHoursEnd: TimeSpan.TryParse(request.QuietHoursEnd, out var end) ? end : TimeSpan.FromHours(7),
            BaselineLevel: request.BaselineFitnessLevel,
            AccessibilityTags: request.AccessibilityTags ?? Array.Empty<string>(),
            Consents: request.Consents.Select(c => new ConsentItemDto(c.Category, c.Granted, c.PolicyVersion)).ToList());

        var result = await _mediator.Send(command, ct);

        return CreatedAtAction(nameof(Register), new { userId = result.UserId }, result);
    }

    /// <summary>
    /// Preview the first recommended starter action matching the user's constraints without persisting.
    /// </summary>
    [HttpPost("first-plan-preview")]
    [ProducesResponseType(typeof(PlanCandidateDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFirstPlanPreview(
        [FromBody] FirstPlanPreviewRequestDto request,
        CancellationToken ct)
    {
        var query = new GetFirstPlanPreviewQuery(
            GoalDomain: request.GoalDomain,
            BaselineLevel: request.BaselineFitnessLevel,
            IndoorPreferred: request.IndoorPreferred,
            Locale: request.Locale ?? "en");

        var result = await _mediator.Send(query, ct);

        return Ok(result);
    }
}

public sealed record RegisterOnboardingUserRequest(
    bool IsAdultConfirmed,
    string PreferredLocale,
    string TimezoneId,
    Domain.Aggregates.UserAggregate.GoalDomain PrimaryGoalDomain,
    Domain.Aggregates.UserAggregate.WorkSchedulePattern WorkPattern,
    bool IndoorPreferred,
    int MaxDailyActions,
    string? QuietHoursStart,
    string? QuietHoursEnd,
    Domain.Aggregates.UserAggregate.BaselineFitnessLevel BaselineFitnessLevel,
    string[]? AccessibilityTags,
    List<ConsentInputItem> Consents);

public sealed record ConsentInputItem(
    Domain.Aggregates.UserAggregate.ConsentCategory Category,
    bool Granted,
    string PolicyVersion);

public sealed record FirstPlanPreviewRequestDto(
    Domain.Aggregates.UserAggregate.GoalDomain GoalDomain,
    Domain.Aggregates.UserAggregate.BaselineFitnessLevel BaselineFitnessLevel,
    bool IndoorPreferred,
    string? Locale);
