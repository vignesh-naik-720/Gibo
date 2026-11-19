// ==============================================================================================
// Integration Tests: Daily Plan Generation, Task Completion, Swapping, and Offline Sync Endpoints
// Validates end-to-end HTTP pipeline, EF Core persistence, and Principle V (10 XP parity)
// ==============================================================================================

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gibo.Api.Controllers.v1;
using Gibo.Application.Features.Planning.Commands.CompleteTask;
using Gibo.Application.Features.Planning.Commands.GenerateDailyPlan;
using Gibo.Application.Features.Planning.Commands.SyncOfflineEvents;
using Gibo.Domain.Aggregates.PlanAggregate;

namespace Gibo.Api.IntegrationTests;

public static class DailyPlanEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() 
    { 
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task RunAll(HttpClient client)
    {
        Console.WriteLine("\nRunning DailyPlanEndpointsTests...");

        var plan = await Test_GenerateDailyPlan_Returns200WithActions(client);
        if (plan != null && plan.Tasks.Count > 0)
        {
            var firstTask = plan.Tasks[0];
            await Test_CompleteTask_Manual_AwardsExactly10Xp(client, firstTask.TaskInstanceId);
            await Test_CompleteTask_DuplicateClientEventId_ReturnsDuplicateWithZeroXp(client, firstTask.TaskInstanceId);

            if (plan.Tasks.Count > 1)
            {
                var secondTask = plan.Tasks[1];
                await Test_AdjustTaskTarget_ReducesTargetSuccessfully(client, secondTask.TaskInstanceId);
                await Test_SwapTask_ReplacesActionCard(client, secondTask.TaskInstanceId);
            }

            await Test_SyncOffline_BatchesCompletionsWithIdempotency(client, plan.Tasks[0].TaskInstanceId);
        }

        Console.WriteLine("ALL DailyPlanEndpointsTests PASSED SUCCESSFULLY (5/5)!");
    }

    private static async Task<DailyPlanDto?> Test_GenerateDailyPlan_Returns200WithActions(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/plans/generate?locale=en", new GeneratePlanRequest { ForceRegenerate = false });
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"FAIL: /api/v1/plans/generate returned {response.StatusCode}: {errorBody}");
        }

        var plan = await response.Content.ReadFromJsonAsync<DailyPlanDto>(JsonOptions);
        if (plan == null || plan.Tasks.Count == 0)
        {
            throw new Exception("FAIL: Generated plan is null or contains 0 tasks.");
        }

        if (plan.Tasks.Count > 3)
        {
            throw new Exception($"FAIL: Generated plan exceeded maximum cap of 3 actions (Got {plan.Tasks.Count}).");
        }

        Console.WriteLine($"✓ Test_GenerateDailyPlan_Returns200WithActions passed ({plan.Tasks.Count} actions).");
        return plan;
    }

    private static async Task Test_CompleteTask_Manual_AwardsExactly10Xp(HttpClient client, Guid taskId)
    {
        var clientEventId = Guid.NewGuid().ToString();
        var request = new CompleteTaskRequest
        {
            EvidenceSource = CompletionEvidenceSource.Manual,
            ClientEventId = clientEventId,
            RecordedAtUtc = DateTimeOffset.UtcNow
        };

        var response = await client.PostAsJsonAsync($"/api/v1/tasks/{taskId}/complete", request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"FAIL: CompleteTask returned {response.StatusCode}: {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<TaskCompletionResultDto>(JsonOptions);
        if (result == null || result.XpAwarded != 10)
        {
            throw new Exception($"FAIL: Expected exactly 10 XP award, got {result?.XpAwarded}.");
        }

        if (result.IsDuplicate)
        {
            throw new Exception("FAIL: Fresh completion was marked as duplicate.");
        }

        Console.WriteLine("✓ Test_CompleteTask_Manual_AwardsExactly10Xp passed.");
    }

    private static async Task Test_CompleteTask_DuplicateClientEventId_ReturnsDuplicateWithZeroXp(HttpClient client, Guid taskId)
    {
        var clientEventId = Guid.NewGuid().ToString();
        var request = new CompleteTaskRequest
        {
            EvidenceSource = CompletionEvidenceSource.Manual,
            ClientEventId = clientEventId,
            RecordedAtUtc = DateTimeOffset.UtcNow
        };

        // First call
        await client.PostAsJsonAsync($"/api/v1/tasks/{taskId}/complete", request);

        // Second call with same clientEventId
        var secondResponse = await client.PostAsJsonAsync($"/api/v1/tasks/{taskId}/complete", request);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<TaskCompletionResultDto>(JsonOptions);

        if (secondResult == null || !secondResult.IsDuplicate || secondResult.XpAwarded != 0)
        {
            throw new Exception($"FAIL: Expected duplicate with 0 XP, got duplicate={secondResult?.IsDuplicate}, xp={secondResult?.XpAwarded}");
        }

        Console.WriteLine("✓ Test_CompleteTask_DuplicateClientEventId_ReturnsDuplicateWithZeroXp passed.");
    }

    private static async Task Test_AdjustTaskTarget_ReducesTargetSuccessfully(HttpClient client, Guid taskId)
    {
        var request = new AdjustTargetRequest { NewTarget = 5 };
        var response = await client.PutAsJsonAsync($"/api/v1/tasks/{taskId}/target", request);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"FAIL: AdjustTaskTarget returned {response.StatusCode}: {err}");
        }

        var updated = await response.Content.ReadFromJsonAsync<TaskInstanceDto>(JsonOptions);
        if (updated == null || updated.TargetValue != 5)
        {
            throw new Exception($"FAIL: Expected target 5, got {updated?.TargetValue}");
        }

        Console.WriteLine("✓ Test_AdjustTaskTarget_ReducesTargetSuccessfully passed.");
    }

    private static async Task Test_SwapTask_ReplacesActionCard(HttpClient client, Guid taskId)
    {
        var replacementTemplateId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var request = new SwapTaskRequest
        {
            ReplacementTemplateId = replacementTemplateId,
            Locale = "en"
        };

        var response = await client.PostAsJsonAsync($"/api/v1/tasks/{taskId}/swap", request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"FAIL: SwapTask returned {response.StatusCode}: {err}");
        }

        var swapped = await response.Content.ReadFromJsonAsync<TaskInstanceDto>(JsonOptions);
        if (swapped == null || swapped.TaskTemplateId != replacementTemplateId)
        {
            throw new Exception("FAIL: Swapped task does not match requested template ID.");
        }

        Console.WriteLine("✓ Test_SwapTask_ReplacesActionCard passed.");
    }

    private static async Task Test_SyncOffline_BatchesCompletionsWithIdempotency(HttpClient client, Guid taskId)
    {
        var duplicateId = Guid.NewGuid().ToString();
        var uniqueId = Guid.NewGuid().ToString();

        var request = new SyncOfflineRequest
        {
            Events = new List<OfflineEventRequestItem>
            {
                new()
                {
                    TaskInstanceId = taskId,
                    EvidenceSource = CompletionEvidenceSource.HealthConnect,
                    ClientEventId = duplicateId,
                    RecordedAtUtc = DateTimeOffset.UtcNow
                },
                new()
                {
                    TaskInstanceId = taskId,
                    EvidenceSource = CompletionEvidenceSource.HealthConnect,
                    ClientEventId = uniqueId,
                    RecordedAtUtc = DateTimeOffset.UtcNow
                }
            }
        };

        // First batch
        var firstResponse = await client.PostAsJsonAsync("/api/v1/tasks/sync-offline", request);
        if (!firstResponse.IsSuccessStatusCode)
        {
            var err = await firstResponse.Content.ReadAsStringAsync();
            throw new Exception($"FAIL: SyncOffline returned {firstResponse.StatusCode}: {err}");
        }

        // Re-send the same batch to test idempotent replay
        var secondResponse = await client.PostAsJsonAsync("/api/v1/tasks/sync-offline", request);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<BatchSyncResultDto>(JsonOptions);

        if (secondResult == null || secondResult.DuplicateCount != 2 || secondResult.TotalXpAwarded != 0)
        {
            throw new Exception($"FAIL: Expected 2 duplicates with 0 XP awarded, got duplicates={secondResult?.DuplicateCount}, xp={secondResult?.TotalXpAwarded}");
        }

        Console.WriteLine("✓ Test_SyncOffline_BatchesCompletionsWithIdempotency passed.");
    }
}
