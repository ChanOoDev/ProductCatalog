# Tasks: Product Catalog Management

**Input**: Approved design artifacts in `/specs/001-manage-product-catalog/`

**Tests**: Unit and integration tests are mandatory. Add the listed tests before their paired
implementation and verify that they fail for the expected missing behavior.

**Traceability**: Every task cites functional requirements (`FR-nnn`) and acceptance scenarios
(`USn-ASn`) from [spec.md](spec.md). Edge cases are cited as `EC` and measurable outcomes as `SC-nnn`.

**Plan precedence**: The clarified specification supersedes the earlier plan wherever they conflict.
In particular, SKU is immutable, listings default to Active, and read/write permissions are separate.

## Phase 1 — PR 1: Shared Domain Foundations and Product Aggregate

**Goal**: Establish the framework-independent Product aggregate, immutable case-insensitive SKU,
validation, lifecycle behavior, audit fields, and optimistic version semantics.

**Independent test**: Domain tests prove creation, validation, immutable SKU, detail updates,
idempotent lifecycle transitions, UTC audit identity, and version advancement without infrastructure.

- [ ] T001 [P] Add ProductStatus and domain error types in src/ProductCatalog.Domain/Products/ProductStatus.cs and src/ProductCatalog.Domain/Products/ProductDomainException.cs (FR-003, FR-004, FR-012, FR-014; US1-AS3, US1-AS4, US4-AS1)
- [ ] T002 [P] Add Create Product aggregate unit tests covering normalization, case-insensitive SKU value equality, required fields, positive two-decimal price, Active initial state, authenticated audit identity, UTC timestamps, and version 1 in tests/ProductCatalog.Domain.UnitTests/Products/ProductCreationTests.cs (FR-002, FR-003, FR-004, FR-010; US1-AS1, US1-AS3, US1-AS4)
- [ ] T003 [P] Write Product update unit tests proving SKU cannot be supplied or changed, only name/description/price change, invalid changes are atomic, and real changes advance audit/version in tests/ProductCatalog.Domain.UnitTests/Products/ProductUpdateTests.cs (FR-003, FR-004, FR-009, FR-010, FR-011; US3-AS1, US3-AS2, US3-AS4)
- [ ] T004 [P] Write Product lifecycle unit tests proving activation/deactivation transitions and same-state idempotency without audit/version changes in tests/ProductCatalog.Domain.UnitTests/Products/ProductLifecycleTests.cs (FR-010, FR-011, FR-012, FR-013; US4-AS1, US4-AS2, US4-AS3, US4-AS5)
- [ ] T005 Implement Product aggregate creation, immutable SKU normalization, validated detail updates, lifecycle transitions, trusted audit fields, and numeric version in src/ProductCatalog.Domain/Products/Product.cs (FR-002, FR-003, FR-004, FR-009, FR-010, FR-011, FR-012, FR-013; US1-AS1, US1-AS3, US1-AS4, US3-AS1, US3-AS2, US3-AS4, US4-AS1, US4-AS2, US4-AS3, US4-AS5)
- [ ] T006 Record approved field limits, SKU characters, MySQL version, UUID/collation, currency, existing authentication scheme, ProductRead/ProductWrite claims, actor claim, and audit governance, then run the PR 1 build/domain tests in specs/001-manage-product-catalog/pr-1-verification.md (FR-001, FR-002, FR-003, FR-004, FR-009, FR-010, FR-011, FR-012, FR-013, FR-015; US1-AS1, US3-AS1, US4-AS1)

**Checkpoint**: PR 1 is independently buildable, domain-tested, and reviewable.

---

## Phase 2 — PR 2: EF Core DbContext, Entity Configuration, and Migration

**Goal**: Persist products and audit events with MySQL constraints, deterministic SKU uniqueness,
UTC fields, concurrency tokens, and no deletion path.

**Independent test**: A clean MySQL container applies the migration and proves constraints,
case-insensitive uniqueness including inactive records, UTC round-trip, concurrency, and restrictive
audit relationships.

