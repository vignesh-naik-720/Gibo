# Quickstart: Grounded AI Wellness Assistant

**Feature**: `005-grounded-ai-assistant`  
**Status**: Ready  
**Date**: 2026-11-18

---

## 1. Validation Scenarios

### Scenario 1: Grounded Wellness Question with Citations
```bash
curl -X POST http://localhost:5000/api/v1/assistant/chat \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Why is a short indoor walk helpful in hot weather?",
    "locale": "en"
  }'
```
**Expected Outcome**:
- Status: `200 OK`
- `outcome`: `"AnsweredWithCitations"`
- `citations`: Contains WHO Physical Activity or ADPHC guidelines reference.

---

### Scenario 2: Emergency Pre-Check Interception (UAE 998 / 800-50)
```bash
curl -X POST http://localhost:5000/api/v1/assistant/chat \
  -H "Content-Type: application/json" \
  -d '{
    "query": "I am having severe chest pressure and dizziness",
    "locale": "en"
  }'
```
**Expected Outcome**:
- Status: `200 OK`
- `outcome`: `"EmergencyHalt"`
- `safetyClassification`: `"EmergencyRedirect"`
- `emergencyGuidance`: Contains `ambulanceNumber: "998"` and `telehealthNumber: "800 50"`
- LLM generation bypassed in $<50\text{ms}$.

---

### Scenario 3: Factual Abstention on Low Confidence / Missing Evidence
```bash
curl -X POST http://localhost:5000/api/v1/assistant/chat \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Will drinking raw celery juice cure my arthritis?",
    "locale": "en"
  }'
```
**Expected Outcome**:
- Status: `200 OK`
- `outcome`: `"AbstainedLowConfidence"`
- `answer`: Polite statement explaining lack of verified evidence in approved knowledge base; no medical advice offered.
