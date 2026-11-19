<!-- Sync Impact Report
Version Change: 0.0.0 -> 1.0.0 (Initial Ratification)
Added Principles:
  - Principle I: User Agency & Autonomy Before Engagement
  - Principle II: Feasibility & Adaptive Recovery Over Rigid Streaks
  - Principle III: Evidence-Based AI, Curated Grounding & Non-Negotiable Safety Guardrails
  - Principle IV: Privacy by Design, UAE Data Sovereignty & Health ICT Compliance
  - Principle V: Inclusive Equity, Multilingual Parity & Manual-Device Equality
  - Principle VI: Test-First Discipline, Modular Monolith Architecture & Offline Resilience
Added Sections:
  - Technical Architecture & Security Standards
  - Code Quality, Architectural Concepts & Comprehensive Commenting Guidelines
  - Development Workflow, Quality Gates & Compliance Audits
  - Governance & Amendment Procedure
Deferred Items: None
-->

# Gibo Project Constitution

Healthy habits that adapt. Progress that survives real life.
An adaptive behaviour-support companion for Abu Dhabi.

## Core Principles

### Principle I: User Agency & Autonomy Before Engagement
Every interaction within Gibo MUST respect user agency and control. Users are sovereign over their goals, action caps (1 to 3 actions daily), data sources, reminders, quiet hours, and privacy permissions. The platform MUST NOT deploy coercive dark patterns, shame-based notifications, or punitive streak resets. All gamification (XP, private milestones, weekly styles) MUST remain strictly private, non-clinical, and user-configurable (with a toggle to disable). Individual user activity, habits, or scores MUST NEVER be exposed to employers, insurers, healthcare payers, or public leaderboards.

### Principle II: Feasibility & Adaptive Recovery Over Rigid Streaks
A missed task or lapse is diagnostic information about the plan's feasibility—NOT a character judgment or personal failure. The core innovation of Gibo is the closed feedback loop: `Plan -> Act -> Observe -> Understand Barrier -> Adapt -> Recover`. When a task is skipped or engagement drops:
1. The system MUST NOT deduct XP, reset progress counters, or emit guilt-inducing microcopy.
2. The system MUST trigger an empathetic, non-judgmental barrier check-in (e.g., timing, difficulty, fatigue, access, illness).
3. The system MUST dynamically adjust the plan (proposing smaller increments, indoor weather alternatives, shifted schedules, or temporary pauses).
4. Re-engagement after an extended absence MUST acknowledge return neutrally, restore the routine at an accessible baseline, and preserve all historical effort.

### Principle III: Evidence-Based AI, Curated Grounding & Non-Negotiable Safety Guardrails
Gibo provides general wellness companion support and is explicitly NOT a diagnostic tool, clinical triage system, or medical prescriber.
1. **Adaptive Task Engine**: The daily task selector MUST operate as a deterministic, rules-first ranking engine over a curated, clinician-reviewed task catalog. AI models MUST NOT invent arbitrary exercises, nutritional restrictions, fasting regimens, or clinical interventions.
2. **Wellness Chatbot (RAG)**: The conversational assistant MUST be strictly grounded in a curated, versioned knowledge base (WHO guidelines, UAE/Abu Dhabi Department of Health public health publications, and clinician-approved wellness copy).
3. **Guardrails & Safe Abstention**: If retrieval confidence is low, contradictory, or absent, the AI MUST explicitly abstain ("I do not have verified evidence on this topic") rather than speculate. Any request expressing emergency symptoms, self-harm, eating disorders, or medication adjustments MUST immediately bypass conversational coaching and display localized emergency contacts and professional healthcare referrals.

