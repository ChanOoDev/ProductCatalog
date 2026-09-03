<!--
Sync Impact Report
- Version change: unversioned placeholder scaffold -> 1.0.0
- Modified principles:
  - Placeholder Principle 1 -> I. Clean Architecture and Repository Alignment
  - Placeholder Principle 2 -> II. Vertical-Slice Application Design
  - Placeholder Principle 3 -> III. Predictable and Compatible HTTP APIs
  - Placeholder Principle 4 -> IV. Durable and Encapsulated Data
  - Placeholder Principle 5 -> V. Security, Verification, and Human Accountability
- Added sections:
  - Technology and Operational Constraints
  - Delivery Workflow and Quality Gates
- Removed sections: none
- Follow-up TODOs:
  - TODO(RATIFICATION_DATE): Confirm the project's original constitution adoption date.
-->

# ProductCatalog Constitution

## Core Principles

### I. Clean Architecture and Repository Alignment
All changes MUST follow the repository's existing architecture, naming, and conventions unless an
approved requirement explicitly changes them. Dependencies MUST point inward: Domain MUST NOT
depend on Application, Infrastructure, or API; Application MUST define use cases and the
abstractions they require; Infrastructure MUST implement persistence and external integrations;
and API MUST own transport concerns and composition. New abstractions or dependencies MUST have a
concrete, current use case and MUST NOT be introduced solely for anticipated reuse. This keeps
business policy independent of frameworks while avoiding needless architectural complexity.

### II. Vertical-Slice Application Design
Features MUST be implemented as cohesive vertical slices that keep each use case's request,
behavior, validation, and tests easy to locate. When the repository already supports CQRS and
MediatR, new use cases MUST follow that pattern; changes MUST NOT introduce a parallel dispatch or
application-flow mechanism without explicit architectural approval. Application use cases MUST
depend only on inward-facing domain types and abstractions, with transport and persistence details
kept outside the slice's business behavior. This limits change scope and makes features
independently understandable and testable.

### III. Predictable and Compatible HTTP APIs
Endpoints MUST use RESTful resource naming, correct HTTP status codes, and ProblemDetails for error
responses. All client-controlled input MUST be validated before it reaches business or persistence
operations, and writable contracts MUST explicitly select assignable fields to prevent mass
assignment. Asynchronous operations MUST accept and propagate cancellation tokens. Collection
endpoints MUST paginate results. Endpoints and schemas MUST be documented through OpenAPI. Existing
clients MUST remain compatible unless a breaking change is explicitly approved and documented.
These rules establish a stable, diagnosable public contract.

### IV. Durable and Encapsulated Data
Persistence MUST use EF Core with MySQL and schema changes MUST be represented by reviewed
migrations. Important invariants MUST be enforced in the domain and, where the database can enforce
them, through suitable constraints. Persisted timestamps MUST use UTC. Updates to mutable data MUST
use optimistic concurrency and define conflict behavior. API endpoints MUST return dedicated
contracts and MUST NOT expose persistence entities directly. Database access MUST use EF Core's
parameterized operations; any exceptional raw SQL MUST remain parameterized and receive explicit
review. These constraints protect data integrity without leaking storage concerns into public or
business contracts.

### V. Security, Verification, and Human Accountability
Protected operations MUST use the repository's existing authentication mechanism and MUST declare
explicit authorization requirements under least privilege. Code MUST NOT log secrets, credentials,
tokens, or sensitive personal data. Error handling MUST not expose stack traces, implementation
details, or sensitive values to clients. Security-sensitive and state-changing operations MUST
produce audit records sufficient to identify the actor, action, target, outcome, and UTC time.
Business rules MUST have unit tests; API and persistence behavior MUST have integration tests. Every
requirement MUST define testable acceptance criteria. Critical or high-severity security findings
MUST block merging. AI-generated code MUST receive human review, and no AI agent may approve or
merge its own changes. These controls make security and correctness demonstrable rather than
implicit.

## Technology and Operational Constraints

- The runtime and target framework MUST be .NET 8 unless an approved architecture decision changes
  the platform.
- EF Core and MySQL are the required persistence stack. Alternative stores or access technologies
  require explicit approval and a documented migration or interoperability rationale.
- Domain logic MUST remain framework-independent. Infrastructure configuration and service
  registration MUST be composed at the API boundary.
- Validation, authorization, cancellation, concurrency handling, error mapping, auditing, and
  pagination MUST be observable in acceptance criteria and covered at the appropriate test level.
- Documentation and migrations MUST be updated in the same change that alters the behavior or
  schema they describe.

## Delivery Workflow and Quality Gates

Work MUST be delivered through small, independently reviewable pull requests. Every pull request
MUST reference its requirements and implementation tasks and MUST explain any intentional
constitution exception. Before merge, new code MUST pass the repository's build, formatting,
automated tests, and static-analysis checks. Reviewers MUST verify architectural dependency
direction, API compatibility, data migration safety, authorization, audit coverage, and the stated
acceptance criteria.

Production deployment requires explicit human approval. Every release MUST include executable
rollback instructions and smoke-test instructions appropriate to the changed behavior. A release
MUST NOT proceed when rollback is infeasible without an explicitly approved recovery plan.

## Governance

This constitution is the authoritative engineering policy for ProductCatalog and supersedes
conflicting informal practices. An amendment MUST be proposed in a reviewed pull request that
states the reason, affected principles, compatibility or migration impact, and intended version
bump. Approval MUST include at least one authorized human maintainer; an AI agent MUST NOT supply
the required approval or merge the amendment.

Constitution versions follow semantic versioning: MAJOR for incompatible governance changes or
principle removals or redefinitions, MINOR for new principles or materially expanded obligations,
and PATCH for non-semantic clarifications. Amendments MUST update the version and Last Amended date.
The Ratified date remains the original adoption date once confirmed.

Every specification and plan review MUST check compliance with this constitution. Every pull
request review MUST verify the applicable quality gates and record approved exceptions. Exceptions
MUST be explicit, time-bounded when practical, identify an accountable owner, and include a
remediation or migration plan. Complexity and new dependencies MUST be justified against a current
requirement.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): Confirm original adoption date | **Last Amended**: 2026-09-03
