# Feature Specification: Onboarding & Consent Governance

**Feature Branch**: `001-onboarding-and-consent`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "Adult onboarding; profile initialization; goal selection across movement, sleep routines, and nutrition; contextual constraints capture (work shift patterns, indoor/outdoor preferences, space/equipment, budget); ability baseline and accessibility needs; granular opt-in consent categories adhering to UAE PDPL (Federal Decree-Law No. 45 of 2021) and UAE Health ICT Law (Federal Law No. 2 of 2019); plain-language privacy summary and first achievable daily plan preview."

---

## Code Concepts & Architectural Commentary for Programmers

```csharp
// ==============================================================================================
// ARCHITECTURAL CONCEPT: Domain-Driven Aggregate Root for User Onboarding & Consent
// In adherence to Principle I (User Agency) & Principle IV (Privacy by Design), consent is not a
// single boolean flag. It is modeled as an immutable collection of individual ConsentGrant
// entities, versioned and scoped to specific processing purposes (e.g., Profile, HealthConnect,
// ChatAnalytics). The User aggregate enforces invariants preventing unauthorized data collection.
// ==============================================================================================

namespace Gibo.Domain.Aggregates.UserAggregate;

public sealed class User : AggregateRoot<UserId>
{
    // Primary invariant: Gibo is restricted to adults (18+) in the MVP to comply with UAE regulations.
    public bool IsAdultConfirmed { get; private set; }
    
    // User locale preference: "en", "ar", or "hi". Determines UI layout (LTR/RTL) and content localization.
    public Locale PreferredLocale { get; private set; }
    
    // User timezone (e.g., "Asia/Dubai"): Crucial for daily plan reset windows and quiet hours calculation.
    public TimeZoneInfo TimeZone { get; private set; }
    
    // Contextual preferences controlling daily action feasibility:
    public UserPreference Preferences { get; private set; }
    
    // Granular consent ledger: An append-only list of granted and revoked permissions.
    private readonly List<ConsentGrant> _consentGrants = new();
    public IReadOnlyCollection<ConsentGrant> ConsentGrants => _consentGrants.AsReadOnly();

    // Factory method enforcing strict onboarding validation rules before entity creation
    public static User CreateInitial(
        UserId id, 
        bool isAdult, 
        Locale locale, 
        string timeZoneId,
        UserPreference preferences)
    {
        if (!isAdult)
            throw new DomainValidationException("Gibo is currently restricted to adults (18+).");

        var user = new User
        {
            Id = id,
            IsAdultConfirmed = true,
            PreferredLocale = locale ?? Locale.DefaultEnglish,
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? "Asia/Dubai"),
            Preferences = preferences ?? throw new ArgumentNullException(nameof(preferences))
        };

        return user;
    }

    // Records a legally auditable consent grant with purpose limitation
    public void RecordConsent(ConsentCategory category, string purpose, string policyVersion)
    {
        // Principle IV: Explicit opt-in per category; prevents bundling disparate data permissions.
        var grant = new ConsentGrant(
            ConsentId.New(), 
            Id, 
            category, 
            purpose, 
            policyVersion, 
            DateTimeOffset.UtcNow);
            
        _consentGrants.Add(grant);
        
        // Emits a domain event for audit logging required by UAE PDPL Article 5
        AddDomainEvent(new ConsentRecordedEvent(Id, category, policyVersion, grant.GrantedAt));
    }
}
```

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Trust-Centered Welcome & Granular Consent (Priority: P1)

As a new resident in Abu Dhabi opening Gibo for the first time,
I want to select my language (English, Arabic, or Hindi), confirm age eligibility, and review a plain-language summary of data practices with granular consent toggles,
So that I understand Gibo is a wellness companion (not a medical diagnostic service) and feel fully in control of my personal data from the very first screen.

**Why this priority**: Establishes legal compliance, cultural comfort, and user trust before any personal health details are gathered. Without valid consent, no further data may be legally processed under UAE PDPL.

**Independent Test**: Can be tested independently by launching the app, choosing Arabic (verifying full RTL layout), confirming adult eligibility, reviewing the consent items (account, wearable sync, chat query processing), toggling only Account and declining Wearable, and verifying the user profile and consent ledger are stored with correct timestamps in the backend.

**Acceptance Scenarios**:

