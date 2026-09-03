# Quickstart Validation: Product Catalog Management

## Prerequisites

- .NET 8 SDK and Docker-compatible runtime for MySQL integration tests.
- Test organizational authentication configuration with admin and stable actor claims; use a
  test-only handler, never a production user store.
- Approved values from [plan.md](plan.md#assumptions-and-decisions-required-before-implementation).

## Automated Validation

```powershell
dotnet restore ProductCatalog.slnx
dotnet format ProductCatalog.slnx --verify-no-changes
dotnet build ProductCatalog.slnx --configuration Release --no-restore
dotnet test ProductCatalog.slnx --configuration Release --no-build --collect:"XPlat Code Coverage"
```

Integration tests start MySQL, apply real migrations, and isolate state. They prove [spec.md](spec.md),
[data-model.md](data-model.md), and the [OpenAPI contract](contracts/product-catalog.openapi.yaml).

## End-to-End Smoke Scenarios

1. Create valid product: expect `201`, Location, Active, trusted UTC audit fields, ETag.
2. Retrieve it: expect `200`, same identity and ETag.
3. Exercise search case-insensitivity, combined status filter, all sorts, and pagination.
4. Invalid input/query: expect safe `400 application/problem+json`, no mutation.
5. Case-variant duplicate SKU: expect `409`, no change.
6. Retrieve twice; update once, then reuse stale ETag: expect first `200`, second `409`, no overwrite.
7. Deactivate, retrieve/search, repeat deactivation: remains stored; repeat is `200` no-op with the
   same audit/version. Reactivate with current ETag and verify version/audit advance.
8. Exercise every route anonymously and as a non-admin: expect `401` and `403`, no data/change.
9. Verify success/rejection audit events contain safe required metadata and no submitted values.
10. Verify generated OpenAPI contains all operations/responses and no DELETE.

## Performance, Migration, and Rollback

- Under agreed normal load with 100,000 realistic rows, at least 95% of first pages arrive within
  two seconds. Capture query plans; add indexes only from evidence.
- Review target-MySQL SQL and lock/runtime impact; apply from empty state in CI; confirm production
  restore point; migrate, deploy, smoke-test, and monitor errors/latency/conflicts/audit/DB health.
- Roll back binaries first and retain additive tables. If data exists, disable routes or forward-fix.
  Drop tables only with explicit human approval and verified restore.