- [ ] T007 [P] Define product persistence operations, projections, page result, and typed conflict outcomes in src/ProductCatalog.Application/Abstractions/IProductStore.cs and src/ProductCatalog.Application/Products/ProductModels.cs (FR-003, FR-005, FR-006, FR-007, FR-008, FR-011, FR-014; US1-AS2, US2-AS1, US2-AS4, US3-AS3)
- [ ] T008 [P] Define current-user, UTC clock, and durable audit abstractions in src/ProductCatalog.Application/Abstractions/ICurrentUser.cs, src/ProductCatalog.Application/Abstractions/IClock.cs, and src/ProductCatalog.Application/Abstractions/IAuditWriter.cs (FR-010, FR-015; US1-AS1, US3-AS1, US4-AS1)
- [ ] T009 [P] Create ProductCatalogDbContext, Product and audit configurations, approved constraints/indexes, and durable ProductAuditEvent model in src/ProductCatalog.Infrastructure/Persistence/ProductCatalogDbContext.cs, src/ProductCatalog.Infrastructure/Persistence/Configurations/ProductConfiguration.cs, src/ProductCatalog.Infrastructure/Persistence/Configurations/ProductAuditEventConfiguration.cs, and src/ProductCatalog.Infrastructure/Auditing/ProductAuditEvent.cs (FR-003, FR-004, FR-005, FR-010, FR-011, FR-012, FR-013, FR-015; US1-AS1, US1-AS3, US3-AS3, US4-AS1, US4-AS4)
- [ ] T010 [P] Implement separate ProductRead/ProductWrite policies and trusted current-user resolution over the approved existing authentication scheme in src/ProductCatalog.Api/Authorization/ProductAuthorization.cs and src/ProductCatalog.Api/Authorization/HttpCurrentUser.cs (FR-001, FR-010, FR-015; US1-AS1, US1-AS2, US2-AS1, US3-AS1, US4-AS1)
- [ ] T011 [P] Implement baseline RFC 7807 mapping for validation, authentication, authorization, not-found, conflict, and unexpected outcomes in src/ProductCatalog.Api/Errors/ApiExceptionHandler.cs (FR-001, FR-003, FR-004, FR-011, FR-014; US1-AS3, US1-AS4, US3-AS3, US4-AS3)
- [ ] T012 Implement EF product reads/writes, deterministic projections, atomic mutation/audit transactions, duplicate-SKU translation, and concurrency translation in src/ProductCatalog.Infrastructure/Persistence/EfProductStore.cs (FR-003, FR-005, FR-006, FR-007, FR-008, FR-011, FR-014, FR-015; US1-AS2, US1-AS3, US2-AS1, US3-AS3)
- [ ] T013 Implement independent rejected-attempt audit writes without submitted values in src/ProductCatalog.Infrastructure/Auditing/EfAuditWriter.cs (FR-015; US1-AS3, US1-AS4, US3-AS2, US3-AS3, US3-AS4, US4-AS3)
- [ ] T014 Register MediatR, validators, DbContext, stores, clock, audit, existing authentication, policies, ProblemDetails, and test-host entry point in src/ProductCatalog.Application/DependencyInjection.cs, src/ProductCatalog.Infrastructure/DependencyInjection.cs, and src/ProductCatalog.Api/Program.cs (FR-001, FR-004, FR-005, FR-010, FR-014, FR-015; US1-AS1, US1-AS2, US1-AS4)
- [ ] T015 Create reviewed initial MySQL schema migration for products and product_audit_events in src/ProductCatalog.Infrastructure/Persistence/Migrations/ (FR-003, FR-004, FR-010, FR-011, FR-013, FR-015; US1-AS3, US3-AS3, US4-AS4)
- [ ] T016 [P] Build the reusable MySQL Testcontainer, database reset, WebApplicationFactory, and test authentication principals in tests/ProductCatalog.Api.IntegrationTests/Infrastructure/MySqlFixture.cs, tests/ProductCatalog.Api.IntegrationTests/Infrastructure/ProductCatalogApiFactory.cs, and tests/ProductCatalog.Api.IntegrationTests/Infrastructure/TestAuthenticationHandler.cs (FR-001, FR-003, FR-004, FR-011, FR-015; US1-AS1, US1-AS2, US1-AS3, US3-AS3)
- [ ] T017 Add clean-migration, constraints, inactive-SKU reservation, concurrency-token, UTC, audit relationship, fail-closed authorization, and baseline ProblemDetails integration tests in tests/ProductCatalog.Api.IntegrationTests/Infrastructure/PersistenceTests.cs and tests/ProductCatalog.Api.IntegrationTests/Infrastructure/ApiFoundationTests.cs (FR-001, FR-003, FR-004, FR-010, FR-011, FR-013, FR-014, FR-015; US1-AS1, US1-AS3, US1-AS4, US3-AS3, US4-AS4)
- [ ] T018 Run the PR 2 build, domain tests, and MySQL persistence tests and record migration/rollback evidence in specs/001-manage-product-catalog/pr-2-verification.md (FR-003, FR-004, FR-010, FR-011, FR-013, FR-015; US1-AS3, US3-AS3, US4-AS4)

