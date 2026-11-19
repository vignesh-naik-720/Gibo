# Quickstart & Verification Guide: Daily Plan & Action Engine

**Feature**: `002-daily-plan-and-action-engine`  
**Status**: Ready  
**Date**: 2026-11-15  
**Prerequisites**: Feature 001 implemented, Gibo backend and mobile solution compiled.

---

## 1. Prerequisites & Test Setup

Ensure backend API is running locally:
```powershell
# From repository root
dotnet run --project src/backend/Gibo.Api/Gibo.Api.csproj
```
API Base URL: `http://localhost:5000` or `https://localhost:5001`.

---

## 2. Validation Scenarios

### Scenario 1: Generate Today's Plan with 5-Factor Ranking
**Goal**: Verify that today's plan generates 1 to 3 curated micro-habits matching user preferences and Abu Dhabi weather constraints.

```bash
# 1. Request plan generation with an Idempotency-Key
curl -X POST http://localhost:5000/api/v1/plans/generate \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d" \
  -d '{"forceRegenerate": false}'
```

**Expected Outcome**:
- Status: `201 Created` (or `200 OK` on repeated idempotent calls)
- Response JSON contains `planId`, `planDate` (today in `Asia/Dubai`), and between 1 and 3 `tasks`.
- Each task includes an `actionVerb`, `targetValue`, `targetUnit`, and `rationale` (e.g., "Fits your indoor preference and 15m budget").

---

### Scenario 2: Manual Task Completion (10 XP Awarded)
**Goal**: Verify that a user can mark an action complete with one tap, awarding exactly 10 XP and recording evidence as `Manual`.

```bash
# Complete a task manually
curl -X POST http://localhost:5000/api/v1/tasks/{TASK_INSTANCE_ID}/complete \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: f1e2d3c4-b5a6-7890-1234-56789abcdef0" \
  -d '{
    "evidenceSource": "Manual",
    "clientEventId": "f1e2d3c4-b5a6-7890-1234-56789abcdef0",
    "recordedAtUtc": "2026-11-15T15:30:00Z"
  }'
```

**Expected Outcome**:
- Status: `200 OK`
- `state`: `"Completed"`
- `xpAwarded`: `10`
- `isDuplicate`: `false`

---

### Scenario 3: Health Connect Background Sync (Equality Parity Test)
**Goal**: Verify that automated Health Connect synchronization marks completion and awards the exact same 10 XP without inequality.

```bash
# Complete a step task via Health Connect
curl -X POST http://localhost:5000/api/v1/tasks/{STEP_TASK_INSTANCE_ID}/complete \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: 99887766-5544-3322-1100-aabbccddeeff" \
  -d '{
    "evidenceSource": "HealthConnect",
    "measuredValue": 1250,
    "clientEventId": "99887766-5544-3322-1100-aabbccddeeff",
    "recordedAtUtc": "2026-11-15T15:45:00Z"
  }'
```

**Expected Outcome**:
- Status: `200 OK`
- `state`: `"Completed"`
- `xpAwarded`: `10` (Identical to Manual mode)
- `evidenceSource`: `"HealthConnect"`

---

### Scenario 4: User Override: Swap Task with Alternative
**Goal**: Verify that a user can swap a task for an approved domain alternative without penalty or streak reset.

```bash
curl -X POST http://localhost:5000/api/v1/tasks/{TASK_INSTANCE_ID}/swap \
  -H "Content-Type: application/json" \
  -d '{
    "replacementTemplateId": "{ALTERNATIVE_TEMPLATE_ID}",
    "reason": "Air conditioning maintenance today"
  }'
```

**Expected Outcome**:
- Status: `200 OK`
- Returns new replacement `TaskInstanceDto` in the active plan.
- The parent `DailyPlanDto.userEditsApplied` is updated to `true`.

---

### Scenario 5: Offline Batch Synchronization & Idempotency
**Goal**: Simulate device offline event queueing and verify flush deduplication.

```bash
curl -X POST http://localhost:5000/api/v1/tasks/sync-offline \
  -H "Content-Type: application/json" \
  -d '{
    "events": [
      {
        "clientEventId": "f1e2d3c4-b5a6-7890-1234-56789abcdef0",
        "taskInstanceId": "{TASK_INSTANCE_ID}",
        "evidenceSource": "Manual",
        "recordedAtUtc": "2026-11-15T15:30:00Z"
      }
    ]
  }'
```

**Expected Outcome**:
- Status: `200 OK`
- `duplicateCount`: `1` (Recognized as already completed from Scenario 2, zero double-credited XP)
- `syncedCount`: `0`
