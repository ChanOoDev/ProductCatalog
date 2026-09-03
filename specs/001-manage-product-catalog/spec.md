# Feature Specification: Product Catalog Management

**Feature Branch**: Not created (no `before_specify` hook configured)

**Created**: 2026-09-03

**Status**: Draft

**Input**: User description: "Build Product Catalog Management for internal product administrators. Administrators need to create, view, search, update, activate and deactivate products. Each product has a unique SKU, name, optional description, price, status and audit information. Users must be able to search by SKU or name, filter by status, sort approved fields and retrieve paginated results. A product cannot be physically deleted through the API. It must be deactivated so historical references remain valid. Concurrent updates must not silently overwrite another user's changes."

## Clarifications

### Session 2026-09-03

- Q: How are SKUs compared for uniqueness? → A: Case-insensitively.
- Q: Can a SKU change after product creation? → A: No; SKU is immutable.
- Q: Which products appear when no status filter is supplied? → A: Active products only.
- Q: Can inactive products be requested? → A: Yes, through an explicit inactive-status filter.
- Q: How do repeated activation or deactivation requests behave? → A: They are idempotent.
- Q: What outcome is returned for a duplicate SKU? → A: A conflict response.
- Q: What happens when an update is based on a stale product version? → A: It returns a conflict and preserves stored data.
- Q: What information is captured in product audit fields? → A: UTC timestamp and authenticated user identity.
- Q: Can products be physically deleted? → A: No; physical deletion is unsupported.
- Q: What are the default and maximum page sizes? → A: Default 20 and maximum 100.
- Q: Which fields may clients use for sorting? → A: Only the explicitly approved fields.
- Q: Which authentication mechanism protects product operations? → A: The application's existing mechanism.
- Q: How is product authorization divided? → A: Separate read and write permissions.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and View Products (Priority: P1)

As a product administrator with write permission, I can create a valid product, and as a product
administrator with read permission, I can view its complete details so that it becomes a reliable
catalog record for internal operations.

**Why this priority**: Product records must exist and be retrievable before any other catalog
management behavior provides value.

**Independent Test**: Create a product with valid required data, retrieve it, and verify its SKU,
name, description, price, status, and audit information.

**Acceptance Scenarios**:

1. **Given** an administrator with product write permission and an unused SKU, **When** the administrator supplies a
   valid name and positive price with an optional description, **Then** a product is created as
   active and its creation and latest-modification audit information identify the administrator
   and time of the action.
2. **Given** an existing product, **When** an administrator with product read permission requests it by its stable
   identifier, **Then** the product's current business and audit information is returned.
3. **Given** an existing SKU, **When** an administrator attempts to create another product with the
   same SKU regardless of letter case, **Then** creation is rejected and the existing product is
   unchanged.
4. **Given** missing or invalid product data, **When** creation is attempted, **Then** all detected
   validation failures are reported without creating a product.

---

### User Story 2 - Find Products (Priority: P2)

As a product administrator with read permission, I can search, filter, sort, and page through products so
that I can quickly locate and inspect the records I need to manage.

**Why this priority**: Administrators need predictable discovery once the catalog contains more
than a few products.

**Independent Test**: Seed products with varied SKUs, names, prices, statuses, and dates; then
verify search, status filtering, every approved sort, and navigation across multiple pages.

**Acceptance Scenarios**:

1. **Given** products whose SKU or name contains a search term with different letter case,
   **When** an administrator searches for that term, **Then** all matching products are returned
   without requiring an exact-case match.
2. **Given** active and inactive products, **When** an administrator filters by one status,
   **Then** only products with that status are returned.
3. **Given** a product set, **When** an administrator sorts by SKU, name, price, status, created
   time, or latest-modification time in ascending or descending order, **Then** results follow the
   requested order with stable identifier order resolving ties.
4. **Given** more matching products than fit on one page, **When** an administrator requests a
   valid page, **Then** only that page is returned together with page number, page size, total item
   count, and total page count.
5. **Given** no matching products, **When** search or filtering is performed, **Then** an empty page
   and zero total item count are returned without treating the result as an error.
6. **Given** active and inactive products, **When** an administrator lists products without a status
   filter, **Then** only active products are returned.
7. **Given** inactive products, **When** an administrator with product read permission explicitly
   filters for inactive status, **Then** matching inactive products are returned.

---

### User Story 3 - Update Products Safely (Priority: P3)

As a product administrator with write permission, I can update a product's editable details without silently
overwriting another administrator's newer work.

**Why this priority**: Catalog data changes after creation, and conflicting edits must be visible
to protect accuracy.

**Independent Test**: Retrieve one product into two editing sessions, save the first change, and
verify that the second stale update is rejected while a fresh update succeeds.

**Acceptance Scenarios**:

1. **Given** the current product version, **When** an administrator submits valid changes to name,
   description, or price, **Then** the changes are saved and latest-modification audit
   information and the product version are advanced.