**Checkpoint**: PR 2 is independently buildable and validates the real MySQL schema from empty state.

---

## Phase 3 — PR 3: Create Product and Get Product by ID Vertical Slices

**Goal**: Deliver the first usable vertical slices with explicit contracts, validation, cancellation,
duplicate conflict, trusted audit data, Location, and ETag.

**Independent test**: An authorized test principal creates a valid product and retrieves it by
Location; invalid and duplicate creates fail without mutation, and unknown IDs return not found.

- [ ] T019 [P] [US1] Write CreateProduct handler and validator unit tests for valid creation, trimming, validation aggregation, duplicate conflict, cancellation, authenticated actor, UTC clock, and safe audit in tests/ProductCatalog.Application.UnitTests/Products/CreateProductTests.cs (FR-002, FR-003, FR-004, FR-010, FR-014, FR-015; US1-AS1, US1-AS3, US1-AS4)
- [ ] T020 [P] [US1] Write GetProductById handler unit tests for existing, inactive, unknown, and cancelled reads in tests/ProductCatalog.Application.UnitTests/Products/GetProductByIdTests.cs (FR-005, FR-014; US1-AS2, US4-AS1)
- [ ] T021 [P] [US1] Implement CreateProduct command, FluentValidation validator, handler, duplicate outcome, audit, and cancellation in src/ProductCatalog.Application/Products/Create/CreateProduct.cs (FR-002, FR-003, FR-004, FR-010, FR-014, FR-015; US1-AS1, US1-AS3, US1-AS4)
- [ ] T022 [P] [US1] Implement GetProductById query, projection result, not-found outcome, and cancellation in src/ProductCatalog.Application/Products/GetById/GetProductById.cs (FR-005, FR-014; US1-AS2, US4-AS1)
- [ ] T023 [P] [US1] Define create request and product response DTOs with SKU accepted only on create and no client-writable audit/version fields in src/ProductCatalog.Api/Contracts/Products/CreateProductRequest.cs and src/ProductCatalog.Api/Contracts/Products/ProductResponse.cs (FR-002, FR-003, FR-009, FR-010; US1-AS1, US3-AS2)
- [ ] T024 [P] [US1] Implement strong opaque ETag encoding for response versions in src/ProductCatalog.Api/Contracts/Products/ProductEtag.cs (FR-011; US3-AS3, US4-AS3)
- [ ] T025 [US1] Implement POST /api/products and GET /api/products/{id} with ProductWrite/ProductRead, MediatR, cancellation, Location, ETag, ProblemDetails, and OpenAPI metadata in src/ProductCatalog.Api/Controllers/ProductsController.cs and src/ProductCatalog.Api/OpenApi/ProductCatalogOpenApi.cs (FR-001, FR-002, FR-003, FR-004, FR-005, FR-010, FR-014, FR-015; US1-AS1, US1-AS2, US1-AS3, US1-AS4)
- [ ] T026 [US1] Add create/get HTTP and OpenAPI tests for permissions, status codes, contracts, Location, ETag, inactive retrieval, validation ProblemDetails, duplicate conflict, unknown ID, cancellation, and persisted audit in tests/ProductCatalog.Api.IntegrationTests/Products/CreateAndGetProductTests.cs and tests/ProductCatalog.Api.IntegrationTests/OpenApi/CreateAndGetContractTests.cs (FR-001, FR-002, FR-003, FR-004, FR-005, FR-010, FR-014, FR-015; US1-AS1, US1-AS2, US1-AS3, US1-AS4)
- [ ] T027 [US1] Run the PR 3 build and Create/Get unit and integration tests and record evidence in specs/001-manage-product-catalog/pr-3-verification.md (FR-002, FR-003, FR-004, FR-005, FR-010, FR-014, FR-015; US1-AS1, US1-AS2, US1-AS3, US1-AS4)

