# Quickstart: Gamification & Weekly Styles

**Feature**: `004-gamification-and-weekly-styles`  
**Status**: Ready  
**Date**: 2026-11-18

---

## 1. Validation Scenarios

### Scenario 1: Verify Immutable XP Ledger Balance
```bash
curl -X GET http://localhost:5000/api/v1/gamification/balance
```
**Expected Outcome**:
- `totalXp`: Matches the sum of positive transaction amounts.
- Every transaction has `amount > 0` and an explicit reason (`TaskCompleted`, `ReflectionReward`, etc.).

---

### Scenario 2: Weekly Style Retrieval & Toggle
```bash
# 1. Fetch current weekly style
curl -X GET http://localhost:5000/api/v1/gamification/weekly-style

# 2. Toggle user visibility preference to hide style card
curl -X PUT http://localhost:5000/api/v1/gamification/weekly-style/toggle-visibility \
  -H "Content-Type: application/json" \
  -d '{"hide": true}'
```
**Expected Outcome**:
- Status: `200 OK`
- `assignedStyle`: One of 5 approved non-clinical styles.
- `isHiddenByUser`: `true`.

---

### Scenario 3: Monthly Trends with Provenance Separation
```bash
curl -X GET http://localhost:5000/api/v1/gamification/monthly-trends?month=2026-11
```
**Expected Outcome**:
- Separates `sensorUnits` (Health Connect steps) from `selfReportedCount`.
- Neutral, observational summary text (e.g. "Active on 4 out of 7 days").