1. **Given** a new user launching the app for the first time,  
   **When** the welcome screen loads,  
   **Then** the user can toggle between English, Arabic (العربية with immediate RTL mirroring), and Hindi (हिन्दी), and see an explicit statement that "Gibo supports general wellness and is not a clinical medical service."
2. **Given** the age verification screen,  
   **When** the user indicates they are under 18 years of age,  
   **Then** the app gracefully halts registration, presents an age-eligibility explanation, and does not retain any personal health data.
3. **Given** the consent selection screen,  
   **When** the user is presented with data categories (`CoreProfile`, `WearableIntegration`, `ChatContext`, `ResearchTelemetry`),  
   **Then** the user can opt-in to `CoreProfile` while leaving `WearableIntegration` disabled, and the app proceeds to onboarding without degrading functionality or forcing wearable connectivity.

---

### User Story 2 - Goal Selection & Feasibility Context Capture (Priority: P1)

As an adult user with a busy work schedule,
I want to select a primary wellness focus (Movement, Sleep Routine, or Nutrition) and specify my daily constraints (shift pattern, available time, indoor/outdoor preferences, space/equipment),
So that Gibo understands what is realistically achievable in my day-to-day life without making assumptions.

**Why this priority**: Personalization is Gibo's core differentiator. Capturing real-life barriers and constraints during onboarding guarantees the first generated plan is immediately feasible rather than discouraging.

**Independent Test**: Can be tested by selecting "Movement", indicating "Night Shift Worker", "Indoor Preferred (due to summer heat)", and "No Special Equipment", and verifying that the generated profile correctly captures these parameters in `UserPreference`.

**Acceptance Scenarios**:

1. **Given** the goal selection screen,  
   **When** the user views the options,  
   **Then** the app offers Movement, Sleep Routine, and Nutrition as primary goals, explicitly avoiding weight-loss or caloric restriction as default objectives.
2. **Given** the contextual constraint screen,  
   **When** the user inputs their work shift (e.g., standard day, rotating shifts, night shift), available active windows (e.g., morning 10 mins, evening 15 mins), and indoor preference,  
   **Then** the system saves these constraints and permits skipping any optional question without blocking progress.
3. **Given** any optional onboarding question,  
   **When** the user chooses "Skip" or "Prefer not to say",  
   **Then** the system records sensible neutral defaults without nagging or degrading the plan quality.

---

### User Story 3 - Ability Baseline, Health Restrictions & Safety Boundaries (Priority: P2)

As a user with specific physical comfort levels or disclosed health limitations,
I want to self-rate my starting activity comfort and disclose any mobility or health restrictions,
So that Gibo automatically filters out high-intensity or contraindicated tasks without attempting to diagnose me.

**Why this priority**: Safety before ambition. Tasks that aggravate undisclosed limitations or exceed capacity lead to injury, fatigue, and app abandonment.

**Independent Test**: Can be tested by self-rating activity as "Beginner / Low Stamina" and checking "Joint Discomfort", verifying that all high-impact task templates (e.g., stair sprints, vigorous jogging) are completely suppressed from the candidate pool and a professional guidance disclaimer is shown.

**Acceptance Scenarios**:

1. **Given** the ability assessment screen,  
   **When** the user selects a beginner baseline,  
   **Then** the app sets the starting daily action cap to 1 micro-task (under 10 minutes).
2. **Given** the health boundary screen,  
   **When** the user discloses a physical restriction (e.g., lower back pain or mobility restriction),  
   **Then** the system suppresses high-impact templates, displays a supportive advisory recommending consultation with a physician, and flags the profile for gentle indoor alternatives.
3. **Given** the completion of baseline questions,  
   **When** the first plan preview screen appears,  
   **Then** the user sees exactly one small, tailored task card with a clear verb, duration (e.g., 5-7 minutes), evidence mode (manual check-off), and an expandable "Why this fits you" explanation, with options to "Accept", "Swap", or "Reduce".

---

## Edge Cases