**Checkpoint**: PR 3 is a buildable, testable Create/Get MVP vertical slice.

---

## Phase 4 — PR 4: Search, Filtering, Sorting, and Pagination Vertical Slice

**Goal**: Return Active products by default and allow explicit inactive filtering, partial
case-insensitive SKU/name search, approved stable sorts, and bounded pages.

**Independent test**: Seed varied products and verify defaults, explicit inactive filtering,
combined search/filter, every allowed sort/direction, tie-breaking, invalid inputs, empty results,
and beyond-last pages.

- [ ] T028 [P] [US2] Write SearchProducts validator, active-default, approved-sort, stable tie-break, and pagination unit tests in tests/ProductCatalog.Application.UnitTests/Products/SearchProductsTests.cs (FR-006, FR-007, FR-008, FR-014; US2-AS1, US2-AS2, US2-AS3, US2-AS4, US2-AS5, US2-AS6, US2-AS7)
- [ ] T029 [P] [US2] Implement SearchProducts query, status default, sort allowlist, paging defaults/limits, page metadata, and cancellation in src/ProductCatalog.Application/Products/Search/SearchProducts.cs (FR-006, FR-007, FR-008, FR-014; US2-AS1, US2-AS2, US2-AS3, US2-AS4, US2-AS5, US2-AS6, US2-AS7)
- [ ] T030 [P] [US2] Define search query-binding and product page response DTOs in src/ProductCatalog.Api/Contracts/Products/SearchProductsRequest.cs and src/ProductCatalog.Api/Contracts/Products/ProductPageResponse.cs (FR-006, FR-007, FR-008; US2-AS3, US2-AS4, US2-AS6)
- [ ] T031 [US2] Implement case-insensitive SKU/name search, explicit status filtering, active default, stable allowed sorting, count, page projection, and cancellation in src/ProductCatalog.Infrastructure/Persistence/EfProductStore.cs (FR-005, FR-006, FR-007, FR-008; US2-AS1, US2-AS2, US2-AS3, US2-AS4, US2-AS5, US2-AS6, US2-AS7)
- [ ] T032 [US2] Add ProductRead-protected GET /api/products with cancellation, validation ProblemDetails, page contract, Active default, and OpenAPI metadata in src/ProductCatalog.Api/Controllers/ProductsController.cs and src/ProductCatalog.Api/OpenApi/ProductCatalogOpenApi.cs (FR-001, FR-006, FR-007, FR-008, FR-014; US2-AS1, US2-AS2, US2-AS3, US2-AS4, US2-AS5, US2-AS6, US2-AS7)
- [ ] T033 [US2] Add real-MySQL search and OpenAPI tests for ProductRead, Active default, explicit inactive, search/filter combinations, approved sorts, tie-breaks, page bounds, totals, validation ProblemDetails, and empty results in tests/ProductCatalog.Api.IntegrationTests/Products/SearchProductsTests.cs and tests/ProductCatalog.Api.IntegrationTests/OpenApi/SearchContractTests.cs (FR-001, FR-006, FR-007, FR-008, FR-014; US2-AS1, US2-AS2, US2-AS3, US2-AS4, US2-AS5, US2-AS6, US2-AS7)
- [ ] T034 [US2] Run the PR 4 build and Search unit/integration tests and record evidence in specs/001-manage-product-catalog/pr-4-verification.md (FR-006, FR-007, FR-008, FR-014; US2-AS1, US2-AS2, US2-AS3, US2-AS4, US2-AS5, US2-AS6, US2-AS7)

