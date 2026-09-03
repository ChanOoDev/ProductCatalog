# Data Model: Product Catalog Management

## Product Aggregate

| Field | Logical type | Rules |
|-------|--------------|-------|
| Id | UUID | Stable, generated on creation, never client-writable |
| Sku | String | Required, trimmed, stored invariant-uppercase, case-insensitively unique, immutable after creation, max 64 |
| Name | String | Required after trim, max 200 |
| Description | Nullable string | Trimmed; whitespace-only becomes null; max 2000 |
| Price | Decimal | Non-negative, max two fractional digits, `decimal(18,2)` |
| Status | ProductStatus | Active or Inactive; creation starts Active |
| CreatedAtUtc / CreatedBy | Timestamp / actor | Required, trusted, immutable |
| ModifiedAtUtc / ModifiedBy | Timestamp / actor | Required; change only on real mutation |
| Version | Unsigned integer | Starts at 1; concurrency token; increments on real mutation |

Operations are Create, UpdateDetails, Activate, and Deactivate. UpdateDetails accepts only name,
description, and price. Same-state lifecycle calls do not change product audit fields or version,
but produce a separate `NoOp` audit event. No delete exists. Audit/concurrency fields are never
directly assigned.

## Product Audit Event

| Field | Logical type | Rules |
|-------|--------------|-------|
| Id | UUID | Generated, immutable |
| ProductId | Nullable UUID | Target when resolved; no cascade delete |
| ActorId | Nullable string | Trusted authenticated identifier; null only for anonymous attempts |
| ActorCategory | String | Authenticated or Anonymous; required |
| Action | Enum/string | Create, Update, Activate, Deactivate, AuthorizationFailure |
| Outcome | Enum/string | Succeeded, NoOp, ValidationRejected, Forbidden, NotFound, DuplicateSku, Conflict, Failed |
| OccurredAtUtc | Timestamp | Required UTC |
| CorrelationId | Nullable string | Safe request correlation; provisional max 100 |
| FailureCategory | Nullable string | Stable category, never raw submitted data |

Successful mutations and audit events commit atomically. Rejected attempts use an independent write
scope or approved enterprise sink. Audit metadata contains no body, token, credential, SKU, name,
or description.

## MySQL Schema and Constraints

### `Products`

- `Id char(36)` primary key.
- Required `Sku` (64) and `Name` (200); optional `Description` (2000).
- `Sku` is stored uppercase and has a unique index under `utf8mb4_0900_ai_ci` collation.
- `Price decimal(18,2) NOT NULL`; non-negative validation remains a domain invariant.
- Required bounded status and check allowing only Active/Inactive.
- Required `datetime(6)` UTC timestamps; `CreatedBy` and `ModifiedBy` are limited to 200 characters.
- `Version bigint NOT NULL`, configured as an EF concurrency token.
- Candidate `(status, sku_normalized, id)` index; add other sort indexes only after query-plan data.
- No global inactive filter or delete path.

Currency is not stored on Product. `Catalog:Currency` is application configuration and is set to
`SGD`; a later API slice may add the configured value to responses.

### `product_audit_events`

- UUID primary key; optional restrictive/no-action foreign key to product.
- Required bounded actor/action/outcome and `datetime(6)` time.
- Index `(product_id, occurred_at_utc)` and `(actor_id, occurred_at_utc)`.

## Concurrency Sequence

1. Read/create/mutation emits an ETag encoding the current numeric version.
2. Client sends `If-Match` for PUT/activate/deactivate; malformed/missing is validation failure.
3. Update predicate includes original version and increments only on real change.
4. Zero affected rows/EF concurrency exception becomes safe stale-version conflict.
5. Duplicate normalized-SKU races become a separate duplicate conflict.

## Lifecycle

```text
Create -> Active
Active --deactivate--> Inactive
Inactive --activate--> Active
Active --activate--> Active       (no-op)
Inactive --deactivate--> Inactive (no-op)
```

Inactive products remain retrievable/searchable and retain identity and SKU reservation.

## Validation Ownership

- API contracts whitelist fields and parse HTTP headers/query tokens.
- Application validators enforce shape, lengths, paging, sort allowlists, price, and version.
- Domain enforces business invariants and lifecycle.
- Database repeats requiredness, length, price, status, uniqueness, relationship, and concurrency.
