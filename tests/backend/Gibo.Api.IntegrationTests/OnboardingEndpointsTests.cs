using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gibo.Api.Controllers.v1;
using Gibo.Application.Features.Onboarding.Commands.RegisterOnboardingUser;
using Gibo.Application.Features.Onboarding.Services;
using Gibo.Domain.Aggregates.UserAggregate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Gibo.Api.IntegrationTests;

/// <summary>
/// End-to-End API Integration Tests for Onboarding, Consent Ledger & First Plan Preview
/// </summary>
public static class OnboardingEndpointsTests
{
    public static async Task Main()
    {
        Console.WriteLine("Running Gibo.Api.IntegrationTests...");

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "backend", "Gibo.Api")))
        {
            dir = dir.Parent;
        }

        var contentRoot = dir != null 
            ? Path.Combine(dir.FullName, "src", "backend", "Gibo.Api") 
            : Path.GetFullPath("../../../../../../src/backend/Gibo.Api");

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseContentRoot(contentRoot);
                builder.UseEnvironment("Development");
            });
        using var client = factory.CreateClient();

        await Test_PostOnboarding_ValidAdult_Returns201Created(client);
        await Test_PostOnboarding_Underage_Returns400BadRequest(client);
        await Test_PostOnboarding_IdempotencyKey_DeduplicatesSubmission(client);
        await Test_PostFirstPlanPreview_MovementIndoor_ReturnsPlan(client);

        await DailyPlanEndpointsTests.RunAll(client);

        Console.WriteLine("\nALL Gibo.Api.IntegrationTests PASSED SUCCESSFULLY (9/9)!");
    }

    private static async Task Test_PostOnboarding_ValidAdult_Returns201Created(HttpClient client)
    {
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = new RegisterOnboardingUserRequest(
            IsAdultConfirmed: true,
            PreferredLocale: "ar",
            TimezoneId: "Asia/Dubai",
            PrimaryGoalDomain: GoalDomain.Movement,
            WorkPattern: WorkSchedulePattern.StandardDaytime,
            IndoorPreferred: true,
            MaxDailyActions: 1,
            QuietHoursStart: "22:00:00",
            QuietHoursEnd: "07:00:00",
            BaselineFitnessLevel: BaselineFitnessLevel.Starter,
            AccessibilityTags: new[] { "LowImpact" },
            Consents: new List<ConsentInputItem>
            {
                new(ConsentCategory.CoreProfile, true, "2026.1"),
                new(ConsentCategory.HealthConnectSync, false, "2026.1")
            });

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/onboarding")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey);

        var response = await client.SendAsync(message);
        Assert(response.StatusCode == HttpStatusCode.Created, $"Expected 201 Created but got {response.StatusCode}");

        var body = await response.Content.ReadFromJsonAsync<OnboardingResultDto>();
        Assert(body != null, "Response body should not be null");
        Assert(body!.UserId != Guid.Empty, "UserId should be non-empty");
        Assert(body.Status == "Active", "Status must be Active");
        Assert(body.FirstPlan != null, "FirstPlan preview must be returned");
        Assert(body.FirstPlan.EstimatedMinutes <= 10, "First plan must be starter size (< 10 mins)");

        Console.WriteLine("✓ Test_PostOnboarding_ValidAdult_Returns201Created passed.");
    }

    private static async Task Test_PostOnboarding_Underage_Returns400BadRequest(HttpClient client)
    {
        var request = new RegisterOnboardingUserRequest(
            IsAdultConfirmed: false,
            PreferredLocale: "en",
            TimezoneId: "Asia/Dubai",
            PrimaryGoalDomain: GoalDomain.Movement,
            WorkPattern: WorkSchedulePattern.StandardDaytime,
            IndoorPreferred: true,
            MaxDailyActions: 1,
            QuietHoursStart: "22:00:00",
            QuietHoursEnd: "07:00:00",
            BaselineFitnessLevel: BaselineFitnessLevel.Starter,
            AccessibilityTags: Array.Empty<string>(),
            Consents: new List<ConsentInputItem>
            {
                new(ConsentCategory.CoreProfile, true, "2026.1")
            });

        var response = await client.PostAsJsonAsync("/api/v1/onboarding", request);
        Assert(response.StatusCode == HttpStatusCode.BadRequest, $"Expected 400 BadRequest for underage user, got {response.StatusCode}");

        Console.WriteLine("✓ Test_PostOnboarding_Underage_Returns400BadRequest passed.");
    }

    private static async Task Test_PostOnboarding_IdempotencyKey_DeduplicatesSubmission(HttpClient client)
    {
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = new RegisterOnboardingUserRequest(
            IsAdultConfirmed: true,
            PreferredLocale: "en",
            TimezoneId: "Asia/Dubai",
            PrimaryGoalDomain: GoalDomain.SleepRoutine,
            WorkPattern: WorkSchedulePattern.ShiftWorker,
            IndoorPreferred: true,
            MaxDailyActions: 2,
            QuietHoursStart: "23:00:00",
            QuietHoursEnd: "06:00:00",
            BaselineFitnessLevel: BaselineFitnessLevel.Starter,
            AccessibilityTags: Array.Empty<string>(),
            Consents: new List<ConsentInputItem>
            {
                new(ConsentCategory.CoreProfile, true, "2026.1")
            });

        // First submission
        using var message1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/onboarding")
        {
            Content = JsonContent.Create(request)
        };
        message1.Headers.Add("Idempotency-Key", idempotencyKey);
        var response1 = await client.SendAsync(message1);
        Assert(response1.StatusCode == HttpStatusCode.Created, "First request must return 201 Created");
        var body1 = await response1.Content.ReadFromJsonAsync<OnboardingResultDto>();

        // Duplicate submission with same Idempotency-Key
        using var message2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/onboarding")
        {
            Content = JsonContent.Create(request)
        };
        message2.Headers.Add("Idempotency-Key", idempotencyKey);
        var response2 = await client.SendAsync(message2);
        Assert(response2.StatusCode == HttpStatusCode.Created, "Second request must return 201 Created (idempotent)");
        var body2 = await response2.Content.ReadFromJsonAsync<OnboardingResultDto>();

        Assert(body1!.UserId == body2!.UserId, "Deduplication must return the identical UserId");
        Console.WriteLine("✓ Test_PostOnboarding_IdempotencyKey_DeduplicatesSubmission passed.");
    }

    private static async Task Test_PostFirstPlanPreview_MovementIndoor_ReturnsPlan(HttpClient client)
    {
        var request = new FirstPlanPreviewRequestDto(
            GoalDomain: GoalDomain.Movement,
            BaselineFitnessLevel: BaselineFitnessLevel.Starter,
            IndoorPreferred: true,
            Locale: "ar");

        var response = await client.PostAsJsonAsync("/api/v1/onboarding/first-plan-preview", request);
        Assert(response.StatusCode == HttpStatusCode.OK, $"Expected 200 OK, got {response.StatusCode}");

        var plan = await response.Content.ReadFromJsonAsync<PlanCandidateDto>();
        Assert(plan != null, "Plan preview must not be null");
        Assert(plan!.Verb == "مشي", "Verb in Arabic must be 'مشي'");
        Assert(plan.EstimatedMinutes == 7, "Estimated duration must be 7 minutes");

        Console.WriteLine("✓ Test_PostFirstPlanPreview_MovementIndoor_ReturnsPlan passed.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception("Assertion Failed: " + message);
    }
}