**Checkpoint**: PR 4 is independently reviewable and preserves PR 3 contracts.

---

## Phase 5 — PR 5: Update Product with Optimistic Concurrency

**Goal**: Update name, description, and price only; reject SKU assignment, invalid input, duplicate
write fields, and stale versions without overwriting current data.

**Independent test**: Two reads produce one current version; the first update succeeds and advances
audit/ETag, while the stale second update returns conflict and leaves newer data intact.

- [ ] T035 [P] [US3] Write UpdateProduct handler/validator unit tests for immutable SKU, allowed fields, atomic validation, cancellation, not found, audit, and concurrency conflict in tests/ProductCatalog.Application.UnitTests/Products/UpdateProductTests.cs (FR-003, FR-004, FR-009, FR-010, FR-011, FR-014, FR-015; US3-AS1, US3-AS2, US3-AS3, US3-AS4)
- [ ] T036 [P] [US3] Implement required If-Match parsing and malformed-token validation in src/ProductCatalog.Api/Contracts/Products/IfMatchHeader.cs (FR-011, FR-014; US3-AS3)
- [ ] T037 [P] [US3] Define update request DTO containing only name, description, and price with unknown-field rejection in src/ProductCatalog.Api/Contracts/Products/UpdateProductRequest.cs (FR-003, FR-004, FR-009; US3-AS1, US3-AS2, US3-AS4)
- [ ] T038 [P] [US3] Implement UpdateProduct command, validator, handler, trusted audit update, cancellation, not-found, and stale-version outcomes in src/ProductCatalog.Application/Products/Update/UpdateProduct.cs (FR-003, FR-004, FR-009, FR-010, FR-011, FR-014, FR-015; US3-AS1, US3-AS2, US3-AS3, US3-AS4)
- [ ] T039 [US3] Add ProductWrite-protected PUT /api/products/{id} with required If-Match, immutable-SKU contract, cancellation, conflict ProblemDetails, new ETag, and OpenAPI metadata in src/ProductCatalog.Api/Controllers/ProductsController.cs and src/ProductCatalog.Api/OpenApi/ProductCatalogOpenApi.cs (FR-001, FR-003, FR-004, FR-009, FR-010, FR-011, FR-014; US3-AS1, US3-AS2, US3-AS3, US3-AS4)
- [ ] T040 [US3] Add update and OpenAPI tests for ProductWrite, allowed fields, SKU rejection, validation atomicity, malformed/stale If-Match, competing updates, new ETag, conflict ProblemDetails, and durable audit in tests/ProductCatalog.Api.IntegrationTests/Products/UpdateProductTests.cs and tests/ProductCatalog.Api.IntegrationTests/OpenApi/UpdateContractTests.cs (FR-001, FR-003, FR-004, FR-009, FR-010, FR-011, FR-014, FR-015; US3-AS1, US3-AS2, US3-AS3, US3-AS4; SC-003)
- [ ] T041 [US3] Run the PR 5 build and Update unit/integration tests and record concurrency evidence in specs/001-manage-product-catalog/pr-5-verification.md (FR-003, FR-004, FR-009, FR-010, FR-011, FR-014, FR-015; US3-AS1, US3-AS2, US3-AS3, US3-AS4; SC-003)

**Checkpoint**: PR 5 proves stale updates never overwrite stored data.

---

## Phase 6 — PR 6: Activation, Authorization, and Authenticated Auditing

**Goal**: Add idempotent lifecycle slices, separate product read/write permissions through the
application's existing authentication mechanism, and trusted durable audit identity.

**Independent test**: Read-only and write-only principals receive correct access; lifecycle actions
are idempotent and concurrency-safe; inactive products remain retrievable/filterable; every tested
state-changing success/rejection has safe authenticated audit data; no deletion exists.