2. **Given** an existing product, **When** an administrator attempts to include or change SKU in a
   general update, **Then** the request is rejected and the product's SKU and other fields remain
   unchanged.
3. **Given** a product changed after an administrator retrieved it, **When** that administrator
   submits an update using the stale version, **Then** the update is rejected as a conflict, the
   newer values remain intact, and the administrator is told to refresh before retrying.
4. **Given** an update containing invalid fields or values, **When** it is submitted, **Then** all
   detected validation failures are reported and no fields are changed.

---

### User Story 4 - Activate and Deactivate Products (Priority: P4)

As a product administrator with write permission, I can deactivate products that are no longer available and
reactivate them later while preserving their identity and history.

**Why this priority**: Lifecycle control is necessary, but it depends on products already being
created and discoverable.

**Independent Test**: Deactivate an active product, confirm it remains retrievable and searchable
as inactive, reactivate it, and confirm physical deletion is unavailable.

**Acceptance Scenarios**:

1. **Given** an active product at its current version, **When** an administrator deactivates it,
   **Then** its status becomes inactive, its latest-modification audit information and version are
   advanced, and the product remains retrievable.
2. **Given** an inactive product at its current version, **When** an administrator activates it,
   **Then** its status becomes active and its latest-modification audit information and version are
   advanced.
3. **Given** a stale product version, **When** activation or deactivation is attempted, **Then** the
   action is rejected as a conflict and the current status is preserved.
4. **Given** any product, **When** an administrator attempts to physically delete it through the
   product service, **Then** deletion is unavailable and the product remains stored.
5. **Given** a product already in the requested status, **When** the same status transition is
   requested with its current version, **Then** the request succeeds without changing product audit
   information or the version, and a separate audit event records a `NoOp` outcome.

### Edge Cases

- Leading and trailing whitespace is removed from SKU, name, and description before validation;
  values containing only whitespace are treated as empty.
- SKU uniqueness comparisons are case-insensitive, including for inactive products; deactivation
  does not free a SKU for reuse.
- A product's SKU is immutable after creation and is never accepted by an update operation.
- Search terms containing only whitespace are treated as no search term.
- Search and status filtering can be combined; both conditions must match.
- Omitting the status filter returns active products only; inactive products are returned only when
  inactive status is explicitly requested by a user with product read permission.
- Omitted sort uses ascending SKU order with stable identifier order resolving ties.
- Unknown sort fields or directions, page numbers below 1, and page sizes outside 1 through 100
  are rejected as validation errors.
- Requesting a valid page beyond the final page returns an empty page with accurate totals.
- Prices with more than two fractional digits, zero values, or negative values are rejected.
- Requests for an unknown product report that no such product exists and disclose no internal
  details.
- Audit information cannot be supplied or overwritten by administrators.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The product service MUST use the application's existing authentication mechanism and
  require separate product read and product write permissions. Retrieve, search, filter, sort, and
  page operations MUST require product read permission. Create, update, activate, and deactivate
  operations MUST require product write permission. Acceptance: each read scenario is repeated
  without read permission and each state-changing scenario without write permission; denied
  attempts reveal no product data and make no changes.
- **FR-002**: The service MUST allow an administrator with product write permission to create a product with a SKU,
  name, optional description, and price. New products MUST initially be active. Acceptance: User
  Story 1, scenarios 1 and 4 pass.
- **FR-003**: Each product MUST have a stable identifier and an immutable SKU unique across active
  and inactive products using a case-insensitive comparison. A duplicate SKU MUST produce a
  conflict outcome. Acceptance: User Story 1 scenario 3, User Story 3 scenario 2, and the SKU edge
  cases pass.
- **FR-004**: The service MUST validate all administrator-controlled fields before changing stored
  data. SKU and name MUST be non-empty after trimming; price MUST be greater than zero and have no
  more than two fractional digits; description MAY be omitted. Acceptance: User Story 1 scenario 4,
  User Story 3 scenario 4, and the input edge cases pass.
- **FR-005**: The service MUST allow an administrator with product read permission to retrieve a product by its stable
  identifier, including inactive products and audit information. Acceptance: User Story 1 scenario
  2 and User Story 4 scenario 1 pass.
- **FR-006**: The service MUST return a paginated product collection and report page number, page
  size, total item count, and total page count. Page numbers start at 1; page size MUST be from 1
  through 100, with a default of 20. Acceptance: User Story 2 scenarios 4 and 5 and the pagination
  edge cases pass.
- **FR-007**: Administrators MUST be able to search products by a partial, case-insensitive match on
  SKU or name and combine the search with a status filter. When status is omitted, only active
  products MUST be returned; inactive products MUST be returned only when inactive status is
  explicitly requested. Acceptance: User Story 2 scenarios 1, 2, 5, 6, and 7 and the combined-filter
  edge cases pass.
