# Ballastlane.Tasks

.NET - Task-management application

## 1. Overview

Ballastlane.task-manager is a full-stack task-management application developed as a technical engineering
exercise. It demonstrates Clean Architecture, ASP.NET Core Identity, JWT authentication, SQL Server
persistence, Angular integration, automated testing, and a transparent AI-assisted engineering
workflow. It was delivered through a sequence of constrained implementation and hardening phases,
with an ASP.NET Core 10 backend and an Angular 22 frontend, both fully implemented and wired
together.

**User story:** *As a registered user, I want to create, organize, and track the status of my own
tasks, so that I have a single reliable place to see what I need to do — without ever being able to
see or affect anyone else's tasks.*

## 2. Quick start

The fastest path from a clean clone to a working app in your browser. See §8 for exact prerequisite
versions and §11–§13 for what each step does and why.

```bash
# 1. Backend — set the JWT signing key once, then run
cd src/Ballastlane.Tasks.Api
dotnet user-secrets set "Jwt:SigningKey" "local-dev-only-signing-key-not-for-prod-32chars-min"
cd ../..
dotnet run --project src/Ballastlane.Tasks.Api
# -> applies pending EF Core migrations and seeds demo data automatically (Development only)
# -> API listening at http://localhost:5276 / https://localhost:7111
```

```bash
# 2. Frontend — in a second terminal
cd client/ballastlane-tasks-web
npm ci
npm start
# -> Angular dev server at http://localhost:4200
```

Open `http://localhost:4200`, then either register a new account or log in with the seeded demo
account (§14): `demo@ballastlane.local` / `Demo1234!`.

## 3. Features

* Registration and login, with a session that survives a browser refresh.
* Full task CRUD: create, view, edit, delete.
* Status changes (`Pending` / `InProgress` / `Completed`), search, and status filtering.
* Every task is scoped to its owner — no user can see or modify another user's tasks, and the API
  reports another user's task as `404`, not `403` (see [ADR-008](docs/decisions/ADR-008-cross-user-404.md)),
  so its mere existence isn't leaked either.

## 4. Current status — fully implemented

The backend and the Angular SPA are implemented, wired together, and hardened. A user can register,
log in, stay signed in across a page refresh, view/create/edit/delete their own tasks, change task
status, and log out — all through the browser, against the real backend.

Backend:

* `TaskItem` domain model with enforced invariants (see [ADR-006](docs/decisions/ADR-006-taskitem-domain-model.md)).
* Application use cases for authentication (register/login/current user) and task CRUD, returning a
  `Result`/`UseCaseError` type instead of throwing for expected failures.
* EF Core persistence (`ApplicationDbContext`, one migration `InitialCreate`), ASP.NET Core Identity
  (`ApplicationUser`/`ApplicationRole`), and JWT issuance/validation.
* Minimal API endpoints for `/api/auth/*` and `/api/tasks/*`, all mapped to Problem Details on
  failure — including authentication failures themselves (a missing/invalid JWT), not just
  application-layer errors.
* Pagination (`page`/`pageSize`) rejects invalid values with `400` rather than silently correcting
  them.
* A demo user and four demo tasks, seeded idempotently in Development only.
* Development-only CORS for the Angular dev origin (`http://localhost:4200`) — see §19.

Frontend:

* Register, login, logout, session-persisted-across-refresh auth (`AuthStore`, signals-based — see
  [ADR-011](docs/decisions/ADR-011-spa-token-storage.md)).
* Route guards (`authGuard`, `guestGuard`), a functional bearer-token interceptor with a correct
  same-origin check under both an absolute dev API URL and a relative production one — see §21, and
  a 401 session-expiry handler that redirects to `/login` with a return URL.
* Full task CRUD UI: list with status/search filters, create, edit, per-row status change, delete
  with an accessible confirmation dialog.
* Centralized Problem-Details-aware error normalization and a signal-based notification system.

**Not yet implemented:** refresh tokens, email confirmation, password reset, multi-factor auth, role
administration, and a Docker/Testcontainers-based integration test path (Docker was unavailable in
this development environment — see §20). No real end-to-end browser test suite exists (Cypress/
Playwright were out of scope) — see §20 for how this was validated instead.

## 5. Architecture

```text
Domain <- Application <- Infrastructure
                 ^
                API
```

