# Quickstart: Adaptive Recovery Loop

**Feature**: `003-adaptive-recovery-loop`  
**Status**: Ready  
**Date**: 2026-11-18  
**Prerequisites**: Features 001 and 002 implemented.

---

## 1. Validation Scenarios

### Scenario 1: Barrier Submission with +2 XP Reflection Credit
**Goal**: Verify submitting a non-judgmental barrier check-in records feedback and credits 2 XP.

```bash
curl -X POST http://localhost:5000/api/v1/recovery/barriers \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: c1d2e3f4-a5b6-7890-1234-56789abcdef0" \
  -d '{
    "taskInstanceId": "{MISSED_TASK_INSTANCE_ID}",
    "category": "TimingConflict",
    "userNote": "Extended shift at work today"
  }'
```

**Expected Outcome**:
- Status: `200 OK`
- `xpAwarded`: `2`
- `adaptation`: Proposal suggests `RescheduleWindow` (e.g. evening window)
- `isSafetyHaltActive`: `false`

---

### Scenario 2: Dynamic Adaptation Acceptance
**Goal**: Verify accepting an adaptation modifies today's active plan.

```bash
curl -X POST http://localhost:5000/api/v1/recovery/adaptations/{ADAPTATION_ID}/respond \
  -H "Content-Type: application/json" \
  -d '{
    "accept": true
  }'
```

**Expected Outcome**:
- Status: `200 OK`
- `status`: `"Accepted"`
- Active plan refreshed with reduced or rescheduled task.

---

### Scenario 3: Safety Intercept on Reported Illness
**Goal**: Verify selecting `UnwellOrPain` halts coaching and shows Abu Dhabi emergency contacts (998, 800 50).

```bash
curl -X POST http://localhost:5000/api/v1/recovery/barriers \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: e1f2a3b4-c5d6-7890-9876-543210fedcba" \
  -d '{
    "taskInstanceId": "{TASK_INSTANCE_ID}",
    "category": "UnwellOrPain",
    "userNote": "Feeling dizzy and feverish"
  }'
```

**Expected Outcome**:
- Status: `200 OK`
- `isSafetyHaltActive`: `true`
- `emergencyGuidance`: Contains `ambulanceNumber: "998"` and `telehealthNumber: "800 50"`
- All physical coaching tasks transitioned to `Paused`.
