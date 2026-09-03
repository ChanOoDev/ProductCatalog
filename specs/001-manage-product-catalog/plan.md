# Implementation Plan: Product Catalog Management

**Branch**: `001-manage-product-catalog` | **Date**: 2026-09-03 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-manage-product-catalog/spec.md`

## Summary

Add protected product administration as REST endpoints backed by vertical MediatR slices. A domain
aggregate enforces product invariants and lifecycle rules; Application owns use cases, validation,
DTOs, and persistence/user/time/audit abstractions; Infrastructure owns EF Core MySQL persistence,
migrations, optimistic concurrency, and durable audit storage; API owns contracts, authorization,
ProblemDetails, OpenAPI, logging, metrics, and composition. Products are never deleted. Updates and
status transitions require an opaque ETag derived from an application-managed numeric version.

## Technical Context

**Language/Version**: C# with .NET 8 (`net8.0` in every existing project)

**Primary Dependencies**: ASP.NET Core controllers, MediatR 14.2, FluentValidation 12.1, EF Core 8,
Pomelo MySQL 8, Serilog.AspNetCore 8, Swashbuckle 10.2 (all already referenced)

**Storage**: MySQL through EF Core; `products` and `product_audit_events` tables

**Testing**: xUnit, FluentAssertions, coverlet, WebApplicationFactory, Testcontainers.MySql

**Target Platform**: Internal server-hosted ASP.NET Core Web API; deployment topology is supplied by
the existing organizational platform

**Project Type**: Web service in an existing four-project Clean Architecture solution

**Performance Goals**: First page of search over 100,000 products within two seconds for at least
95% of requests under the agreed normal-load profile

**Constraints**: Administrator authorization; end-to-end cancellation; page size 1-100; no physical
delete; UTC persistence; safe ProblemDetails; no secrets or submitted business values in logs/audit;
stale writes and duplicate SKU races return conflicts

**Scale/Scope**: One Product aggregate, six use cases, six REST operations, two new tables, four
production projects and three existing test projects; initial target catalog is 100,000 products

## Constitution Check

*GATE: Passed before research and re-checked after design.*

| Gate | Design evidence | Result |
|------|-----------------|--------|
| Existing architecture and inward dependencies | Domain has no references; Application references Domain; Infrastructure implements Application ports; API composes both | PASS |
| Vertical slices and existing CQRS/MediatR | Create, GetById, Search, Update, Activate, and Deactivate are separate MediatR slices | PASS |
| No unnecessary abstractions/dependencies | Reuse referenced packages; concrete persistence, current-user, clock, and audit ports only | PASS |
| REST, validation, ProblemDetails, pagination, cancellation, OpenAPI | Contract defines resource routes, allowlists, page envelope, errors, cancellation, and OpenAPI | PASS |
| EF Core/MySQL, migrations, UTC, invariants, concurrency, DTO isolation | Database constraints, numeric version, UTC fields, and separate contracts | PASS |
| Security and audit | Fail-closed admin policy, explicit write DTOs, safe errors, durable audit | PASS, pending host auth values |
| Testing and delivery gates | Existing test projects cover all layers; CI includes formatting, analysis, vulnerabilities, migration and contract checks | PASS |
| Human governance | Humans approve AI work and production; releases include rollback/smoke tests | PASS |

Post-design re-check: PASS. No constitution exception or complexity justification is required.

## Project Structure

### Documentation (this feature)

```text
specs/001-manage-product-catalog/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── product-catalog.openapi.yaml
└── tasks.md                         # created later by $speckit-tasks
```

### Source Code (repository root)

```text
src/
├── ProductCatalog.Domain/Products/                 # aggregate, status, domain errors
├── ProductCatalog.Application/
│   ├── Abstractions/                               # persistence, user, clock, audit ports
│   ├── Behaviors/                                  # validation pipeline
│   └── Products/{Create,GetById,Search,Update,Activate,Deactivate}/
├── ProductCatalog.Infrastructure/
│   ├── Persistence/                                # DbContext, mappings, adapters
│   ├── Persistence/Migrations/
│   └── Auditing/
└── ProductCatalog.Api/
    ├── Contracts/Products/
    ├── Controllers/ProductsController.cs
    ├── Errors/
    ├── Authorization/
    └── Observability/

