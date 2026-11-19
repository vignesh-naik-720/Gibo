using System;
using System.Linq;
using Gibo.Domain.Aggregates.UserAggregate;
using Gibo.Domain.Events;
using Gibo.Domain.Exceptions;

namespace Gibo.Domain.UnitTests;

/// <summary>
/// Unit Tests for User Aggregate Root and Invariant Validation
/// Validates age eligibility, consent ledger behavior, and preference clamping.
/// </summary>
public static class UserAggregateTests
{
    public static void Main()
    {
        Console.WriteLine("Running Gibo.Domain.UnitTests...");

        Test_UnderageUser_ThrowsDomainValidationException();
        Test_AdultUser_CreationSucceeds();
        Test_ConsentRecording_AppendsToLedgerAndRaisesEvent();
        Test_DuplicateConsent_RevokesPreviousActiveGrant();
        Test_RevokeConsent_SetsRevokedTimestamp();
        Test_UserPreference_ClampsActionCapBetweenOneAndThree();

        TaskRankingEngineTests.RunAll();
        TaskCompletionTests.RunAll();

        Console.WriteLine("\nALL Gibo.Domain.UnitTests PASSED SUCCESSFULLY!");
    }

    private static void Test_UnderageUser_ThrowsDomainValidationException()
    {
        try
        {
            var pref = CreateSamplePreference();
            User.CreateInitial(UserId.New(), isAdult: false, Locale.DefaultEnglish, "Asia/Dubai", pref);
            throw new Exception("FAIL: Expected DomainValidationException was not thrown for underage user.");
        }
        catch (DomainValidationException ex) when (ex.ErrorCode == "ELIGIBILITY_UNDERAGE")
        {
            Console.WriteLine("✓ Test_UnderageUser_ThrowsDomainValidationException passed.");
        }
    }

    private static void Test_AdultUser_CreationSucceeds()
    {
        var pref = CreateSamplePreference();
        var user = User.CreateInitial(UserId.New(), isAdult: true, Locale.Arabic, "Asia/Dubai", pref);

        Assert(user.IsAdultConfirmed, "User must have adult confirmed.");
        Assert(user.PreferredLocale.Code == "ar", "Locale must match Arabic.");
        Assert(user.Status == UserAccountStatus.Active, "User status must be Active.");
        Console.WriteLine("✓ Test_AdultUser_CreationSucceeds passed.");
    }

    private static void Test_ConsentRecording_AppendsToLedgerAndRaisesEvent()
    {
        var pref = CreateSamplePreference();
        var user = User.CreateInitial(UserId.New(), isAdult: true, Locale.DefaultEnglish, "Asia/Dubai", pref);

        user.RecordConsent(ConsentCategory.CoreProfile, "Core operations", "2026.1");

        Assert(user.ConsentGrants.Count == 1, "Consent grant count must be 1.");
        var grant = user.ConsentGrants.First();
        Assert(grant.Category == ConsentCategory.CoreProfile, "Category must match.");
        Assert(grant.IsActive, "Grant must be active.");
        Assert(user.DomainEvents.OfType<ConsentRecordedEvent>().Any(), "Must emit ConsentRecordedEvent.");
        Console.WriteLine("✓ Test_ConsentRecording_AppendsToLedgerAndRaisesEvent passed.");
    }

    private static void Test_DuplicateConsent_RevokesPreviousActiveGrant()
    {
        var pref = CreateSamplePreference();
        var user = User.CreateInitial(UserId.New(), isAdult: true, Locale.DefaultEnglish, "Asia/Dubai", pref);

        user.RecordConsent(ConsentCategory.HealthConnectSync, "Step sync v1", "2026.1");
        user.RecordConsent(ConsentCategory.HealthConnectSync, "Step sync v2", "2026.2");

        Assert(user.ConsentGrants.Count == 2, "Must contain 2 grant records.");
        var first = user.ConsentGrants.First();
        var second = user.ConsentGrants.Last();

        Assert(!first.IsActive, "First grant must be revoked.");
        Assert(first.RevokedAt != null, "First grant must have RevokedAt timestamp.");
        Assert(second.IsActive, "Second grant must be active.");
        Console.WriteLine("✓ Test_DuplicateConsent_RevokesPreviousActiveGrant passed.");
    }

    private static void Test_RevokeConsent_SetsRevokedTimestamp()
    {
        var pref = CreateSamplePreference();
        var user = User.CreateInitial(UserId.New(), isAdult: true, Locale.DefaultEnglish, "Asia/Dubai", pref);

        user.RecordConsent(ConsentCategory.AssistantChatContext, "Chat context", "2026.1");
        user.RevokeConsent(ConsentCategory.AssistantChatContext);

        var grant = user.ConsentGrants.First(c => c.Category == ConsentCategory.AssistantChatContext);
        Assert(!grant.IsActive, "Grant must be revoked.");
        Assert(grant.RevokedAt != null, "RevokedAt timestamp must be recorded.");
        Console.WriteLine("✓ Test_RevokeConsent_SetsRevokedTimestamp passed.");
    }

    private static void Test_UserPreference_ClampsActionCapBetweenOneAndThree()
    {
        var prefLow = new UserPreference(
            PreferenceId.New(), UserId.New(), GoalDomain.Movement, WorkSchedulePattern.StandardDaytime,
            indoorPreferred: true, maxDailyActions: 0, quietHoursStart: TimeSpan.FromHours(22),
            quietHoursEnd: TimeSpan.FromHours(7), BaselineFitnessLevel.Starter, null);

        var prefHigh = new UserPreference(
            PreferenceId.New(), UserId.New(), GoalDomain.Movement, WorkSchedulePattern.StandardDaytime,
            indoorPreferred: true, maxDailyActions: 10, quietHoursStart: TimeSpan.FromHours(22),
            quietHoursEnd: TimeSpan.FromHours(7), BaselineFitnessLevel.Starter, null);

        Assert(prefLow.MaxDailyActions == 1, "Cap of 0 must be clamped to 1.");
        Assert(prefHigh.MaxDailyActions == 3, "Cap of 10 must be clamped to 3.");
        Console.WriteLine("✓ Test_UserPreference_ClampsActionCapBetweenOneAndThree passed.");
    }

    private static UserPreference CreateSamplePreference() =>
        new(PreferenceId.New(), UserId.New(), GoalDomain.Movement, WorkSchedulePattern.StandardDaytime,
            indoorPreferred: true, maxDailyActions: 1, quietHoursStart: TimeSpan.FromHours(22),
            quietHoursEnd: TimeSpan.FromHours(7), BaselineFitnessLevel.Starter, new[] { "LowImpact" });

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception("Assertion Failed: " + message);
    }
}
