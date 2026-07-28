# 03 — Review findings

Findings from human/AI-assisted review across the project, grouped by when they surfaced. Only
real findings are listed.

## Found during Sprint 4's repository-wide gap analysis

1. **Pagination silently clamped invalid input instead of rejecting it.**
   `ListTasksHandler.cs:18-19` used `Math.Max(query.Page, 1)` / `Math.Clamp(query.PageSize, 1, 100)`
   — `page=0`, `page=-5`, or `pageSize=0` were silently corrected rather than rejected, even though
   the sprint's own validation requirement was "invalid zero or negative values produce 400."
2. **Inconsistent 401 response shape.** A missing/invalid JWT fell through to `JwtBearerHandler`'s
   default challenge response, which has a different body than the application's own
   `UseCaseError.Unauthorized().ToProblem()` 401s — same status code, different contract.
3. **Auth interceptor's same-origin check was vacuously true in production.**
   `request.url.startsWith(baseUrl)` in `auth.interceptor.ts` is always `true` when `baseUrl` is
   `''` (the intended production, relative-`/api` configuration) — meaning the bearer token would
   attach to any outgoing request, not just same-origin API calls.
4. **`Directory.Build.props` and `Directory.Packages.props` were never committed**, in any commit
   including "sprint 3 completed" — confirmed via `git ls-tree <commit> --name-only` returning
   nothing for either path, and `git log --all --full-history -- <path>` returning no history at
   all. A clean clone of the repository as it stood could not restore or build.
5. **CI ran SQL-Server-LocalDB-dependent integration tests on `ubuntu-latest`.** LocalDB is
   Windows-only; `.github/workflows/ci.yml`'s `dotnet test` step had no `--filter`, so it invoked
   the full solution including `Infrastructure.IntegrationTests` and `Api.IntegrationTests` on a
   runner that cannot satisfy their `(localdb)\MSSQLLocalDB` connection string.
6. **ADR-010's status was stale.** It still read plain "Accepted" even though ADR-011 had since
   made and recorded the actual token-storage decision it deferred.
7. **`.gitignore`'s secrets pattern didn't match the real convention.**
   `appsettings.*.local.json` requires an extra environment segment and lowercase `local`; it would
   not match the actual .NET local-override filename, `appsettings.Local.json`.

## Found during Sprint 3 (frontend integration against the real backend)

8. **Pagination defaults defect.** `GET /api/tasks`'s `page`/`pageSize` parameters had no defaults,
   so a plain `GET /api/tasks` (issued by the Angular task-list page on first load) threw
   `BadHttpRequestException` for the missing values, which `GlobalExceptionHandler` mapped to a
   generic `500` instead of `400`. Only surfaced because a real browser-equivalent client (the
   Angular app, then a `curl` walkthrough) called the endpoint the way a user actually would —
   backend unit/integration tests always passed `page`/`pageSize` explicitly.
9. **CORS omission.** No CORS policy existed at all until the Angular dev server's cross-origin
   requests to the API were actually attempted and failed in the browser network tab.

## Design decisions verified during review, not defects

10. **Cross-user task access returns 404, not 403** (by design — see ADR-008) — reviewed to confirm
    this was deliberate, not an oversight of "correct" REST status codes.
11. **The authenticated user ID is always derived from the JWT's `sub` claim (`ICurrentUser`),
    never accepted from the request body or route** — reviewed across every task handler to confirm
    no endpoint trusts a client-supplied owner ID.
12. **ASP.NET Core Identity types stay inside `Infrastructure`** — `IdentityService` implements the
    `Application`-owned `IIdentityService` abstraction; `Application` and `Domain` have no reference
    to `Microsoft.AspNetCore.Identity`, enforced by an architecture test.
13. **`DateOnly` is used for due dates end-to-end** (request DTO → domain → EF Core column mapping),
    never `DateTime`, to avoid UTC-midnight timezone shift bugs.

## Tests that caught real bugs (not just passed as expected)

14. **Architecture negative-control test.** `Domain_ShouldNotDependOn_EntityFrameworkCore` was
    deliberately made to fail once (by temporarily adding an EF Core reference + usage to `Domain`),
    then reverted — proving the architecture tests actually fail on a real violation, not just pass
    vacuously.
15. **Missing FK setup caught by a real test failure.** `TaskRepositoryTests` originally used random
    `Guid`s as owner IDs with no corresponding `ApplicationUser` rows, which violated the
    `Tasks.OwnerId -> AspNetUsers.Id` foreign key against the real LocalDB database — fixed by
    seeding real users in test setup. This validated that the FK constraint was actually enforced,
    not a schema bug.
16. **`BadHttpRequestException` -> 500 caught by contract testing**, not by inspection — see
    finding 8 above; the fix is covered by `TaskEndpointTests.ListTasks_WithoutPagingParameters_*`.
