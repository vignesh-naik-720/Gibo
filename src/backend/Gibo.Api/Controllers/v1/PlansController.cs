// ==============================================================================================
// ARCHITECTURAL CONCEPT: Plans Controller
// Exposes REST endpoints for today's active plan retrieval and deterministic plan generation.
// Enforces Idempotency-Key header on plan generation to prevent duplicate creations on mobile reconnect.
// ==============================================================================================

using Gibo.Application.Features.Planning.Commands.GenerateDailyPlan;
using Gibo.Application.Features.Planning.Queries.GetTodayPlan;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gibo.Api.Controllers.v1;

[ApiController]
[Route("api/v1/plans")]
[Produces("application/json")]
public sealed class PlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves today's active plan for the authenticated or demo user.
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(DailyPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTodayPlan(
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        CancellationToken ct)
    {
        var targetUserId = userId ?? Guid.Parse("11111111-1111-1111-1111-111111111111"); // Demo fallback
        var plan = await _mediator.Send(new GetTodayPlanQuery(targetUserId), ct);

        if (plan == null)
            return NotFound(new ProblemDetails
            {
                Title = "Plan Not Found",
                Detail = "No plan has been generated for today yet. Call POST /api/v1/plans/generate to initialize.",
                Status = StatusCodes.Status404NotFound
            });

        return Ok(plan);
    }

    /// <summary>
    /// Generates or refreshes today's plan with 5-factor deterministic ranking.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(DailyPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(DailyPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GeneratePlan(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        [FromQuery] string locale = "en",
        [FromBody] GeneratePlanRequest? request = null,
        CancellationToken ct = default)
    {
        var targetUserId = userId ?? Guid.Parse("11111111-1111-1111-1111-111111111111");
        var forceRegenerate = request?.ForceRegenerate ?? false;

        var plan = await _mediator.Send(new GenerateDailyPlanCommand(targetUserId, locale, forceRegenerate), ct);

        return Ok(plan);
    }
}

public sealed class GeneratePlanRequest
{
    public bool ForceRegenerate { get; set; }
}
