# Research: Grounded AI Wellness Assistant & Clinical Safety Guardrails

**Feature**: `005-grounded-ai-assistant`  
**Status**: Completed  
**Date**: 2026-11-18  
**Constitution**: Adheres strictly to [constitution.md](file:///c:/Users/Vignesh/OneDrive/Desktop/Gibo/.specify/memory/constitution.md)

---

## 1. 7-Stage Defensive RAG Architecture

### Context & Problem
Adherence to Principle III (Evidence Before AI Fluency) and WHO AI Health Governance guidelines demands that conversational AI must never hallucinate health advice, diagnose conditions, or prescribe medications.

### Decision
Implement a 7-stage deterministic orchestration pipeline in `Gibo.AI.Orchestration.GroundedAssistantPipeline`:
1. **Input Sanitization**: Strip prompt injection tags, control sequences, and redact identifiable phone/email strings.
2. **Pre-Check Safety Classifier**: Deterministic regex + fast semantic classifier evaluating 6 risk categories: Acute Emergency, Self-Harm, Diagnosis, Medication, Disordered Eating, and Prompt Injection.
3. **Curated Vector Retrieval**: Restrict search exclusively to chunked embeddings from pre-approved `KnowledgeSource` records (WHO, DoH Abu Dhabi, Gibo Clinical Team).
4. **Deterministic Abstention Check**: If highest cosine similarity chunk is $<0.78$ or empty, abort LLM generation immediately and output pre-approved abstention text.
5. **Constrained Generation**: Prompt template locks LLM to retrieved context and forces inline citation tags (`[Source:ID]`).
6. **Post-Check Citation Verifier**: Scans output text; if any health claim lacks a matching source chunk, discards response and falls back to safe abstention.
7. **Privacy-Safe Audit Logging**: Logs only source IDs, classification outcome, and timestamps; raw prompt text is redacted.

---

## 2. Knowledge Source Whitelist & Seed Ingestion

### Context & Problem
General web scraping introduces dangerous medical fads and contradictory wellness claims.

### Decision
Seed the knowledge store with 15 versioned, clinician-signed documents:
- **WHO Guidelines on Physical Activity and Sedentary Behaviour (2020)**
- **Abu Dhabi Public Health Center (ADPHC) Workplace Wellness Guide**
- **UAE National Guidelines for Clinical Nutrition & Hydration**
- **National Sleep Foundation Guidelines for Adults**
- **Clinician-Reviewed Gibo Micro-Habit Guides (Indoor walking, AC heat pacing, digital sundown)**

---

## 3. High-Risk Emergency Routing (UAE Numbers)

### Context & Problem
If a user experiencing chest pain, acute distress, or suicidal ideation types into the assistant, generic conversational empathy can cause fatal delays.

### Decision
Bypass the LLM in $<50\text{ms}$ and return localized UAE emergency resources:
- **Abu Dhabi Emergency Services (Ambulance)**: **998**
- **Police Emergency**: **999**
- **SEHA Healthcare Teleconsultation**: **800 50**
- **UAE National Mental Health Support Line (Hopeline)**: **800 4673**
- Mobile UI renders high-visibility urgent action card with native dialer intent.