tests/
├── ProductCatalog.Domain.UnitTests/Products/
├── ProductCatalog.Application.UnitTests/Products/
└── ProductCatalog.Api.IntegrationTests/{Infrastructure,Products,OpenApi}/
```

**Structure Decision**: Extend the existing projects. Keep feature code grouped under `Products`
and use-case folders. Do not add projects, a generic repository, a new user store, or alternative
framework stacks.

## Affected Projects and Components

- **Domain**: Product aggregate, status, guards, mutation and lifecycle behavior.
- **Application**: six handlers and validators; DTOs; typed outcomes; persistence, current-user,
  clock, and audit ports; validation behavior.
- **Infrastructure**: DbContext, mappings, migrations, projections/transactions, uniqueness and
  concurrency translation, audit persistence, dependency registration.
- **API**: routes/contracts, ETags, admin policy, principal adapter, ProblemDetails, OpenAPI,
  Serilog, metrics, composition.
- **Tests**: replace placeholders with domain/application suites and authenticated real-MySQL API
  integration tests. Leave WeatherForecast untouched unless separately approved for cleanup.

## API and Application Design

- `POST /api/products`: create; `201 Created`, Location, representation, and ETag.
- `GET /api/products/{id}`: detail with audit fields; `200` + ETag or `404`.
- `GET /api/products`: search/status/sort/direction/pageNumber/pageSize; allowlisted sorts and stable
  identifier tie-break; `200` page envelope.
- `PUT /api/products/{id}`: replace name, description, and price; SKU is immutable and MUST NOT
  appear in update input; requires `If-Match`; `200` + new ETag.
- `POST /api/products/{id}/activation` and `/deactivation`: require `If-Match`; `200` + ETag.
  Current-version same-state requests are no-ops and do not advance audit fields/version.
- No `DELETE`. Request models cannot bind identifier, status, audit, or version.
- Controllers translate HTTP, send MediatR requests, pass `RequestAborted`, and map typed results;
  handlers remain transport-independent.

## Authorization, Validation, and Errors

- Require `ProductRead` for GET operations and `ProductWrite` for create, update, activate, and
  deactivate. Both consume configurable claims from the application's existing authentication
  mechanism. Missing authentication, permission, or stable actor claim fails closed. Do not add
  Identity or users.
- FluentValidation validates requests/queries; Domain guards enforce business invariants; database
  constraints close bypass/race paths.
- ProblemDetails mapping: malformed/validation `400`; unauthenticated `401`; forbidden `403`;
  unknown product `404`; duplicate SKU/stale version `409`; unexpected `500`. Include stable type/
  code and trace ID, never exception, SQL, stack, or submitted values.
- Translate unique-index and EF concurrency exceptions to typed conflicts. Propagate cancellation.

## Concurrency and Audit

- Store unsigned monotonic `version` as EF concurrency token, incrementing only on real mutation.
  Encode it as a strong opaque ETag and require `If-Match`; stale values map to `409`.
- Derive actor from the trusted principal and time from a clock. Creation sets created/modified data;
  real mutations update only modified data.
- Commit product changes and success audit atomically. Record rejected mutations through an
  independent audit scope or enterprise sink; capture authorization failures at the API boundary.
  Never audit request bodies or raw values.

## Logging and Metrics

- Activate Serilog and one structured request event with trace ID, route, status, duration, allowed
  actor identifier, action, target ID, and outcome. Exclude tokens, credentials, bodies, SKU, name,
  description, and validation values.
- Add low-cardinality meters for route/status latency and count, command outcomes, validation,
  duplicates, concurrency conflicts, search duration, and page size. Never label with actor/product
  IDs. Use a supplied platform exporter; add none here.

## Migration, Rollback, and Compatibility

### Approved PR 2 database decisions

- Target MySQL `8.0.46` for this pilot and use `mysql:8.0.46` in Testcontainers. Pomelo uses an
  explicit `MySqlServerVersion(8, 0, 46)`; runtime and design-time setup must not use AutoDetect.
- Use `utf8mb4`. Store SKU uppercase (maximum 64) and enforce its case-insensitive unique index with
  `utf8mb4_0900_ai_ci`. Name is limited to 200, description to 2000, and audit actors to 200.
- Currency is configuration-only: `Catalog:Currency=SGD`; Product has no Currency column in PR 2.
- Preserve required `ModifiedAtUtc` and `ModifiedBy` terminology. Version is an optimistic
  concurrency token, and physical deletion is unsupported.
- MySQL 8.0 reached end of life in April 2026 and is unsuitable for a new production deployment.
  Production release is blocked until MySQL 8.4 LTS compatibility is validated or the EF/provider
  stack is upgraded.

- Create and review an initial migration for both tables; generate SQL for target MySQL and validate
  it from empty state in a container. Confirm restore point, migrate, deploy, smoke-test, then monitor.
- Roll back binaries first while additive tables remain. Once data exists, prefer route disablement
  or forward fix; dropping tables requires human approval, retention confirmation, and tested restore.
- `/api/products` is the new baseline. Future changes remain additive and retain field/route meanings,
  enums, defaults, problem codes, and ETag semantics. Gate changes with OpenAPI diff tests. Add no API
  versioning package until an organizational convention requires it.

## Test Strategy and Quality Gates

- **Domain unit**: normalization, required fields, price, initial state, edits, transitions/no-op,
  UTC audit changes, and version increments.
- **Application unit**: handlers, validation, cancellation, actor/time, duplicate/concurrency results,
  sorts/tie-breaks, paging/totals, and no writes after invalid input.
- **Integration**: WebApplicationFactory + MySQL Testcontainer + migrations; auth, HTTP/ProblemDetails,
  uniqueness/races, search/filter/sort/page, concurrency, no-op, UTC, audit sanitization, OpenAPI and
  no DELETE. Test 100,000-row performance separately under controlled load.
- **PR CI**: restore; Release build with warnings as errors; format verification; unit/container tests
  with coverage; static analysis; vulnerability scan; clean migration; architecture-reference check;
  OpenAPI compatibility diff after baseline. Critical/high findings block.
- PRs link requirements/tasks and require human review of AI output. Production requires human
  approval plus rollback and smoke-test instructions.

## Assumptions and Decisions Required Before Implementation

No technical mechanism remains unclear, but these absent environment/business values MUST be
provided before their affected task begins:

1. Existing authentication scheme, issuer/audience, ProductRead claim/value, ProductWrite
   claim/value, and stable actor claim. These values block the first endpoint implementation.
2. SKU character policy beyond required trimmed uppercase storage; lengths are approved.
3. MySQL 8.4 LTS compatibility or provider upgrade required before production release.
4. Currency serialization in later API contracts; `SGD` configuration is approved, while conversion,
   tax, discounts, and price history are out of scope.
5. Audit retention, access/tamper policy, sink availability, and actor-field visibility.
6. Telemetry exporter/backend and normal-load profile for the two-second p95 target.
7. Whether a versioned route is mandated. Otherwise `/api/products` is the baseline.

## Complexity Tracking

No constitution violations require justification.
