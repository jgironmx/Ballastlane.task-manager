# ADR-005 — Testing strategy

## Status

Accepted

## Context

The exercise requires Test-Driven Development across a multi-layer Clean Architecture solution plus an
Angular frontend. Different layers need different kinds of tests, run at different speeds and with
different levels of realism (in particular, database-backed behavior versus pure business logic).

## Decision

Six distinct kinds of tests are used, each with a specific scope:

| Test kind                           | Project                                              | Scope |
|--------------------------------------|-------------------------------------------------------|-------|
| Domain unit tests                    | `Ballastlane.Tasks.Domain.Tests`                       | Entities, value objects, invariants, domain services — no I/O, no framework types. |
| Application unit tests               | `Ballastlane.Tasks.Application.Tests`                  | Use case behavior against fakes/stubs of Application's ports (repositories, current-user, clock, etc.) — no real database, no HTTP. |
| Infrastructure integration tests     | `Ballastlane.Tasks.Infrastructure.IntegrationTests`    | Concrete adapters (EF Core repositories, Identity stores, JWT issuance) against real dependencies. Marked `[Trait("Category", "Integration")]` so they can be filtered out of fast local loops. |
| API integration tests                | `Ballastlane.Tasks.Api.IntegrationTests`               | End-to-end HTTP behavior via `WebApplicationFactory<Program>` — routing, middleware, status codes, response shapes, and (later) authentication/authorization. |
| Architecture tests                   | `Ballastlane.Tasks.ArchitectureTests`                  | Static rules on assembly dependencies (NetArchTest.Rules) — enforce the Clean Architecture dependency direction described in [`docs/architecture/solution-overview.md`](../architecture/solution-overview.md). |
| Angular unit tests                   | `client/ballastlane-tasks-web`                         | Component and service behavior via the Angular CLI's configured test runner. |

**EF Core repository behavior is tested against real SQL Server** (LocalDB locally; a SQL Server
container is the documented CI/portable fallback — see [ADR-002](ADR-002-sql-server-localdb.md)), not
solely against EF Core's InMemory provider. InMemory does not enforce SQL Server's constraints
(`Ballastlane.Tasks.Infrastructure.IntegrationTests` caught a real foreign-key constraint on
`Tasks.OwnerId` this way — InMemory would have silently allowed the orphaned row), concurrency
behavior, or query translation, so relying on it alone would let bugs pass tests and fail in
production. Docker was not available in this development environment, so LocalDB (the
documented fallback, not a silent SQLite/InMemory substitution) is what actually ran.

### Why NetArchTest.Rules over ArchUnitNET

NetArchTest.Rules was chosen for the architecture tests because its fluent API
(`Types.InAssembly(...).Should().NotHaveDependencyOn(...)`) maps directly onto the dependency rules
this solution needs to enforce (namespace/assembly-level "must not reference" checks), it is a small,
focused, actively maintained library, and it keeps the architecture test project's own dependency
surface minimal. ArchUnitNET offers a richer rule DSL (e.g., layer/slice abstractions, more expressive
predicates) that this solution does not currently need.

## Consequences

* Fast feedback loops (Domain, Application, Angular unit tests) can run on every save without touching
  a database or network.
* Integration and API tests are slower and are explicitly categorized (`Category=Integration`), which
  lets local workflows filter them out for a fast loop (`dotnet test --filter "Category!=Integration"`)
  while CI still runs them all, split by platform need (see the root README's Continuous Integration
  section). They also share a single database per test assembly via an xUnit collection fixture
  (`InfrastructureTestGroup`, `ApiTestGroup`) — xUnit parallelizes test *classes* by default, and
  multiple classes each creating/dropping the same LocalDB database concurrently caused real failures
  until they were grouped to run sequentially against one shared fixture instance.
* Architecture tests fail the build the moment a forbidden dependency is introduced (verified by
  temporarily adding a `Domain -> Microsoft.EntityFrameworkCore` reference, and separately for the
  `Infrastructure -> Api` rule, confirming the corresponding test fails each time), rather than
  relying on code review alone to catch layering violations.
* Current pass/fail totals are not repeated here, since they change as the implementation grows — see
  the root [README](../../README.md)'s Test commands section or
  [`docs/ai/05-validation-results.md`](../ai/05-validation-results.md) for the current, verified
  counts.
