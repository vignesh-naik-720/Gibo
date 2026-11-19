# Quickstart Validation Guide: Onboarding & Consent Governance

**Feature Branch**: `001-onboarding-and-consent` | **Date**: 2026-11-15

---

## 1. Prerequisites

- **.NET 10 SDK** (v10.0.100+) installed
- **Node.js** (v20+ LTS) and **pnpm** (v9+)
- **PostgreSQL 16+** (local instance or Docker container)
- **Android SDK** (API Level 34) for running the React Native mobile client emulator

---

## 2. Environment Setup

### 2.1 Backend Setup & Database Migration
```bash
# Clone and navigate to project root
cd Gibo

# Restore backend dependencies
dotnet restore src/backend/Gibo.Api/Gibo.Api.csproj

# Run Entity Framework Core migrations to create database schema
dotnet ef database update --project src/backend/Gibo.Infrastructure --startup-project src/backend/Gibo.Api

# Start the ASP.NET Core Web API server (runs on https://localhost:5001)
dotnet run --project src/backend/Gibo.Api/Gibo.Api.csproj
```

### 2.2 Mobile Client Setup
```bash
# Navigate to mobile application directory
cd mobile

# Install dependencies
pnpm install

# Start Metro bundler and launch Android emulator
pnpm android
```

---

## 3. End-to-End Validation Scenarios

### Scenario 1: Successful Onboarding with Granular Consent (cURL)

**Objective**: Verify that submitting a valid adult registration creates the `User` aggregate, records the `ConsentGrant` ledger items, and returns the tailored starter plan.

```bash
curl -X POST "https://localhost:5001/api/v1/onboarding" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: a1b2c3d4-e5f6-7890-abcd-ef1234567890" \
  -d '{
    "isAdultConfirmed": true,
    "preferredLocale": "ar",
    "timezoneId": "Asia/Dubai",
    "primaryGoalDomain": "Movement",
    "workPattern": "StandardDaytime",
    "indoorPreferred": true,
    "maxDailyActions": 1,
    "baselineFitnessLevel": "Starter",
    "consents": [
      {
        "category": "CoreProfile",
        "granted": true,
        "policyVersion": "2026.1"
      },
      {
        "category": "HealthConnectSync",
        "granted": false,
        "policyVersion": "2026.1"
      }
    ]
  }'
```

**Expected Response**: `HTTP 201 Created`
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Active",
  "registeredAt": "2026-11-15T14:30:00Z",
  "firstPlan": {
    "taskTemplateId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "instruction": "Take a 7-minute indoor walk after lunch",
    "verb": "Walk",
    "target": "7 minutes",
    "estimatedMinutes": 7,
    "evidenceMode": "Manual",
    "whyItFits": "Matches your preference for indoor activity and provides a gentle starter baseline."
  }
}
```

---

### Scenario 2: Under-18 Age Rejection Validation

**Objective**: Verify that user eligibility is strictly enforced at domain and API levels.

```bash
curl -X POST "https://localhost:5001/api/v1/onboarding" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: b2c3d4e5-f6a7-8901-bcde-f23456789012" \
  -d '{
    "isAdultConfirmed": false,
    "preferredLocale": "en",
    "timezoneId": "Asia/Dubai",
    "primaryGoalDomain": "Movement",
    "consents": [
      {
        "category": "CoreProfile",
        "granted": true,
        "policyVersion": "2026.1"
      }
    ]
  }'
```

**Expected Response**: `HTTP 400 Bad Request`
```json
{
  "errorCode": "ELIGIBILITY_UNDERAGE",
  "message": "Gibo is restricted to adults (18+) for wellness habit support."
}
```

---

### Scenario 3: Idempotent Resubmission Test

**Objective**: Verify that re-sending the exact same `Idempotency-Key` returns the cached response without creating duplicate database rows or conflicting records.

```bash
# Re-run Scenario 1 command with identical Idempotency-Key: a1b2c3d4-e5f6-7890-abcd-ef1234567890
# Expected Response: HTTP 200/201 returning original userId without duplicate row creation.
```

---

## 4. Automated Test Suites

```bash
# Run backend domain unit tests (Validates aggregate invariants and consent rules)
dotnet test tests/backend/Gibo.Domain.UnitTests/

# Run API contract and integration tests (Runs against PostgreSQL Testcontainer)
dotnet test tests/backend/Gibo.Api.IntegrationTests/

# Run mobile onboarding UI and RTL layout tests
cd mobile && pnpm test
```
