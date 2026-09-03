# Research: Product Catalog Management

## Repository Baseline

**Decision**: Extend the existing .NET 8 Domain, Application, Infrastructure, API, and test projects
with their already referenced packages.

**Rationale**: Dependency direction already follows Clean Architecture. MediatR, FluentValidation,
EF Core/Pomelo MySQL, Serilog, Swashbuckle, xUnit, FluentAssertions, WebApplicationFactory, and
Testcontainers.MySql are already referenced.

**Alternatives considered**: New projects or alternate frameworks duplicate current capabilities.

## Vertical Slices and Persistence Boundary

**Decision**: Create six MediatR slices and only concrete Application ports for persistence, actor,
time, and audit. Infrastructure uses a DbContext behind those ports; no generic repository.

**Rationale**: This preserves dependency direction and exposes EF capabilities needed for projection,
paging, uniqueness, and concurrency.

**Alternatives considered**: Layer-wide services and generic repositories add indirection without a
requirement.

## Product Identity and SKU Uniqueness

**Decision**: Use a stable UUID and store display SKU plus an invariant-uppercase normalized SKU
with a unique binary-collated index.

**Rationale**: Uniqueness remains deterministic across database collations while display casing is
preserved.

**Alternatives considered**: Direct case-insensitive collation depends on server rules; numeric IDs
provide no business value.

## Optimistic Concurrency

**Decision**: Use an application-managed unsigned numeric EF concurrency token, exposed as an
opaque strong ETag and required through `If-Match`. Stale writes return `409 Conflict`.

**Rationale**: MySQL has no SQL Server rowversion. Numeric tokens support atomic version predicates;
ETags avoid mass-assignable body versions and align with HTTP.

**Alternatives considered**: Timestamps are precision-sensitive, random tokens are larger, and body
versions duplicate HTTP semantics. The approved spec selects 409 rather than 412.

## REST and Lifecycle Contract

**Decision**: Use `/api/products`, POST create, GET collection/detail, PUT replacement of name,
description, and price only, and explicit POST activation/deactivation. SKU is immutable and no
DELETE operation is exposed.

**Rationale**: Explicit contracts prevent mass assignment and make lifecycle audit intent clear.

**Alternatives considered**: PATCH complicates validation; DELETE-as-soft-delete communicates the
wrong behavior; arbitrary sorting is unsafe and unstable.

## Authorization Integration

**Decision**: Define fail-closed `ProductRead` and `ProductWrite` policies whose authentication
scheme, permission claims, and actor claim are supplied by the host. Add no Identity or user store.

**Rationale**: The API calls authorization middleware but contains no authentication registration,
policy, handler, or store. The host must own organizational identity.

**Alternatives considered**: Feature-owned Identity violates scope; anonymous access violates policy;
hard-coded roles assume an unknown issuer.

## Validation and ProblemDetails

**Decision**: Apply FluentValidation to request/query shapes, aggregate guards to invariants,
database constraints to integrity, and one API ProblemDetails mapper.

**Rationale**: Layered enforcement gives safe feedback while protecting all paths and races.

**Alternatives considered**: Controller-only or domain-only validation leaves gaps; raw exceptions
leak details.

## Audit and Observability

**Decision**: Persist business audit events; transact successful events with product changes and use
an independent scope/enterprise sink for rejected attempts. Configure existing Serilog and
low-cardinality meters, choosing no new exporter.

**Rationale**: Operational logs are not durable business audit. Separate rejected-attempt writes
survive rollback. Platform-neutral instruments avoid an unsupported backend dependency.

**Alternatives considered**: Logging-only audit and high-cardinality product/actor metrics were
rejected.

## Testing and Delivery

**Decision**: Use current unit projects and API integration tests with WebApplicationFactory, a
test auth handler, real migrations, and MySQL Testcontainers. CI gates build, format, tests,
analysis, vulnerabilities, migrations, dependency direction, and OpenAPI compatibility.

**Rationale**: In-memory stores cannot prove MySQL collation, constraints, transactions, or races.

**Alternatives considered**: No hard coverage percentage is invented; every changed business rule
and acceptance path remains mandatory.

## Resolved Defaults and External Inputs

- Paging: 1-based, default 20, maximum 100; default sort SKU ascending then ID.
- Sort tokens: `sku`, `name`, `price`, `status`, `createdAt`, `modifiedAt`.
- Money: `decimal(18,2)`, one externally defined currency.
- Provisional lengths: SKU 64, name 200, description 2000, actor 200, correlation ID 100.
- Exact auth claims, MySQL/UUID/collation conventions, final lengths, currency, audit governance,
  telemetry/load profile, and route versioning are owner-supplied values listed in plan.md.