* `Domain` has no outward dependencies (no ASP.NET Core, no EF Core, no Identity).
* `Application` depends only on `Domain`.
* `Infrastructure` depends on `Application` and `Domain`, and implements Application's abstractions.
  It has no dependency on the ASP.NET Core web framework itself (see [ADR-003](docs/decisions/ADR-003-identity-jwt.md)).
* `Api` is the composition root; it depends on `Application` and `Infrastructure`.

These rules are enforced by `tests/Ballastlane.Tasks.ArchitectureTests` (NetArchTest.Rules, 11 tests)
and fail the build if violated. See [`docs/architecture/solution-overview.md`](docs/architecture/solution-overview.md)
for diagrams (system context, containers, authentication flow, task request flow, Angular state
flow, error-handling flow), [`docs/architecture/diagrams.md`](docs/architecture/diagrams.md) for
additional sequence diagrams, and [`docs/architecture/security.md`](docs/architecture/security.md)
for the full security posture.

## 6. Technology stack

* ASP.NET Core Web API (.NET 10), minimal APIs
* SQL Server (LocalDB for local dev and CI's Windows integration-test job; not yet scripted for a
  portable container path)
* Entity Framework Core
* ASP.NET Core Identity
* JWT bearer authentication
* Angular 22 (standalone components, signals, Reactive Forms, functional guards/interceptors) — wired
  to the backend
* Clean Architecture
* Test-Driven Development: xUnit, FluentAssertions, architecture tests, EF Core/Identity integration
  tests, API integration tests, Angular unit tests (Vitest)

## 7. Repository structure

```text
src/
  Ballastlane.Tasks.Domain/            TaskItem, invariants (no framework dependencies)
  Ballastlane.Tasks.Application/       Use cases, DTOs, abstractions, Result/UseCaseError
  Ballastlane.Tasks.Infrastructure/    EF Core, Identity, JWT issuance, repositories, migrations
  Ballastlane.Tasks.Api/               Minimal API endpoints, JWT auth, Problem Details, composition root

tests/
  Ballastlane.Tasks.Domain.Tests/
  Ballastlane.Tasks.Application.Tests/
  Ballastlane.Tasks.Infrastructure.IntegrationTests/
  Ballastlane.Tasks.Api.IntegrationTests/
  Ballastlane.Tasks.ArchitectureTests/

client/
  ballastlane-tasks-web/               Angular SPA (auth + task management, wired to the backend)

docs/
  architecture/                        Solution overview, diagrams, security overview
  decisions/                           Architecture Decision Records (ADRs)
  ai/                                  AI-assisted engineering evidence
  qa/                                  Manual verification checklist and walkthrough
  screenshots/                         Manual screenshot checklist

scripts/                               Build/test helper scripts
```

## 8. Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download) — a **stable** (non-preview) 10.0.x SDK. The
  repository pins the SDK selection via [`global.json`](global.json)
  (`rollForward: latestFeature`, `allowPrerelease: false`); validated against `10.0.302`.
* SQL Server LocalDB (`sqllocaldb info` should list `MSSQLLocalDB`) — ships with Visual Studio / the
  SQL Server Express LocalDB installer. Windows-only; see §18 for how CI handles this.
