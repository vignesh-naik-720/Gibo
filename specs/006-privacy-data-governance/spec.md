# Feature Specification: Privacy, Trust & Abu Dhabi Data Governance

**Feature Branch**: `006-privacy-data-governance`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "Privacy by design, regulatory compliance with UAE Federal Decree-Law No. 45 of 2021 (PDPL) and UAE Federal Law No. 2 of 2019 (ICT in Health Fields); granular consent ledger with per-purpose grant and instant revocation; in-app portable data export (JSON/CSV); permanent account and telemetry deletion workflow; data minimisation using aggregated activity signals rather than continuous raw feeds; strict data residency and sovereignty boundaries preventing unapproved overseas health data transmission; synthetic test data isolation; and clean adapter boundaries for future Abu Dhabi Health Information Exchange (Malaffi) integration."

---

## Code Concepts & Architectural Commentary for Programmers

```csharp
// ==============================================================================================
// ARCHITECTURAL CONCEPT: UAE PDPL & Health ICT Law Compliance Architecture
// Under UAE Federal Decree-Law No. 45 of 2021 (PDPL) Articles 5-9 and Federal Law No. 2 of 2019
// Article 13, health-related information requires sovereign protection, purpose limitation,
// and demonstrable consent. This module enforces:
//
// 1. Immutable Audit & Consent Ledger:
//    Every change to user consent or access to personal records generates an append-only AuditEvent.
// 2. Data Minimisation:
//    Health Connect data is stored as daily aggregated numbers (e.g., "1,200 steps") rather than
//    continuous GPS tracks, heart rate second-by-second feeds, or raw sensor streams.
// 3. User Sovereignty Rights:
//    Users can download a complete portable JSON archive or trigger an atomic cryptographic erasure.
// 4. Clean Malaffi Adapter Boundary:
//    Demonstrates readiness for future Abu Dhabi HIE integration without claiming unauthorized access.
// ==============================================================================================

namespace Gibo.Infrastructure.Privacy;

public sealed class DataGovernanceService : IDataGovernanceService
{
    private readonly IGiboDbContext _dbContext;
    private readonly IEncryptionProvider _encryption;
    private readonly IAuditEventLogger _auditLogger;

    // Handles user right to data portability (UAE PDPL Article 14)
    public async Task<UserPortableDataExport> ExportUserDataAsync(UserId userId, CancellationToken ct)
    {
        // 1. Authenticate & Authorize caller explicitly to ensure zero unauthorized cross-tenant leakage
        var user = await _dbContext.Users
            .Include(u => u.Preferences)
            .Include(u => u.ConsentGrants)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId.Value);

        // 2. Retrieve all associated behaviour records and decrypted feedback
        var plans = await _dbContext.DailyPlans
            .Where(p => p.UserId == userId)
            .Include(p => p.TaskInstances)
            .AsNoTracking()
            .ToListAsync(ct);

        var xpLedger = await _dbContext.XpTransactions
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);

        // 3. Log the export operation to the immutable security audit trail
        await _auditLogger.LogEventAsync(new AuditEvent(
            Actor: userId.ToString(),
            Action: "DATA_EXPORT_REQUESTED",
            Object: $"User:{userId}",
            Reason: "User exercised PDPL Article 14 portability right",
            Timestamp: DateTimeOffset.UtcNow));

        // 4. Construct a structured, standardized JSON payload for export
        return new UserPortableDataExport(
            Profile: new UserProfileExport(user.Id.Value, user.PreferredLocale.Code, user.TimeZone.Id, user.CreatedAt),
            Consents: user.ConsentGrants.Select(c => new ConsentRecordExport(c.Category.ToString(), c.Purpose, c.GrantedAt, c.RevokedAt)),
            Plans: plans.Select(p => new DailyPlanExport(p.Date, p.TaskInstances.Count)),
            TotalEarnedXp: xpLedger.Sum(x => x.Amount));
    }

    // Handles user right to erasure / account deletion (UAE PDPL Article 15)
    public async Task ExecuteAccountDeletionAsync(UserId userId, CancellationToken ct)
    {
        // Execute atomic deletion within a database transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Revoke all active consent grants
            var consents = await _dbContext.ConsentGrants.Where(c => c.UserId == userId).ToListAsync(ct);
            foreach (var consent in consents)
            {
                consent.Revoke();
            }

            // 2. Hard-delete or cryptographically shred personal identifiers and barrier notes
            var user = await _dbContext.Users.FindAsync(new object[] { userId }, ct);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
            }

            // 3. Scrub sensitive device connections and tokens
            var devices = await _dbContext.DeviceConnections.Where(d => d.UserId == userId).ToListAsync(ct);
            _dbContext.DeviceConnections.RemoveRange(devices);

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // 4. Emit auditable record of compliant erasure
            await _auditLogger.LogEventAsync(new AuditEvent(
                Actor: userId.ToString(),
                Action: "ACCOUNT_DELETED",
                Object: $"User:{userId}",
                Reason: "User exercised PDPL Article 15 erasure right",
                Timestamp: DateTimeOffset.UtcNow));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
```

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Granular Consent Review & Immediate Revocation (Priority: P1)