### Principle IV: Privacy by Design, UAE Data Sovereignty & Health ICT Compliance
All personal and health-related information MUST be treated as high-risk confidential data:
1. **Regulatory Alignment**: The architecture and data lifecycle MUST comply with UAE Federal Decree-Law No. 45 of 2021 (Personal Data Protection Law - PDPL), UAE Federal Law No. 2 of 2019 (ICT in Health Fields), and Abu Dhabi Department of Health Information and Cyber Security Standard (ADHICS V2).
2. **Data Sovereignty**: Identifiable UAE health data MUST NOT be transmitted to unapproved overseas third parties, public cloud regions, or external LLM APIs without formal legal authorization. Hackathon and preview environments MUST exclusively utilize synthetic profiles or de-identified test sets.
3. **Granular Consent Ledger**: Consent MUST be recorded per distinct category (profile, wearable sync, chat queries, research analytics) with versioned grant and revocation timestamps. Revocation MUST be instant and auditable.
4. **User Rights**: Users MUST possess full in-app capabilities to export their complete data dossier in portable JSON/CSV format and initiate immediate, verified account and telemetry deletion.

### Principle V: Inclusive Equity, Multilingual Parity & Manual-Device Equality
Health support must be universally accessible across the diverse population of the UAE regardless of income, nationality, or physical capability:
1. **Device Parity**: Wearable device integration (via Android Health Connect) is strictly an optional convenience. Manual check-off is a first-class evidence mode that awards identical XP (10 XP per completion) without penalization or second-class status.
2. **First-Class Trilingual Experience**: English, Arabic, and Hindi MUST be supported with full semantic parity. Modern Standard Arabic MUST support comprehensive Right-to-Left (RTL) layout mirroring, localized number handling, and clinically vetted translations.
3. **Universal Accessibility**: The mobile interface MUST adhere to WCAG 2.1 AA standards, supporting scalable typography, high-contrast ratios, screen-reader semantics (TalkBack), reduced-motion preferences, and minimum 48x48 dp touch targets. Tasks MUST support low-mobility and weather-appropriate (indoor cooling) alternatives.

### Principle VI: Test-First Discipline, Modular Monolith Architecture & Offline Resilience
Software quality and maintainability are critical for health-supporting software:
1. **Modular Monolith**: The backend MUST be implemented as a clean, modular monolith in ASP.NET Core on .NET 10 LTS utilizing Domain-Driven Design (DDD) principles with clear project boundaries (`Gibo.Domain`, `Gibo.Application`, `Gibo.Infrastructure`, `Gibo.AI`, `Gibo.Api`).
2. **Relational Data Integrity**: The data layer MUST utilize PostgreSQL with EF Core, enforcing strict foreign key constraints, transactional consistency, audit trails, and idempotency keys on all event submissions.
3. **Offline Resilience**: The React Native client MUST cache daily plans locally, allow offline completion and barrier submissions, and safely synchronize with the API upon network restoration without duplicate events or corrupted state.
4. **Test-Driven Verification**: Core business rules—especially task scoring, barrier adaptation algorithms, XP calculations, and safety filter triage—MUST be accompanied by exhaustive unit and contract test suites prior to deployment.

---

## Technical Architecture & Security Standards

