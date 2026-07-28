# ADR-002 — SQL Server and LocalDB

## Status

Accepted — implemented.

## Context

The application needs a relational store for users and their tasks, accessed through EF Core from
`Ballastlane.Tasks.Infrastructure`. The stack specifies SQL Server. Local development and CI have
different constraints: a developer's Windows machine typically has SQL Server LocalDB available out
of the box via Visual Studio / the .NET SDK, while CI runners and non-Windows machines would ideally
use a portable, container-based option instead.

## Decision

* **Provider:** SQL Server via `Microsoft.EntityFrameworkCore.SqlServer`.
* **Local development (Windows):** SQL Server LocalDB, referenced via a
  `(localdb)\MSSQLLocalDB` connection string committed in `appsettings.Development.json` (safe to
  commit — Windows Integrated/Trusted authentication, no username or password). The JWT signing key
  is the only secret, supplied separately via user secrets (see [ADR-003](ADR-003-identity-jwt.md)).
* **CI:** the integration tests (`Ballastlane.Tasks.Infrastructure.IntegrationTests`,
  `Ballastlane.Tasks.Api.IntegrationTests`) run against real LocalDB on a dedicated Windows CI job
  (`.github/workflows/ci.yml`'s `backend-integration` job), rather than against SQLite or EF Core
  InMemory. Docker was not available in the development environment this project was built in
  (verified via `docker --version` / `docker info`, both failed), so this is the working, documented
  path today — not a Docker Compose/container-based one.
* **Likely production path:** Azure SQL Database or another managed SQL Server offering, using the same
  EF Core provider and migrations — only the connection string and hosting environment change.

## Portability considerations

* LocalDB is Windows-only and unsuitable for cross-platform local development; CI works around this
  today by running the LocalDB-backed tests on a Windows runner. A container-based option
  (Testcontainers or a SQL Server Docker image) would cover the same need without code changes (same
  provider, same migrations, only the connection string differs) and would also let non-Windows
  contributors run the full suite locally — see [ADR-002 Consequences](#consequences) below.
* Using a single provider (SQL Server) across LocalDB, CI, and managed cloud SQL Server avoids
  SQL-dialect drift between environments (a risk with, e.g., SQLite for dev and SQL Server for
  production).
* EF Core migrations will be provider-specific to SQL Server; this is an accepted tradeoff in exchange
  for environment consistency (see [ADR-005](ADR-005-testing-strategy.md) for how this affects testing
  strategy — integration tests target real SQL Server rather than EF Core InMemory).

## Consequences

* Contributors on Windows can run the API against LocalDB with no additional install; see the root
  [README](../../README.md) for the exact `dotnet ef database update` / `dotnet user-secrets`
  commands.
* CI runs the LocalDB-backed integration tests on a Windows runner (`backend-integration` in
  `.github/workflows/ci.yml`) rather than a container. Containerized integration tests
  (Testcontainers, or a SQL Server Docker image) remain a possible future portability improvement —
  they would remove the Windows-only constraint on that CI job and let non-Windows contributors run
  the full integration suite locally — but are not required for the application to work today.
* One migration (`InitialCreate`) exists, covering both Identity's tables and `Tasks` — see
  [ADR-009](ADR-009-single-dbcontext.md).