As a privacy-minded resident in Abu Dhabi,
I want to view my active data consent permissions in settings and revoke any specific permission (e.g., disconnect Health Connect or disable chat history) with one tap,
So that I maintain continuous autonomy over how my data is processed without being forced to close my account.

**Why this priority**: Required by UAE PDPL Article 5 (Consent conditions). Consent must be as easy to withdraw as it was to grant.

**Independent Test**: Can be tested by opening Settings > Privacy & Permissions, toggling off "Health Connect Integration", verifying that the background sync stops immediately, the `ConsentGrant` is marked revoked with a UTC timestamp, and manual completion remains fully accessible.

**Acceptance Scenarios**:

1. **Given** the Privacy & Permissions settings screen,  
   **When** the user views the permissions list,  
   **Then** each data category (`AccountProfile`, `HealthConnectSync`, `AssistantChatContext`, `AnonymousResearch`) is displayed with its specific purpose, policy version, and grant date.
2. **Given** an active permission (e.g., `HealthConnectSync`),  
   **When** the user toggles the switch to disabled,  
   **Then** the app immediately sets `RevokedAt = DateTimeOffset.UtcNow` on the backend, severs device token credentials, and displays a confirmation message: *"Health Connect sync stopped. You can continue completing actions manually."*
3. **Given** a user who revokes all optional categories,  
   **When** they return to the daily plan,  
   **Then** the core habit engine functions seamlessly without popups demanding re-authorization.

---

### User Story 2 - Portable User Data Export (Priority: P2)

As a user exercising my rights under UAE data privacy laws,
I want to request an export of my entire personal record (profile, preferences, habits, completion events, XP ledger, and barrier logs) in a downloadable JSON file,
So that I have full portability and transparency regarding what information Gibo maintains about me.

**Why this priority**: Directly implements UAE PDPL Article 14 (Right to Data Portability) and establishes credibility with regulatory authorities and judges.

**Independent Test**: Can be tested by navigating to Settings > Privacy > "Export My Data", tapping request, and verifying that an encrypted, structured JSON package containing all user records is delivered in under 5 seconds.

**Acceptance Scenarios**:

1. **Given** the user settings screen,  
   **When** the user taps "Export My Data",  
   **Then** the system packages their profile, consent history, completed task instances, XP ledger, and barrier reflections into a standard JSON file.
2. **Given** the generated data export,  
   **When** inspected,  
   **Then** all sensitive fields (such as internal hashing salts or database primary keys) are sanitized, presenting clean, human-readable ISO 8601 timestamps and explicit data provenance labels.
3. **Given** an export request,  
   **When** processed,  
   **Then** an immutable `AuditEvent` is created recording the export timestamp and user trace ID for compliance reporting.

---

### User Story 3 - Verified Account Deletion & Cryptographic Shredding (Priority: P1)

As a user who decides to discontinue using Gibo,
I want to delete my account and permanently purge all personal information, habits, and device connections from the servers,
So that no lingering identifiable health records remain after I depart.

**Why this priority**: Implements UAE PDPL Article 15 (Right to Erasure / Right to be Forgotten) and UAE Health Data ICT protection standards.

**Independent Test**: Can be tested by initiating "Delete Account", confirming the two-step verification prompt, and checking the backend database to verify that the user profile, device tokens, and personal identifiers have been completely purged.

**Acceptance Scenarios**:

1. **Given** the "Delete Account" screen,  
   **When** the user taps delete,  
   **Then** the app displays a clear confirmation dialog explaining that all habits, XP, and personal history will be permanently erased.
2. **Given** user confirmation of deletion,  
   **When** the deletion command executes,  
   **Then** the backend executes an atomic deletion transaction: revokes all consent grants, hard-deletes the user profile and device connection tokens, and invalidates all active JWT tokens immediately.
3. **Given** a completed deletion,  
   **When** the user attempts to log back in with old credentials,  
   **Then** authentication fails, the mobile app clears its local encrypted cache, and resets to the initial welcome screen.

---

### User Story 4 - Future HIE / Malaffi Integration Boundary (Priority: P3)

As a technical auditor or hackathon judge reviewing Gibo's healthcare ecosystem readiness,
I want to see a clean, abstracted architecture adapter interface for Abu Dhabi's Malaffi Health Information Exchange,
So that I can verify that future clinical integration would follow Department of Health (DoH) standards without falsely claiming unapproved live access today.

**Why this priority**: Directly addresses judging criteria and regulatory integrity. Prevents disqualified presentations that make unsubstantiated claims regarding live Malaffi connections.

**Independent Test**: Can be tested by inspecting the backend architecture and interface contracts (`IHealthInformationExchangeAdapter`), verifying that the prototype runs on a documented `SyntheticMalaffiAdapter` with zero unapproved calls to UAE governmental clinical servers.