* [Node.js](https://nodejs.org/) `v24.15.0`+ and npm `11.12.1`+, for the Angular workspace.
* [Angular CLI](https://angular.dev/tools/cli) `22.0.8` (`npx @angular/cli`, no global install
  required).
* The `dotnet-ef` CLI tool is restored automatically from the local tool manifest
  (`dotnet-tools.json`) — no global install; run `dotnet tool restore` once if needed.

## 9. Build commands

```bash
dotnet restore
dotnet build --configuration Release
```

Or use the helper scripts: `./scripts/build.ps1` (Windows) / `./scripts/build.sh` (Linux/macOS).

## 10. Test commands

```bash
dotnet test --configuration Release
```

Or: `./scripts/test.ps1` / `./scripts/test.sh`.

Integration tests (`Ballastlane.Tasks.Infrastructure.IntegrationTests`,
`Ballastlane.Tasks.Api.IntegrationTests`, tagged `[Trait("Category", "Integration")]`) require SQL
Server LocalDB and create/drop their own dedicated databases
(`BallastlaneTasksDb_InfrastructureTests`, `BallastlaneTasksDb_ApiIntegrationTests`) — they do not
touch your local development database. To run only fast, non-database tests:

```bash
dotnet test --filter "Category!=Integration"
```

Or only the LocalDB-backed integration tests:

```bash
dotnet test --filter "Category=Integration"
```

Frontend:

```bash
cd client/ballastlane-tasks-web
npm test -- --watch=false
```

Current verified totals: **99 backend tests** (17 Domain + 34 Application + 11 Architecture + 11
Infrastructure integration + 26 API integration) and **54 frontend tests** — **153 total**. See
[`docs/ai/05-validation-results.md`](docs/ai/05-validation-results.md) for full command output.

## 11. Connection string and user secrets

The LocalDB connection string is already committed in `appsettings.Development.json` (safe — it uses
Windows Integrated auth, no credentials):

```text
Server=(localdb)\MSSQLLocalDB;Database=BallastlaneTasksDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

The JWT signing key is **not** committed anywhere and must be set via user secrets before running the
API locally:

```bash
cd src/Ballastlane.Tasks.Api
dotnet user-secrets set "Jwt:SigningKey" "local-dev-only-signing-key-not-for-prod-32chars-min"
```

(The value above is a clearly fake, development-only placeholder — pick your own local value; it must
be at least 32 characters. Production must supply its own signing key via environment variables or a
secret manager — never `appsettings.json`. `Jwt:SigningKey` fails the app fast at startup
(`ValidateOnStart`) if left unset or under 32 characters, rather than starting insecurely.)

## 12. Migrations

```bash
# Apply migrations / create the database
dotnet tool restore
dotnet dotnet-ef database update \
  --project src/Ballastlane.Tasks.Infrastructure \
  --startup-project src/Ballastlane.Tasks.Api

# List migrations
dotnet dotnet-ef migrations list \
  --project src/Ballastlane.Tasks.Infrastructure \
  --startup-project src/Ballastlane.Tasks.Api
```

In practice you don't need to run `database update` manually for local development — `Program.cs`
applies pending migrations (and seeds demo data) automatically on startup when
`ASPNETCORE_ENVIRONMENT=Development`.

## 13. Backend and Angular startup

```bash
dotnet run --project src/Ballastlane.Tasks.Api
```

By default this uses `launchSettings.json`, which sets `ASPNETCORE_ENVIRONMENT=Development` and binds
to `http://localhost:5276` / `https://localhost:7111`. The OpenAPI document is served at
`/openapi/v1.json` in Development; pair it with any OpenAPI UI (e.g. Scalar or Swagger UI) to get an
interactive "Authorize" button — paste in the `accessToken` from `POST /api/auth/login` (as
`Bearer <token>`) to call protected endpoints. Swagger/OpenAPI is registered only in Development —
it is never exposed in a production build.

```bash
cd client/ballastlane-tasks-web
npm ci
npm start            # dev server at http://localhost:4200
```

## 14. Demo credentials

Seeded automatically on startup in Development (idempotent — safe to restart repeatedly):

```text
Email:    demo@ballastlane.local
Password: Demo1234!
```

The demo user has four tasks: one pending, one in-progress, one completed, and one with a future due
date.

## 15. API endpoints

| Method | Route                       | Auth        | Success | Notes |
|--------|------------------------------|-------------|---------|-------|
| GET    | `/health`                    | Anonymous   | 200     | `{ "status": "Healthy" }` |
| POST   | `/api/auth/register`         | Anonymous   | 201     | Returns the created user (no token — see [ADR-011](docs/decisions/ADR-011-spa-token-storage.md)); `409` on duplicate email |
| POST   | `/api/auth/login`            | Anonymous   | 200     | Returns `{ user, accessToken, tokenType, expiresAtUtc }`; `401` on invalid credentials (generic — never reveals whether the email exists) |
| GET    | `/api/auth/me`                | Bearer      | 200     | Current user profile; `401` (Problem Details) if anonymous |
| GET    | `/api/tasks`                 | Bearer      | 200     | Paginated (`page`, `pageSize`, default 1/20, max `pageSize` 100); filterable by `status` and `search`; `400` if `page < 1` or `pageSize` out of range |
| GET    | `/api/tasks/{id}`             | Bearer      | 200     | `404` if not found **or** owned by another user (see [ADR-008](docs/decisions/ADR-008-cross-user-404.md)) |
| POST   | `/api/tasks`                 | Bearer      | 201     | `Location` header; `400` on invalid title/due date |
| PUT    | `/api/tasks/{id}`              | Bearer      | 200     | `400`/`404` as above |
| PATCH  | `/api/tasks/{id}/status`      | Bearer      | 200     | `{ "status": "Pending" \| "InProgress" \| "Completed" }` |
| DELETE | `/api/tasks/{id}`              | Bearer      | 204     | `404` if not found/not yours |

All error responses use [RFC 7807 Problem Details](https://www.rfc-editor.org/rfc/rfc7807)
(`application/problem+json`) with a `type` URN identifying the error code (e.g.
`urn:ballastlane-tasks:error:task.not_found`) — including `401`s produced by JWT authentication
itself, not just application-layer failures.

## 16. Frontend routes

| Path | Guard | Component |
|------|-------|-----------|
| `/` | — | redirects to `/tasks` |
| `/login` | `guestGuard` (redirects away if already authenticated) | `Login` |
| `/register` | `guestGuard` | `Register` |
| `/tasks` | `authGuard` | `TaskList` |
| `/tasks/new` | `authGuard` | `TaskCreatePage` |
| `/tasks/:id/edit` | `authGuard` | `TaskEditPage` |
| `/profile` | `authGuard` | `ProfilePage` |
| `**` | — | `NotFound` |

All routes are lazy-loaded (`loadComponent`). `authGuard`/`guestGuard` both wait on
`AuthStore.initialize()` before deciding, and `authGuard` preserves the attempted URL as a
`returnUrl` query parameter for post-login redirect.

## 17. Angular frontend

The Angular SPA at `client/ballastlane-tasks-web` implements registration, login, session-persisted
auth, and full task management, talking to the real backend above. See
[`client/ballastlane-tasks-web/README.md`](client/ballastlane-tasks-web/README.md) for
frontend-specific setup (API base URL, CORS, routes, demo credentials).

### Frontend architecture

* **Feature structure**: `core/` (auth, http, config, notifications — cross-cutting), `shared/`
  (components/utilities/validators used by more than one feature), `layout/` (header), `features/`
  (`auth`, `tasks`, `profile` — one folder per bounded UI concern).
* **Auth state**: `AuthStore` (`core/auth/auth.store.ts`) — a signal-based store, not a full state
  library (no NgRx). Exposes `user`, `isAuthenticated`, `isInitialized`, `isLoading` as readonly
  signals; `initialize()` is idempotent (cached/shared) so route guards and the app shell never
  trigger duplicate `/api/auth/me` requests.
* **Token storage**: `sessionStorage`, via `TokenStorageService` — see
  [ADR-011](docs/decisions/ADR-011-spa-token-storage.md) for the full trade-off discussion.
* **API base URL**: a single `API_BASE_URL` injection token (`core/config/api-config.ts`), sourced
  from `environments/environment*.ts` — `http://localhost:5276` in development, a relative empty
  string (`/api/...` resolved against whatever origin serves the build) in production. See §21 for
  why this is a relative path rather than a hard-coded hostname.
* **Interceptors**: `authInterceptor` attaches `Authorization: Bearer <token>` only to requests whose
  resolved origin matches the API's origin — computed via the `URL` API so it stays correct whether
  `API_BASE_URL` is an absolute dev URL or an empty/relative production one (see §21).
  `httpErrorInterceptor` reacts to a `401` on any *other* authenticated request by clearing the
  session and redirecting to `/login?returnUrl=...` (login/register's own `401`s are excluded —
  those mean "wrong credentials," not "session expired").
* **Guards**: see §16.
* **Error normalization**: `ApiErrorService` turns any `HttpErrorResponse` into a typed `ApiError`
  (`kind`/`status`/`message`/`details`), matching the backend's actual Problem Details shape (`errors`
  is a flat `string[]`, confirmed against a running instance — not the `Record<string,string[]>` shape
  ASP.NET Core MVC's default validation problem details would use).
* **Notifications**: a minimal signal-based `NotificationService` + one ARIA live region
  (`NotificationHost`) — no toast library.
* **Task state**: no client-side cache/store for tasks — `TaskList` fetches fresh from
  `GET /api/tasks` on load and after each mutation's local optimistic-free update, matching "prefer a
  straightforward server-confirmed update over complex optimistic behavior."

## 18. Continuous integration

`.github/workflows/ci.yml` runs on every push/PR to `main`, as three jobs:

* **`backend-unit`** (`ubuntu-latest`) — restore, build (Release), run Domain/Application/Architecture
  tests (`--filter "Category!=Integration"`, fully in-memory, no database needed), verify code
  formatting (`dotnet format --verify-no-changes`), and check for vulnerable NuGet packages
  (`dotnet list package --vulnerable --include-transitive`).
* **`backend-integration`** (`windows-latest`) — restore, build, start SQL Server LocalDB, run the
  Infrastructure/API integration tests (`--filter "Category=Integration"`). LocalDB is Windows-only,
  so this job runs on Windows specifically; the unit/architecture tests run cross-platform on Linux
  instead of paying for a Windows runner unnecessarily.
* **`frontend`** (`ubuntu-latest`) — `npm ci`, `npm test -- --watch=false`, `npm run build`.

Both `--filter` expressions were validated locally before being trusted in CI: `Category!=Integration`
selects exactly the 62 unit/architecture tests with zero overlap, `Category=Integration` selects
exactly the 37 integration tests, with neither filter accidentally including or excluding a test
project (see [`docs/ai/05-validation-results.md`](docs/ai/05-validation-results.md)).

## 19. Security notes

* No production secrets belong in source control — see §11.
* JWTs carry `sub`, `email`, `given_name`, `family_name`, and `jti` claims, are signed HMAC-SHA256, and
  expire after `Jwt:ExpirationMinutes` (default 60). No refresh-token flow exists — see §22.
* Login failures (wrong password vs. unknown email) return the identical `401` Problem Details body —
  the API never reveals whether an email is registered.
* A task belonging to another user returns `404`, never `403` — see [ADR-008](docs/decisions/ADR-008-cross-user-404.md).
* Password policy (min length 8, lockout after 5 failed attempts, 5-minute lockout) is ASP.NET Core
  Identity's, configured in `InfrastructureServiceCollectionExtensions`.
* Unhandled exceptions never leak stack traces or exception messages to the client, in any environment
  — see `GlobalExceptionHandler`. Authentication failures (missing/invalid/expired JWT) also return a
  Problem Details body with no internal detail, via a `JwtBearerEvents.OnChallenge` handler.
* **Frontend**: the JWT is stored in `sessionStorage`, readable by any script on the page — it is
  *not* immune to XSS, only shorter-lived than `localStorage`. See
  [ADR-011](docs/decisions/ADR-011-spa-token-storage.md) for the full trade-off and the production
  alternative (`Secure`/`HttpOnly`/`SameSite` cookies). Angular's route guards are UX only — the API
  independently authorizes every request against the JWT regardless of what the SPA renders.
* CORS in Development allows only `http://localhost:4200`, with no credentials mode requested — see
  `AddDevelopmentCors` and the CORS integration tests in `Ballastlane.Tasks.Api.IntegrationTests`. No
  CORS policy is registered outside Development; the production deployment strategy (§21) serves the
  SPA and API from the same origin, so no cross-origin policy is needed there either.
* Full detail: [`docs/architecture/security.md`](docs/architecture/security.md).

## 20. Known limitations

* **Docker was not available in this development environment** (`docker --version` /
  `docker info` both failed). Integration tests use SQL Server LocalDB, the documented fallback per
  [ADR-002](docs/decisions/ADR-002-sql-server-localdb.md) — never EF Core InMemory or SQLite. A SQL
  Server container / Docker Compose path for non-Windows contributors remains unscripted; CI instead
  runs LocalDB-backed tests on a Windows runner (§18).
* **No real browser automation tool was available either** (no `chromium-cli`, no Playwright/Cypress
  installed — and installing one was explicitly out of scope). The full register→login→create→
  edit→status-change→delete→logout flow was therefore validated via: the automated Angular
  component/service test suite (54 tests), a successful production build, and a manual `curl`-level
  walkthrough of the exact JSON contracts the SPA consumes against the real running backend — not an
  actual browser DOM/console check. This is a real gap, not a scope choice. No manual browser
  verification has been recorded as passed — [`docs/qa/manual-verification-checklist.md`](docs/qa/manual-verification-checklist.md)
  is the outstanding checklist, and [`docs/screenshots/`](docs/screenshots/) contains a manual
  capture checklist, in lieu of fabricated screenshots or unverified claims.
* No refresh tokens, email confirmation, password reset, or MFA (explicitly out of scope).
* No production backend has actually been deployed — the relative-`/api` strategy (§21) is validated
  by design and by test, not by an actual production deployment.

## 21. Trade-offs

Decisions made deliberately in favor of scope-appropriate simplicity over production-grade
robustness. Each has its full reasoning recorded in an ADR where one exists.

| Decision | Benefit | Cost | Alternative | When to reconsider |
|---|---|---|---|---|
| Clean Architecture, modular monolith ([ADR-001](docs/decisions/ADR-001-clean-architecture-modular-monolith.md)) | Business rules independent of framework/database; one deployable | More files/indirection than a single-project app; can't scale parts independently | Microservices | If distinct parts genuinely needed independent scaling |
| SQL Server, LocalDB for dev/CI ([ADR-002](docs/decisions/ADR-002-sql-server-localdb.md)) | Real SQL Server behavior, matches the likely production target (Azure SQL) | Windows-only, so integration tests need a dedicated Windows CI job | Testcontainers/a SQL Server Docker image | As soon as Docker is available in the build environment |
| `sessionStorage` for the JWT ([ADR-011](docs/decisions/ADR-011-spa-token-storage.md)) | Survives refresh, clears on tab close, no CSRF machinery needed | Readable by any script on the page — not immune to XSS | `HttpOnly` cookies (immune to XSS-based theft, needs CSRF mitigation) | Before handling real user data in production |
| No refresh tokens | Simpler — no rotation/revocation/second-credential storage | Expired session forces a full re-login | Refresh-token rotation, or session-tracked `HttpOnly` cookies | Before any production deployment |
| A single `DbContext` ([ADR-009](docs/decisions/ADR-009-single-dbcontext.md)) | No cross-context transaction complexity | Identity's schema and the app's own schema share one migration history | Two separate `DbContext`s | If Identity were ever split into a separate service |
| Relative `/api` production URL (§17, §18) | No hard-coded hostname; works behind any reverse proxy on the same origin | Doesn't support a split-origin deployment without revisiting | A build-time-injected absolute API URL | If the SPA and API are ever deployed to separate origins |
| No client-side task cache/store | Every list view is a fresh, server-confirmed `GET /api/tasks` | A bit more network chatter than an optimistic-update UI | A client-side cache with optimistic updates | If perceived latency became a real problem at scale |
| Explicit use cases, no generic repository | Intention-revealing operations; ownership scoping (`ownerId`) can't be forgotten | More files than a generic CRUD service | A generic `CrudService<T>` / `IRepository<T>` | If the number of near-identical CRUD use cases grew very large |
| No MediatR | No extra indirection for handlers already directly injectable | No built-in cross-cutting pipeline (logging/validation decorators) | MediatR with pipeline behaviors | If cross-cutting concerns across handlers multiplied |
| No NgRx | Signals fit this app's small, mostly server-confirmed state | No time-travel debugging, no built-in devtools story | NgRx (actions/reducers/effects/selectors) | If cross-feature shared or complex derived state grew substantially |
| `DateOnly` for due dates ([ADR-006](docs/decisions/ADR-006-taskitem-domain-model.md)) | Structurally avoids the UTC-midnight timezone bug | Slightly more manual string handling on the frontend | `DateTime` with explicit UTC normalization everywhere | Rarely, for a date-only field |
| RFC 7807 Problem Details everywhere | One consistent error shape for the frontend to parse, from every layer | Slightly more ceremony per failure path | Ad hoc error responses per endpoint | Rarely — the consistency is close to free once set up |
| Architecture tests ([ADR-005](docs/decisions/ADR-005-testing-strategy.md)) | Dependency-direction violations fail the build, not just review | Small added build time | Convention + code review only | Never fully — even these need proving non-vacuous |
| No browser E2E tests | No tooling installation needed where none was available | The full user flow is validated via component tests + a manual API walkthrough, not an actual browser session | Playwright/Cypress E2E suite | As soon as browser automation tooling is available |

## 22. Production improvements

If this moved beyond an engineering exercise toward a real deployment, the highest-value next steps,
roughly in priority order:

1. **Refresh-token rotation** (or move to `HttpOnly`/`Secure`/`SameSite` cookies with CSRF
   mitigation) — the single biggest gap between this and a production auth story.
2. **A real production deployment target**, to validate the relative-`/api` origin assumption (§21)
   against an actual reverse-proxy/hosting setup rather than just local reasoning.
3. **Containerized integration tests** (Testcontainers or a SQL Server Docker image) to remove the
   Windows-only constraint on the integration test suite and its CI job (§18).
4. **Real browser end-to-end tests** (Playwright), to close the gap documented in §20 — this project
   was validated via component/unit tests and a manual API contract walkthrough instead.
5. Rate limiting on `/api/auth/login` and `/api/auth/register`, beyond ASP.NET Core Identity's
   built-in lockout, to blunt credential-stuffing/enumeration attempts at the network layer.
6. Structured, centralized logging/observability (the current logging is ASP.NET Core's default
   console logger) if this were to run anywhere other than a developer's machine.

## 23. Architecture decision records

See [`docs/decisions/`](docs/decisions/):

* [ADR-001 — Clean Architecture modular monolith](docs/decisions/ADR-001-clean-architecture-modular-monolith.md)
* [ADR-002 — SQL Server and LocalDB](docs/decisions/ADR-002-sql-server-localdb.md)
* [ADR-003 — ASP.NET Core Identity and JWT](docs/decisions/ADR-003-identity-jwt.md)
* [ADR-004 — Angular SPA](docs/decisions/ADR-004-angular-spa.md)
* [ADR-005 — Testing strategy](docs/decisions/ADR-005-testing-strategy.md)
* [ADR-006 — TaskItem domain model](docs/decisions/ADR-006-taskitem-domain-model.md)
* [ADR-007 — Application abstractions and the omission of IUnitOfWork](docs/decisions/ADR-007-application-abstractions.md)
* [ADR-008 — Cross-user 404](docs/decisions/ADR-008-cross-user-404.md)
* [ADR-009 — Single DbContext for Identity and application data](docs/decisions/ADR-009-single-dbcontext.md)
* [ADR-010 — JWT storage deferred to the Angular implementation phase](docs/decisions/ADR-010-jwt-storage-deferred.md) *(superseded by ADR-011)*
* [ADR-011 — Store the development SPA access token in sessionStorage](docs/decisions/ADR-011-spa-token-storage.md)

## 24. AI-assisted engineering workflow

Architectural analysis, delivery planning, acceptance criteria, and technical review were supported
by ChatGPT. Scoped implementation tasks were executed with Claude Code. Final decisions on
architecture, security, validation, and acceptance remained the developer's responsibility
throughout — no generated change was accepted automatically. Acceptance was based on code review,
builds, unit tests, integration tests, architecture tests, vulnerability checks, and live validation
against the running application. See [`docs/ai/`](docs/ai/) for the full, non-fabricated evidence
trail:

* [`docs/ai/01-development-workflow.md`](docs/ai/01-development-workflow.md) — the division of labor
  between ChatGPT and Claude Code, and representative excerpts of the prompts used to direct
  implementation work.
* [`docs/ai/02-representative-output.md`](docs/ai/02-representative-output.md) — representative
  generated code, before/after, for real defects.
* [`docs/ai/03-review-findings.md`](docs/ai/03-review-findings.md) — issues actually found during
  review.
* [`docs/ai/04-corrections.md`](docs/ai/04-corrections.md) — what was changed in response, and why.
* [`docs/ai/05-validation-results.md`](docs/ai/05-validation-results.md) — actual build/test/format/
  audit command output validating the corrected code.

## 25. Manual verification

No browser automation tool is available in this development environment, so the following are
maintained as manual checklists for a contributor or reviewer with real browser access — see
[`docs/qa/`](docs/qa/):

* [`docs/qa/manual-walkthrough.md`](docs/qa/manual-walkthrough.md) — a step-by-step sequence for
  running the application end to end, including recovery steps if LocalDB, a port, the HTTPS
  certificate, `npm start`, or the OpenAPI UI misbehave.
* [`docs/qa/manual-verification-checklist.md`](docs/qa/manual-verification-checklist.md) — a checklist
  covering registration/login, task CRUD, session handling, cross-user isolation, accessibility, and
  a security spot-check (no token leakage to third-party origins). No item on it has been confirmed
  in an actual browser as of this document's last update.