- **FR-008**: Administrators MUST be able to sort collection results by SKU, name, price, status,
  creation time, or latest-modification time in ascending or descending order. Results MUST have a
  deterministic order, using stable identifier order to resolve equal sort values. The default
  MUST be ascending SKU order. Acceptance: User Story 2 scenario 3 and the sorting edge cases pass.
- **FR-009**: The service MUST allow an administrator with product write permission to update name,
  description, and price while preventing assignment of SKU, status, audit information, stable
  identifier, or concurrency information through a general update. Acceptance: User Story 3
  scenarios 1, 2, and 4 and the audit edge case pass.
- **FR-010**: Each product MUST record its creation authenticated-user identity and UTC timestamp
  and its latest-modification authenticated-user identity and UTC timestamp. Successful state changes MUST update latest
  modification information; status no-ops MUST not. Acceptance: User Story 1 scenario 1, User Story
  3 scenario 1, and User Story 4 scenarios 1, 2, and 5 pass.
- **FR-011**: Every update and status-change request MUST identify the product version on which the
  administrator acted. A request based on an older version MUST be rejected as a conflict without
  changing the current product. Acceptance: User Story 3 scenario 3 and User Story 4 scenario 3
  pass.
- **FR-012**: The service MUST allow explicit activation and deactivation while preserving the
  product record and stable identifier. Acceptance: User Story 4 scenarios 1, 2, and 5 pass.
- **FR-013**: The service MUST NOT provide any operation that physically deletes a product.
  Acceptance: User Story 4 scenario 4 passes and the published product-management contract contains
  no physical-delete capability.
- **FR-014**: All failed validation, missing-product, authorization, and concurrency outcomes MUST
  be distinguishable and MUST give administrators safe, actionable information without exposing
  internal details. Acceptance: the corresponding negative scenarios in User Stories 1 through 4
  return the expected outcome category and leave stored products unchanged.
- **FR-015**: Every create, successful update, activation, and deactivation MUST produce an audit
  event identifying the actor, action, product, outcome, and UTC time. Rejected state-changing
  attempts MUST record the attempted action and outcome without recording sensitive submitted
  values. An idempotent activation or deactivation MUST record a `NoOp` event without changing the
  product's latest-modification information or version. An unauthenticated attempt MUST use an
  explicit anonymous actor category instead of a fabricated authenticated identity. Acceptance:
  audit records are verified for every state-changing acceptance scenario.

### Key Entities *(include if feature involves data)*

- **Product**: A catalog item with a stable identifier, unique SKU, name, optional description,
  price, active or inactive status, creation audit information, latest-modification audit
  information, and a version used to detect competing changes.
- **Audit Event**: A record of a state-changing or security-sensitive product operation, including
  actor, action, target product, outcome, and occurrence time, while excluding sensitive submitted
  values.
- **Product Page**: A bounded, ordered set of product summaries plus page number, page size, total
  item count, and total page count.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of automated acceptance runs, a valid create request returns a retrievable
  product whose identifier, business fields, status, audit information, and version match the
  create response.
- **SC-002**: At least 95% of searches over 100,000 products complete within two seconds while 25
  concurrent search clients run for 10 minutes after a two-minute warm-up in a production-like
  environment, measured as client-observed elapsed time.
- **SC-003**: Across all automated competing-edit scenarios, 100% of stale updates and stale status
  changes are rejected without loss of the newer changes.
- **SC-004**: Across all lifecycle acceptance tests, 100% of deactivated products remain
  retrievable, retain the same stable identifier and SKU, and can be included in inactive searches.
- **SC-005**: Across all supported product operations, 100% of invalid or unauthorized attempts
  leave product state unchanged and return a safe, distinguishable outcome.
- **SC-006**: For 100% of successful state changes and tested rejected state-changing attempts, an
  audit event contains the required actor, action, target, outcome, and UTC time.
- **SC-007**: In 100% of exact-SKU acceptance tests, one search request returns the matching product
  on the first result page when the product is within the requested status scope.

## Assumptions

- Internal product administrators are authenticated by an existing organizational identity system;
  defining that identity mechanism is outside this feature's scope.
- SKU and name maximum lengths and permitted character sets follow the organization's existing
  catalog policy. If no policy exists, they must be decided before implementation planning.
- Price represents a single catalog currency established outside this feature; currency conversion,
  tax calculation, discounts, inventory, and price history are out of scope.
- Audit actors come from the authenticated administrator identity and cannot be supplied by clients.
- Product history is preserved through the current product record and audit events; a complete
  field-by-field version history and restoration of prior versions are out of scope.
- Bulk import, bulk update, attachments, categories, supplier management, and public catalog access
  are out of scope.
- Search covers SKU and name only and uses partial matching; fuzzy matching and relevance ranking
  are out of scope.