- **User loses network connectivity during onboarding**: The mobile app caches the current onboarding step in local secure storage. When connectivity resumes, the user resumes from the exact screen without duplicate account creation.
- **User revokes consent immediately after onboarding**: The settings screen allows one-tap revocation of individual consent categories. If `CoreProfile` is revoked, an account deletion and data purge sequence is triggered immediately.
- **RTL layout disruption on mixed language input**: When Arabic is selected, right-to-left layout rules apply to all text, navigation headers, and icons, while Latin numerals or technical terms render with bidirectional isolation (`dir="auto"`) to prevent corrupted reading order.
- **Rapid tapping on submit**: Double-submission is prevented on the client via disabled buttons and on the backend via unique user idempotency tokens.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST verify that the user confirms they are at least 18 years of age prior to creating an account or collecting personal data.
- **FR-002**: The system MUST provide full UI and content localization in English, Modern Standard Arabic (with native RTL layout mirroring), and Hindi.
- **FR-003**: The system MUST require explicit, unbundled opt-in consent for distinct data processing categories (`AccountProfile`, `HealthConnectSync`, `AssistantChatContext`, `AnonymizedResearch`). Pre-checked checkboxes are strictly prohibited.
- **FR-004**: The system MUST persist every consent event in a dedicated `ConsentGrant` ledger containing `ConsentId`, `UserId`, `DataCategory`, `Purpose`, `GrantedAt` (UTC), `RevokedAt` (nullable), and `PolicyVersion`.
- **FR-005**: The system MUST permit users to select one primary health domain from Movement, Sleep Routine, or Nutrition, with an option to select secondary interests. Weight loss MUST NOT be presented as a default goal.
- **FR-006**: The system MUST capture optional user context: work schedule (day/shift/flexible), available time windows, indoor/outdoor preference (critical for Gulf summer heat), and access constraints (no gym/no equipment).
- **FR-007**: The system MUST capture self-reported physical comfort and accessibility needs (mobility, vision, dexterity, quiet hours) and automatically map them to task exclusion tags.
- **FR-008**: The system MUST allow any non-essential context question to be skipped with an explicit "Skip" or "Prefer not to say" button.
- **FR-009**: The system MUST generate a preview of the user's very first daily plan consisting of exactly one achievable starter action matching their context before finalizing onboarding.
- **FR-010**: The user MUST be able to edit, swap, reduce, or accept the first plan preview directly on the final onboarding screen.
- **FR-011**: The system MUST transmit all onboarding data over TLS 1.3 with an `Idempotency-Key` header and store sensitive preferences encrypted at rest.

### Key Entities

- **User**: Represents the primary individual account. Attributes: `UserId` (UUID), `IsAdultConfirmed` (bool), `PreferredLocale` (string), `TimeZone` (string), `CreatedAt` (UTC timestamp), `Status` (Active, Suspended, DeletionRequested).
- **ConsentGrant**: Represents a legally binding, granular consent record. Attributes: `ConsentId` (UUID), `UserId` (FK), `DataCategory` (Enum), `Purpose` (string), `PolicyVersion` (string), `GrantedAt` (UTC), `RevokedAt` (UTC, nullable).
- **UserPreference**: Represents lifestyle context and constraints. Attributes: `PreferenceId` (UUID), `UserId` (FK), `PrimaryGoalDomain` (Enum: Movement, SleepRoutine, Nutrition), `WorkPattern` (Enum: StandardDay, ShiftWork, Flexible), `IndoorPreferred` (bool), `MaxDailyActions` (int: 1, 2, or 3, default 1), `QuietHoursStart` (TimeSpan), `QuietHoursEnd` (TimeSpan), `AccessibilityTags` (string array).
- **Goal**: Represents the user's selected habit objective. Attributes: `GoalId` (UUID), `UserId` (FK), `Domain` (Enum), `DesiredOutcome` (string), `StartDate` (Date), `Status` (Active, Paused, Completed).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 90% of new users complete the progressive onboarding flow in under 3 minutes.
- **SC-002**: 100% of collected user profiles have a recorded, timestamped `ConsentGrant` row for every active data processing category before any plan generation occurs.
- **SC-003**: 0% of onboarding paths require a wearable device connection to proceed; manual check-in parity is 100% accessible to all users.
- **SC-004**: Less than 3% drop-off on contextual questions, validated by the presence of easy "Skip" options.
- **SC-005**: 100% of Arabic localization tests pass visual and RTL screen-reader accessibility audits with zero layout overflow or truncated text.

---

## Assumptions

- Target users are adults resident in or familiar with the UAE/Abu Dhabi regional lifestyle (accounting for high daytime temperatures and indoor activity needs).
- No clinical EHR or Malaffi credentials will be requested during onboarding; all clinical health claims are out of scope.
- The user's mobile device supports standard Android secure key store and modern web view / native rendering.
- Push notification permissions are requested contextually only after onboarding completion, respecting the user's specified quiet hours.
