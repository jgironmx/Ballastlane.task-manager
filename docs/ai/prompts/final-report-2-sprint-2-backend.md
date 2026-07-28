# Final Report — Sprint 2: Backend Implementation

This report covers Sprint 2, delivered by Claude Code in response to
[the original prompt](original-prompt-2-sprint-2-backend.md) — the full Clean Architecture backend
(Domain/Application/Infrastructure/API), ASP.NET Core Identity + JWT authentication, EF Core/SQL
Server persistence, task CRUD, seed data, and automated tests. No Angular work was in scope. 

See also the reports for:
[Increment 0](final-report-1-increment-0.md).
[Sprint 3](final-report-3-sprint-3-angular.md).

---

## 1. Executive summary

Sprint 2 turned the Increment 0 skeleton into a working, fully tested backend: user
registration/login with JWT bearer auth, owner-scoped task CRUD, RFC 7807 problem responses, EF
Core migrations against SQL Server LocalDB, and a development seed dataset. All work stayed inside
`src/` and `tests/`; nothing under `client/` was touched.

## 2. Domain layer

- `TaskItem` aggregate (Domain) with `TaskItemStatus` enum (`Pending`, `InProgress`, `Completed`),
  owner id, title/description, due date (`DateOnly?`), and invariant-enforcing methods
  (`Create`, `UpdateDetails`, `ChangeStatus`) rather than public setters.
- Documented in **ADR-006** (TaskItem domain model): why status transitions are unrestricted
  (any→any) per the prompt's simplicity preference, and why `DateOnly` was chosen over `DateTime`
  for due dates.
- 17 Domain unit tests covering construction validation and every invariant path.

## 3. Application layer

- `Result` / `Result<T>` / `UseCaseError` / `ErrorType` (Common) — an explicit success/failure
  pattern for expected failures (validation, not-found, conflict), reserving exceptions for truly
  exceptional conditions. Chose this over throwing custom exceptions for control flow, and over a
  third-party Result library, to keep the dependency graph clean — rationale in **ADR-007**
  (application abstractions).
- Abstractions owned by Application and implemented by Infrastructure/API:
  `ITaskRepository`, `IIdentityService`, `ITokenService`, `ICurrentUser`, `IClock`.
- Vertical-slice feature folders: `Features/Authentication/*` (Register, Login, Me) and
  `Features/Tasks/*` (Create, GetById, List, Update, ChangeStatus, Delete), each a small
  handler class with its own request/response DTOs and FluentValidation-style checks.
- Cross-user access returns **404**, not 403, so authenticated users cannot infer another user's
  task IDs exist — documented in **ADR-008** (cross-user 404).
- 29 Application unit tests (handlers exercised against fakes/mocks of the abstractions above).

## 4. Infrastructure layer

- `ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` — a single
  `DbContext` for both Identity and domain tables, rationale in **ADR-009** (single DbContext):
  avoids two-phase-commit/cross-context transaction issues for a project this size.
  - `ApplicationUser`/`ApplicationRole` extend `IdentityUser<Guid>`/`IdentityRole<Guid>`.
  - `TaskItemConfiguration` (Fluent API): owner FK to `AspNetUsers.Id`, required/optional column
    mapping, `DateOnly` conversion.
- `TaskRepository` (EF Core-backed `ITaskRepository`), `IdentityService` (wraps `UserManager<T>`
  only — no `SignInManager`, since the API is stateless JWT, not cookie auth), `TokenService`
  (HMAC-SHA256 JWT issuance), `SystemClock` (`IClock` wrapping `DateTimeOffset.UtcNow` for
  testability).
- `DevelopmentDataSeeder`: seeds two demo users and a handful of tasks, gated to the Development
  environment only.
- One EF Core migration, `InitialCreate`, generated with a **local** `dotnet-ef` tool
  (`dotnet-tools.json`), never installed globally. A scoped `.editorconfig` inside `Migrations/`
  suppresses generated-code style rules (`IDE0161`, `CA1861`, line-ending/BOM) without touching the
  repo-wide style rules.