### 1. Technology Stack
- **Mobile Client**: React Native 0.88.x + TypeScript, React Navigation, TanStack Query (server state), Zustand (local UI state), i18next (trilingual i18n/RTL), Android Health Connect native Kotlin bridge.
- **Backend API**: ASP.NET Core Web API on .NET 10 LTS (C# 13), MediatR / Clean CQRS pattern, FluentValidation.
- **Persistence & ORM**: PostgreSQL 16+ with Entity Framework Core 10, connection pooling, and optimistic concurrency tokens.
- **Caching & Background Processing**: Redis (transient caching, token blacklist, rate limiting) and .NET Background Services / Hangfire for recurring plan generation and weekly insight computation.
- **AI & RAG Pipeline**: Backend orchestration service with vector embeddings (pgvector or approved local vector index), strict prompt templates with JSON-schema output validation, PII redaction layer, and output verification filters.

### 2. Security & Cryptographic Controls
- **Transport Security**: TLS 1.3 mandatory for all network communications with HTTP Strict Transport Security (HSTS) and certificate pinning in mobile builds.
- **Data at Rest**: Sensitive database columns (OAuth tokens, wearable connection credentials, barrier notes) encrypted using AES-256-GCM with envelope encryption and keys stored in a hardware security module or managed key vault.
- **Authentication & Authorization**: Stateless JWT authentication with short-lived access tokens (15 minutes) and rotating refresh tokens with device binding. Role-Based Access Control (RBAC) enforced with strict row-level tenant/user ownership checks (`UserId` from claims MUST match route/entity ownership).
- **Injection & Input Defense**: Zero dynamic SQL; EF Core parameterized queries only. Request bodies validated against strict FluentValidation schemas. RAG prompt inputs sanitized and wrapped in protective delimiter boundaries to eliminate prompt injection vulnerabilities.
- **Audit Logging**: Comprehensive, append-only `AuditEvent` log recording actor, action, timestamp, entity ID, and security context for all sensitive operations (consent changes, data exports, account deletions, emergency safety triggers).

---

## Code Quality, Architectural Concepts & Developer Understanding Guidelines

To ensure the codebase remains maintainable, secure, and easily understood by current and future developers:

### 1. Mandatory Code Concepts & Commenting Structure
1. **Domain Intent Comments**: Every domain entity, value object, and service MUST begin with an architectural summary explaining its role within the behaviour-change system and why it exists.
2. **Algorithmic Transparency**: Complex mathematical or heuristic formulas (such as the 5-factor task ranking score, barrier adjustment matrix, or weekly style categorization) MUST feature block comments detailing each component weight, rationale, and mathematical derivation.
3. **Line-by-Line Clarifications for Critical Pathways**: Security-sensitive routines (JWT token validation, consent verification filters, Health Connect bridge marshaling, safety classification triage) MUST contain inline step-by-step comments explaining the technical mechanism and threat mitigation.
4. **Meaningful Nomenclature**: Avoid ambiguous acronyms. Use clear ubiquitous language: `TaskTemplate`, `TaskInstance`, `BarrierFeedback`, `RecoveryAdjustment`, `CompletionEvent`.

### 2. Coding Practices & Clean Code Rules
- **Immutability & Value Objects**: Domain state modifications MUST occur through explicit domain methods on aggregate roots that enforce invariants, never via public property setters.
- **Defensive Programming**: Validate preconditions, handle nullability with C# nullable reference types and TypeScript strict mode enabled (`"strict": true`), and never swallow exceptions silently.
- **Idempotency Guarantees**: All state-modifying POST/PUT endpoints MUST accept a client-generated `Idempotency-Key` header, caching responses to protect against mobile duplicate submissions under intermittent cellular connectivity.

---

## Development Workflow, Quality Gates & Compliance Audits

1. **Static Analysis & Linting**: Strict ESLint, Prettier, and TypeScript checks for React Native; Roslyn analyzers, StyleCop, and SonarAnalyzer for .NET with zero compiler warnings permitted.
2. **Testing Pyramid**:
   - **Unit Tests**: Minimum 85% code coverage for `Gibo.Domain` and `Gibo.Application` logic.
   - **Integration Tests**: WebApplicationFactory tests with testcontainers PostgreSQL validating database migrations, repository transactions, and API contracts.
   - **Safety Evaluation Benchmarks**: Automated regression testing on 100+ prompt test cases assessing chatbot abstention accuracy, emergency redirection, and zero unauthorized medical claims.
3. **Code Review Gate**: Every Pull Request requires review and approval by at least one core engineer, verifying compliance with the Gibo Project Constitution, security standards, and accessibility guidelines.

---

## Governance & Amendment Procedure

1. **Supremacy**: This Constitution represents the supreme architectural and ethical framework for the Gibo project. Any code or design pattern that violates these principles MUST NOT be merged.
2. **Amendment Protocol**: Amendments require:
   - A documented Request for Comment (RFC) detailing the motivation, security impact, and ethical implications.
   - Formal approval from the project lead and security/health compliance reviewer.
   - A migration plan for any affected schemas, APIs, or user data.
3. **Version Increment Policy**:
   - **MAJOR (x.0.0)**: Modifications that redefine fundamental principles, data governance boundaries, or safety protocols.
   - **MINOR (1.x.0)**: Additions of new behavior domains, integration adapters, or architectural modules.
   - **PATCH (1.0.x)**: Clarifications, wording refinements, and non-semantic adjustments.

**Version**: 1.0.0 | **Ratified**: 2026-11-15 | **Last Amended**: 2026-11-15
