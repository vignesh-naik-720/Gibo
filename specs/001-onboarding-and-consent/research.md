# Technical Research & Decision Records: Onboarding & Consent Governance

**Feature Branch**: `001-onboarding-and-consent` | **Date**: 2026-11-15

---

## 1. Architectural Style: Modular Monolith vs. Microservices

### Decision
Adopt a **Modular Monolith** architecture built on ASP.NET Core on .NET 10 LTS, organized into Domain-Driven Design (DDD) layers:
- `Gibo.Domain`: Enterprise entities, value objects, domain events, business invariants.
- `Gibo.Application`: Use cases, CQRS commands/queries (MediatR), validation pipelines (FluentValidation).
- `Gibo.Infrastructure`: EF Core PostgreSQL persistence, security/encryption services, audit log sinks.
- `Gibo.Api`: RESTful controllers, OpenAPI documentation, security headers, exception handling middleware.

### Rationale
1. **Team Velocity & Cohesion**: A small engineering team building an MVP in a hackathon or pilot setting experiences heavy overhead managing distributed transactions, network latency, multiple container deployments, and cross-service schema synchronization.
2. **Transactional Integrity**: Onboarding involves atomic creation of the `User`, initial `UserPreference`, multiple `ConsentGrant` records, and initial `Goal`. In a modular monolith, these are committed inside a single database transaction with ACID guarantees.
3. **Future Separation**: Strict internal module boundaries ensure that if a module (such as the AI Assistant or Health Connect Sync Engine) later requires independent scaling, it can be extracted into an independent microservice with zero architectural debt.

### Alternatives Considered
- *Microservices (User Service, Consent Service, Plan Service)*: Rejected due to unnecessary operational complexity, eventual consistency overhead, and deployment overhead for a student prototype.
- *Single-Project "Spaghetti" API*: Rejected because coupling database context directly with API controllers violates Principle VI and prevents unit testing of core rules.

---

## 2. Framework & Runtime Selection

### Decision
- **Backend**: ASP.NET Core on **.NET 10 LTS** (C# 13).
- **Mobile Client**: **React Native 0.88.x** + TypeScript (strict mode enabled).

### Rationale
1. **.NET 10 LTS Support**: .NET 10 is an active Long Term Support (LTS) release supported through November 2028. It provides enterprise-grade performance, native Ahead-Of-Time (AOT) compilation options, memory efficiency, and strong typing.
2. **React Native Android-First**: Enables cross-platform code reuse while allowing direct native Android integration via Kotlin bridges for Android Health Connect. TypeScript provides strict compile-time safety across complex onboarding state flows.

### Alternatives Considered
- *Node.js / Express or NestJS*: Rejected due to weaker concurrency performance, higher memory footprint, and lack of built-in LTS enterprise frameworks comparable to ASP.NET Core.
- *Flutter / Dart*: Rejected because Android Health Connect has superior official Kotlin Android ecosystem support and React Native provides extensive mature RTL libraries.

---

## 3. Database Selection: PostgreSQL + EF Core vs. NoSQL

### Decision
**PostgreSQL 16+** with **Entity Framework Core 10**.

### Rationale
1. **Relational Invariants**: Gibo's data model is strongly relational: `Users` have multiple `ConsentGrants`, a set of `Goals`, a historical sequence of `DailyPlans`, and auditable `XpTransactions`. Referential integrity (foreign keys, cascading delete protection) is essential to prevent orphaned privacy records.
2. **PostgreSQL Security & Extensions**: PostgreSQL natively supports JSONB (for flexible preference metadata), column-level encryption extensions, and `pgvector` (enabling seamless RAG vector retrieval in the same cluster for Feature 005 without maintaining a separate vector database).

### Alternatives Considered
- *MongoDB / Document DB*: Rejected because document databases lack strict schema-level relational constraints, making auditable consent revocation and cascaded account deletion complex and error-prone.

---

## 4. Privacy & Regulatory Compliance (UAE PDPL & Health ICT)

### Decision
Implement a **Granular Consent Ledger** with purpose limitation and strict UAE data sovereignty boundaries:
- Each processing purpose (`CoreProfile`, `WearableIntegration`, `ChatContext`, `ResearchTelemetry`) is an independent `ConsentGrant` entity.
- Consent records store UTC timestamps, policy version strings, and nullable revocation timestamps.
- All identifiable user data is stored in UAE-sovereign database regions (Azure UAE Central / AWS Middle East).
- Pre-checked consent toggles are strictly prohibited.

### Rationale
- Complies directly with **UAE Federal Decree-Law No. 45 of 2021 (PDPL)** Articles 5, 6, 14, and 15, as well as **UAE Federal Law No. 2 of 2019 (ICT in Health Fields)** Article 13.

### Alternatives Considered
- *Single Boolean `HasAcceptedTerms`*: Rejected as non-compliant with modern privacy laws, which require explicit consent per distinct processing purpose.

---

## 5. Localization & RTL (Right-to-Left) Architecture

### Decision
Utilize `i18next` and `react-i18next` combined with React Native's `I18nManager` for dynamic Right-to-Left (RTL) mirroring:
- English (default LTR)
- Modern Standard Arabic (`ar-AE`, full RTL layout mirroring)
- Hindi (`hi-IN`, LTR)
- Dynamic layout flipping on language change without requiring an application restart.

### Rationale
Abu Dhabi has a diverse multilingual population. Modern Standard Arabic is an official language and requires full RTL support, including mirrored navigation, back buttons, card alignments, and bidirectional text isolation (`dir="auto"`) for mixed alphanumeric strings (e.g., numbers and Latin measurements).

---

## 6. Client State Management & Offline Resilience

### Decision
- **Server State**: `@tanstack/react-query` v5 for query caching, automatic background re-fetching, and optimistic updates.
- **Local Onboarding UI State**: `zustand` v5 with persistent storage backed by `react-native-mmkv`.
- **API Idempotency**: All mutating requests (`POST /api/v1/onboarding`) include an `Idempotency-Key` header (UUIDv4) stored in Redis / PostgreSQL with a 24-hour expiration window.

### Rationale
Users may experience network drops during multi-step onboarding. Zustand + MMKV preserves entered answers locally across process restarts. The backend `Idempotency-Key` middleware ensures that if a user taps "Submit" under spotty cellular connectivity, duplicate accounts and duplicate consent grants are impossible.

---

## 7. Cryptographic Controls & Data at Rest Protection

### Decision
Sensitive user attributes (such as disclosed health comfort restrictions and contextual notes) are encrypted using **AES-256-GCM** authenticated envelope encryption before persistence in PostgreSQL.

### Rationale
Ensures defense-in-depth: even if a database backup or replica is compromised, sensitive personal wellness constraints cannot be read in plaintext. Cryptographic keys are managed outside the database via environment-injected secrets or Azure Key Vault.
