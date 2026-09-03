# PR 2 Verification

Date: 2026-09-03

## Scope and model review

- Product persistence only; no controllers, handlers, product store, authentication, or audit-event persistence was introduced.
- Migration creates `Products`, uses `utf8mb4`, applies case-insensitive `utf8mb4_0900_ai_ci` to the uppercase SKU, and creates a unique SKU index.
- Version is an EF concurrency token. Audit fields retain required `ModifiedAtUtc` and `ModifiedBy` names and UTC materialization behavior.
- Runtime connection data comes from `ConnectionStrings:ProductCatalog`; the design-time factory reads `ConnectionStrings__ProductCatalog`. No credential is checked in.
- `Catalog:Currency` is `SGD` and is not persisted on Product.

## Verification results

- Infrastructure build: passed with 0 warnings and 0 errors.
- Domain unit tests: passed, 18/18.
- Persistence integration test assembly: compiled successfully.
- MySQL persistence execution: blocked; all four tests reached fixture setup but the local Docker endpoint `npipe://./pipe/docker_engine` was unavailable. T018 remains unchecked until these tests run successfully against `mysql:8.0.46`.
- A pre-existing ProductCatalog.Api process (PID 69084) locks the default API build output. Alternate build output was used to compile the integration project without terminating that user process.

## Migration and rollback risk

- The initial migration is additive. Its `Down` operation drops `Products`, so applying Down after production data exists is destructive; prefer binary rollback with the table retained.
- MySQL 8.0 reached EOL in April 2026. Production deployment is blocked until MySQL 8.4 LTS compatibility is validated or the EF/Pomelo stack is upgraded.
- Before completing T018, start Docker and rerun the focused persistence tests from a clean database.