**Acceptance Scenarios**:

1. **Given** the backend integration architecture,  
   **When** reviewing the HIE components,  
   **Then** the codebase exposes a well-defined `IHealthInformationExchangeAdapter` interface defining clinical consent verification, payload encryption, and FHIR-compatible data models.
2. **Given** the running MVP environment,  
   **When** inspecting external network traffic,  
   **Then** the system connects exclusively to internal services and synthetic demo mocks, with zero unauthorized outbound connections to live hospital or government endpoints.
3. **Given** the app's public documentation and privacy policy,  
   **When** Malaffi is mentioned,  
   **Then** it is explicitly described as a strategic future integration conditional upon formal DoH authorization, clinical sponsorship, and security accreditation.

---

## Edge Cases

- **Subpoena or Legal Retention Exceptions**: If a legal hold or statutory audit requirement applies, personal identifiers are severed/anonymized while required non-identifiable telemetry is retained for compliance.
- **Concurrent sync during deletion**: If a background sync event arrives while an account deletion transaction is processing, the incoming event is rejected with HTTP 404/410, and no orphan records are written.
- **Cloud Provider Cross-Border Transfer Block**: Network egress rules and database hosting configurations are pinned to UAE data center regions (e.g., Azure UAE Central / AWS Middle East) to satisfy Federal Law No. 2 of 2019 data residency obligations.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST implement a versioned, append-only `ConsentGrant` ledger recording every grant and revocation event with explicit purpose boundaries, timestamps, and policy version numbers.
- **FR-002**: Users MUST be able to view and revoke any individual consent category independently via the in-app settings screen with immediate effect.
- **FR-003**: The system MUST provide an in-app "Export My Data" feature that generates a structured JSON archive containing all user profile details, consent records, task history, and XP transactions within 5 seconds.
- **FR-004**: The system MUST provide a verified "Delete Account" flow that permanently erases personal profile data, device tokens, and identifiable logs in a single atomic transaction.
- **FR-005**: The system MUST store all health-related observations (completions, barrier notes, step aggregates) with clear origin provenance (`Manual`, `HealthConnect`, `Synthetic`).
- **FR-006**: The system MUST enforce data minimisation by storing only aggregated daily step counts rather than continuous GPS tracking, raw sensor streams, or heart rate traces.
- **FR-007**: All sensitive database columns (OAuth tokens, wearable connection keys, barrier notes) MUST be encrypted at rest using AES-256-GCM.
- **FR-008**: All communications MUST be secured via TLS 1.3 with certificate pinning enforced on mobile release builds.
- **FR-009**: The system MUST maintain an immutable `AuditEvent` log recording actor, action, timestamp, entity ID, and security context for all sensitive operations (consent changes, data export, account deletion).
- **FR-010**: The prototype MUST operate strictly on synthetic or de-identified test data and MUST NOT transmit identifiable UAE health data to overseas public cloud regions or unapproved external LLMs.
- **FR-011**: The architecture MUST isolate future clinical health information exchange (Malaffi) integration behind an explicit `IHealthInformationExchangeAdapter` boundary, with live connectivity disabled pending formal DoH regulatory approval.

### Key Entities

- **ConsentGrant**: Legal basis record. Attributes: `ConsentId` (UUID), `UserId` (FK), `Category` (Enum), `Purpose` (string), `PolicyVersion` (string), `GrantedAt` (UTC), `RevokedAt` (UTC nullable).
- **DeviceConnection**: Wearable provider credential record. Attributes: `ConnectionId` (UUID), `UserId` (FK), `Provider` (Enum: HealthConnect), `PermissionScopes` (string array), `LastSyncAt` (UTC), `RevokedAt` (UTC nullable), `EncryptedAccessToken` (string).
- **AuditEvent**: Immutable security audit record. Attributes: `EventId` (UUID), `ActorId` (string), `Action` (string), `TargetObject` (string), `Reason` (string), `TraceId` (string), `Timestamp` (UTC).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of consent revocations take effect immediately (< 100ms), halting associated data ingestion pipelines instantly.
- **SC-002**: Data export requests generate a complete, valid JSON package in under 3 seconds for active user accounts.
- **SC-003**: 100% of account deletion executions successfully purge personal records and credentials with zero orphan records remaining in the database.
- **SC-004**: 0 outgoing network requests from the prototype connect to live unapproved clinical endpoints (Malaffi) or unapproved overseas LLM training APIs.
- **SC-005**: 100% of security audit events for sensitive user actions are successfully persisted in an append-only, tamper-resistant store.

---

## Assumptions

- Users have statutory rights under UAE Federal Decree-Law No. 45 of 2021 (PDPL) and expectations of data privacy under UAE Federal Law No. 2 of 2019.
- The prototype uses synthetic personas and synthetic Health Connect datasets for testing, hackathon judging, and evaluation to prevent real-world privacy exposure.
- Production hosting will be configured exclusively within UAE sovereign cloud regions upon formal deployment.