- 11 Infrastructure integration tests against real LocalDB databases (repository CRUD, Identity
  operations, seeding), grouped into a `[CollectionDefinition]`/`ICollectionFixture`
  (`InfrastructureTestGroup`) to force sequential execution — parallel xUnit classes were racing on
  the same LocalDB database.

## 5. API layer

- Minimal API endpoints: `AuthEndpoints` (`POST /api/auth/register`, `/login`, `GET /api/auth/me`)
  and `TaskEndpoints` (full CRUD + status change under `/api/tasks`), composed in `Program.cs`.
- `ResultProblemExtensions` maps `Result`/`Result<T>` failures to RFC 7807 `ProblemDetails`
  centrally, keyed off `ErrorType`.
- `GlobalExceptionHandler` (`IExceptionHandler`) catches unhandled exceptions and returns a
  generic 500 problem response, keeping stack traces out of API responses.
- OpenAPI document configured with a bearer JWT security scheme so `/openapi/v1.json` /
  Scalar UI can authorize interactively.
- 20 API integration tests via `WebApplicationFactory<Program>`, issuing real JWTs and exercising
  the full HTTP pipeline (auth required/rejected, validation errors, ownership checks, CRUD
  happy/edge paths), grouped into `ApiTestGroup` for the same sequential-LocalDB reason as above.

## 6. Architecture rules

Extended Increment 0's 9 NetArchTest rules with a 10th: Domain must not depend on
`Microsoft.AspNetCore.Identity` — proven non-vacuous the same way as Increment 0 (temporarily
violate, confirm red, revert). 11 architecture tests total (10 rules + 1 non-vacuity guard).

## 7. Documentation created/updated
New: **ADR-006** (TaskItem domain model), **ADR-007** (application abstractions), **ADR-008**
(cross-user 404), **ADR-009** (single DbContext). Updated: **ADR-002** (LocalDB — connection
string/migration workflow finalized), **ADR-003** (Identity/JWT — implementation details filled
in), **ADR-005** (testing strategy — integration test grouping strategy added),
`docs/architecture/solution-overview.md`, root `README.md`.

## 8. Validation commands and results
| Command | Result |
|---|---|
| `dotnet build --configuration Release` | ✅ 0 warnings, 0 errors |
| `dotnet test --configuration Release` | ✅ 88/88 passed (Domain 17, Application 29, Architecture 11, Infrastructure.IntegrationTests 11, Api.IntegrationTests 20) |
| `dotnet format --verify-no-changes` | ✅ exit 0 |
| `dotnet ef migrations list` / `database update` | ✅ `InitialCreate` applied to LocalDB |
| NuGet vulnerability audit | ✅ no vulnerable packages |

## 9. Deviations or limitations
- Removed Increment 0's underscore-prefixed-private-field `.editorconfig` naming rule entirely —
  it conflicted with C# 12 primary constructors (pervasive in this sprint's code), which capture
  parameters directly rather than assigning to `_field`. Documented as an intentional style
  deviation, not an oversight.
- `IdentityService` uses only `UserManager<ApplicationUser>`, deliberately omitting
  `SignInManager` — the API never issues auth cookies, so `SignInManager`'s cookie-auth
  responsibilities are out of scope.
- Cross-user task access returns 404 rather than 403 (ADR-008) — a deliberate information-leak
  reduction, not an oversight of the "correct" HTTP status.
- Git identity was unconfigured; asked the user explicitly via `AskUserQuestion` whether to
  configure it or proceed uncommitted — **user chose to proceed without committing**. Nothing was
  committed during this sprint (documented deviation from the prompt's own commit precondition,
  by explicit user instruction).

## 10. Git status
Nothing committed (HEAD still the original `Initial commit`, `d849580`), per the user's explicit
choice above. All Sprint 2 changes remain in the working tree.

## 11. Recommended next sprint
Angular authentication and task CRUD UI, wired to this real backend — became Sprint 3.
