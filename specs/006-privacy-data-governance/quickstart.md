# Quickstart: Privacy, Trust & Abu Dhabi Data Governance

**Feature**: `006-privacy-data-governance`  
**Status**: Ready  
**Date**: 2026-11-18

---

## 1. Validation Scenarios

### Scenario 1: Immediate Consent Revocation
```bash
curl -X POST http://localhost:5000/api/v1/privacy/consents/HealthConnectSync/revoke
```
**Expected Outcome**:
- Status: `200 OK`
- `isGranted`: `false`
- `revokedAtUtc`: Timestamp present.
- Device background sync immediately halts.

---

### Scenario 2: Data Portability Export (PDPL Article 14)
```bash
curl -X POST http://localhost:5000/api/v1/privacy/export
```
**Expected Outcome**:
- Status: `200 OK`
- Delivers JSON dossier containing user profile, consent records, habit summary, and XP ledger in $<3$ seconds.
- Audit event logged to `AuditEvents` table.

---

### Scenario 3: Atomic Account Deletion (PDPL Article 15)
```bash
curl -X DELETE "http://localhost:5000/api/v1/privacy/account?confirmation=CONFIRM_DELETE"
```
**Expected Outcome**:
- Status: `204 No Content`
- User profile, device connections, and habit records completely purged.
- Subsequent calls with old JWT tokens return HTTP `401 Unauthorized`.