- [ ] T042 [P] [US4] Write ActivateProduct and DeactivateProduct handler tests for transitions, current-state no-op, stale version, missing product, cancellation, audit identity, UTC time, and no delete behavior in tests/ProductCatalog.Application.UnitTests/Products/ProductLifecycleTests.cs (FR-010, FR-011, FR-012, FR-013, FR-014, FR-015; US4-AS1, US4-AS2, US4-AS3, US4-AS4, US4-AS5)
- [ ] T043 [P] [US4] Implement ActivateProduct command, validator, idempotent handler, concurrency, cancellation, and audit outcomes in src/ProductCatalog.Application/Products/Activate/ActivateProduct.cs (FR-010, FR-011, FR-012, FR-014, FR-015; US4-AS2, US4-AS3, US4-AS5)
- [ ] T044 [P] [US4] Implement DeactivateProduct command, validator, idempotent handler, concurrency, cancellation, and audit outcomes in src/ProductCatalog.Application/Products/Deactivate/DeactivateProduct.cs (FR-010, FR-011, FR-012, FR-013, FR-014, FR-015; US4-AS1, US4-AS3, US4-AS4, US4-AS5)
- [ ] T045 [P] [US4] Extend ProductWrite policy coverage tests to lifecycle routes and explicit no-DELETE behavior in tests/ProductCatalog.Api.IntegrationTests/Authorization/ProductLifecycleAuthorizationTests.cs (FR-001, FR-013; US4-AS1, US4-AS2, US4-AS4)
- [ ] T046 [P] [US4] Implement anonymous actor category and authenticated actor identity mapping for lifecycle audit events in src/ProductCatalog.Api/Authorization/HttpCurrentUser.cs and src/ProductCatalog.Infrastructure/Auditing/EfAuditWriter.cs (FR-010, FR-015; US4-AS1, US4-AS3, US4-AS5)
- [ ] T047 [P] [US4] Add anonymous, read-only, write-only, combined-permission, and missing-actor lifecycle principals to tests/ProductCatalog.Api.IntegrationTests/Infrastructure/TestAuthenticationHandler.cs (FR-001, FR-010, FR-015; US4-AS1, US4-AS2, US4-AS3)
- [ ] T048 [US4] Extend authorization-boundary audit capture for lifecycle success, NoOp, conflict, forbidden, and anonymous outcomes in src/ProductCatalog.Api/Program.cs (FR-001, FR-010, FR-015; US4-AS1, US4-AS2, US4-AS3, US4-AS5)
- [ ] T049 [US4] Add ProductWrite-protected POST activation/deactivation routes with If-Match, cancellation, idempotent NoOp audit, conflict ProblemDetails, ETags, no DELETE, and OpenAPI metadata in src/ProductCatalog.Api/Controllers/ProductsController.cs and src/ProductCatalog.Api/OpenApi/ProductCatalogOpenApi.cs (FR-001, FR-010, FR-011, FR-012, FR-013, FR-014, FR-015; US4-AS1, US4-AS2, US4-AS3, US4-AS4, US4-AS5)
- [ ] T050 [US4] Add lifecycle and OpenAPI tests for transitions, NoOp audit, stale conflicts, inactive retrieval/search, identity/SKU preservation, permissions, ProblemDetails, and absence of DELETE in tests/ProductCatalog.Api.IntegrationTests/Products/ProductLifecycleTests.cs and tests/ProductCatalog.Api.IntegrationTests/OpenApi/LifecycleContractTests.cs (FR-001, FR-010, FR-011, FR-012, FR-013, FR-014, FR-015; US4-AS1, US4-AS2, US4-AS3, US4-AS4, US4-AS5; SC-004, SC-006)
- [ ] T051 Add authorization integration tests across every route for anonymous, missing permission, read-only, write-only, combined permission, and missing actor identity in tests/ProductCatalog.Api.IntegrationTests/Authorization/ProductAuthorizationTests.cs (FR-001, FR-010, FR-014, FR-015; US1-AS1, US1-AS2, US2-AS1, US3-AS1, US4-AS1; SC-005)
- [ ] T052 Add durable audit integration tests for successful/rejected changes, UTC authenticated identity, atomic success events, independent failure events, and sensitive-value exclusion in tests/ProductCatalog.Api.IntegrationTests/Auditing/ProductAuditTests.cs (FR-010, FR-015; US1-AS1, US1-AS3, US1-AS4, US3-AS1, US3-AS3, US4-AS1, US4-AS3; SC-006)
- [ ] T053 [US4] Run the PR 6 build and lifecycle/authorization/audit tests and record evidence in specs/001-manage-product-catalog/pr-6-verification.md (FR-001, FR-010, FR-011, FR-012, FR-013, FR-014, FR-015; US4-AS1, US4-AS2, US4-AS3, US4-AS4, US4-AS5)

**Checkpoint**: PR 6 is buildable, security-tested, audit-tested, and preserves all prior slices.

---

## Phase 7 — PR 7: ProblemDetails, Logging, Observability, OpenAPI, and Verification

**Goal**: Complete safe error mapping, redacted operational telemetry, published contracts,
performance/migration evidence, CI gates, and human-reviewed release documentation.

**Independent test**: Full automated and smoke suites verify all functional requirements and
scenarios, OpenAPI matches behavior with no DELETE, errors disclose no internals, telemetry has no
sensitive/high-cardinality data, and release/rollback instructions are executable.

- [ ] T054 [P] Add full-regression ProblemDetails tests across validation, authentication, authorization, not found, duplicate SKU, stale concurrency, cancellation, and unexpected errors in tests/ProductCatalog.Api.IntegrationTests/Errors/ProblemDetailsTests.cs (FR-001, FR-003, FR-004, FR-011, FR-014; US1-AS3, US1-AS4, US3-AS3, US3-AS4, US4-AS3; SC-005)
- [ ] T055 [P] Harden the shared ProblemDetails mapper with stable types/codes, trace IDs, redaction, and exhaustive outcome coverage in src/ProductCatalog.Api/Errors/ApiExceptionHandler.cs (FR-001, FR-003, FR-004, FR-011, FR-014; US1-AS3, US1-AS4, US3-AS3, US3-AS4, US4-AS3)
- [ ] T056 [P] Add structured redacted Serilog request/product-operation logging in src/ProductCatalog.Api/Observability/ProductCatalogLogging.cs (FR-014, FR-015; US1-AS1, US3-AS3, US4-AS3)
- [ ] T057 [P] Add low-cardinality request, command outcome, validation, duplicate, concurrency, and search meters in src/ProductCatalog.Api/Observability/ProductCatalogMetrics.cs (FR-003, FR-006, FR-011, FR-014; US1-AS3, US2-AS4, US3-AS3)
- [ ] T058 Wire ProblemDetails, Serilog, trace enrichment, redaction, and metrics without sensitive labels in src/ProductCatalog.Api/Program.cs (FR-014, FR-015; US1-AS4, US3-AS3, US4-AS3; SC-005, SC-006)
- [ ] T059 [P] Update the checked-in OpenAPI contract so update input excludes immutable SKU and documents the Active listing default, separate ProductRead/ProductWrite security, ETags, ProblemDetails, pagination, sorts, and no DELETE in specs/001-manage-product-catalog/contracts/product-catalog.openapi.yaml (FR-001, FR-003, FR-006, FR-007, FR-008, FR-009, FR-011, FR-013, FR-014; US1-AS2, US2-AS3, US2-AS6, US2-AS7, US3-AS2, US3-AS3, US4-AS4)
- [ ] T060 Implement runtime OpenAPI metadata matching the approved checked-in contract in src/ProductCatalog.Api/OpenApi/ProductCatalogOpenApi.cs (FR-001, FR-002, FR-005, FR-006, FR-007, FR-008, FR-009, FR-011, FR-012, FR-013, FR-014; US1-AS1, US1-AS2, US2-AS4, US3-AS3, US4-AS4)
- [ ] T061 Add generated-contract snapshot, security, immutable-SKU, defaults, ETag, response, and no-DELETE compatibility tests in tests/ProductCatalog.Api.IntegrationTests/OpenApi/ProductCatalogContractTests.cs (FR-001, FR-003, FR-006, FR-007, FR-008, FR-009, FR-011, FR-013, FR-014; US2-AS6, US2-AS7, US3-AS2, US4-AS4)
- [ ] T062 [P] Add logging redaction and metric-cardinality tests in tests/ProductCatalog.Api.IntegrationTests/Observability/ProductCatalogObservabilityTests.cs (FR-014, FR-015; US1-AS4, US3-AS3, US4-AS3; SC-005, SC-006)
- [ ] T063 Execute the 100,000-product normal-load search test and record p95 timing and query plans in specs/001-manage-product-catalog/performance-results.md (FR-006, FR-007, FR-008; US2-AS1, US2-AS3, US2-AS4; SC-002)
- [ ] T064 Update executable setup, migration, smoke-test, binary-first rollback, route-disable/forward-fix recovery, and monitoring guidance in specs/001-manage-product-catalog/quickstart.md (FR-001 through FR-015; US1-AS1 through US4-AS5; SC-001 through SC-007)
- [ ] T065 Add CI gates for restore, Release warnings-as-errors build, formatting, all tests/coverage, static analysis, dependency vulnerabilities, clean migration, architecture direction, and OpenAPI diff in .github/workflows/product-catalog-ci.yml (FR-001 through FR-015; US1-AS1 through US4-AS5; SC-003, SC-004, SC-005, SC-006)
- [ ] T066 Run the complete quickstart, automated suite, contract comparison, migration/rollback rehearsal, and smoke tests and record results in specs/001-manage-product-catalog/pr-7-verification.md (FR-001 through FR-015; US1-AS1 through US4-AS5; SC-001 through SC-007)
- [ ] T067 Record human security and AI-output review, requirement/task links, migration approval, rollback evidence, and production approval owner in specs/001-manage-product-catalog/release-readiness.md (FR-001 through FR-015; US1-AS1 through US4-AS5; SC-001 through SC-007)

**Checkpoint**: PR 7 is fully buildable/testable and ready for explicit human release approval.

---

## Dependencies and Execution Order

```text
PR 1 Domain
  -> PR 2 Persistence/Migration
    -> PR 3 Create + Get (MVP)
      -> PR 4 Search
        -> PR 5 Update/Concurrency
          -> PR 6 Lifecycle/Auth/Audit
            -> PR 7 Errors/Observability/OpenAPI/Verification
```

- PRs follow the requested sequence because each later slice reuses the prior tested contract and
  persistence foundation.
- Within a PR, test tasks precede their paired implementation even when marked `[P]` for authoring.
- A `[P]` task may be authored concurrently only when its referenced prerequisite types/contracts
  are stable and it does not modify the same file as another active task.
- Each PR checkpoint requires its own build and focused tests; later failures do not defer earlier
  verification.

## Parallel Opportunities

- **PR 1**: T001-T004 use separate source/test files; T005 follows their agreed contracts.
- **PR 2**: T007-T011 and T016 can be authored in parallel; T012-T015 integrate them sequentially.
- **PR 3**: Create tests/slice, Get tests/slice, DTOs, and ETag helper can be authored in parallel.
- **PR 4**: Application tests/query and API DTOs can be authored before store/controller integration.
- **PR 5**: Handler tests, header parser, update DTO, and handler can be authored in separate files.
- **PR 6**: Lifecycle slices, policy, actor adapter, and auth test handler are separate workstreams.
- **PR 7**: Error, logging, metrics, contract, observability-test, and documentation files can proceed
  in parallel before final composition and verification.

## Incremental Delivery Strategy

1. Merge PR 1 only after domain tests pass.
2. Merge PR 2 only after clean MySQL migration and constraint tests pass.
3. Treat PR 3 as the smallest deployable MVP; stop and demonstrate Create/Get independently.
4. Add PRs 4-6 one tested vertical slice at a time without changing earlier public contracts.
5. Merge PR 7 only after complete verification and human review; production still requires explicit
   human approval and documented rollback/smoke instructions.

## Notes

- SKU appears in create input and responses but never in update input.
- No task introduces ASP.NET Core Identity, a user store, a DELETE route, or a second framework stack.
- Every task is small enough for focused review and cites its requirement/scenario evidence.
- No AI agent may approve or merge its own changes.
