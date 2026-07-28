# 04 — Corrections

What was actually changed in response to each finding in `03-review-findings.md`, and why.

## Sprint 4 findings

1. **Pagination validation** — `ListTasksHandler.HandleAsync` now returns
   `Result.Failure<PagedResult<TaskDto>>(UseCaseError.Validation(...))` for `page < 1` or
   `pageSize` outside `[1, 100]`, which `ResultProblemExtensions.ToProblem()` maps to `400`. Covered
   by new tests: `ListTasksHandlerTests.HandleAsync_WithNonPositivePage_ShouldReturnValidationError`,
   `HandleAsync_WithOutOfRangePageSize_ShouldReturnValidationError`, and API-level
   `TaskEndpointTests.ListTasks_WithNonPositivePage_ShouldReturnBadRequest`,
   `ListTasks_WithOutOfRangePageSize_ShouldReturnBadRequest`.
2. **Consistent 401** — Added `JwtBearerEvents.OnChallenge` in `ApiServiceCollectionExtensions.cs`
   to write the same `ProblemDetails` shape (`application/problem+json`, `urn:ballastlane-tasks:error:auth.required`)
   as every other 401 in the API. Covered by
   `AuthEndpointTests.ProtectedEndpoint_WithoutToken_ShouldReturnProblemDetailsBody`.
3. **Auth interceptor same-origin check** — Replaced `startsWith` with a `URL`-based origin
   comparison (`new URL(request.url, window.location.origin).origin`) that correctly handles both
   an absolute dev `API_BASE_URL` and an empty/relative production one. Covered by new tests in
   `auth.interceptor.spec.ts` under a `'authInterceptor with a relative (production-style)
   API_BASE_URL'` describe block, including one that proves an external absolute URL is still
   excluded even when `API_BASE_URL` is `''`.
4. **Uncommitted build files** — Not fixed by rewriting anything; the fix is procedural. Per the
   user's explicit choice (asked via a direct question rather than assumed), `Directory.Build.props`
   and `Directory.Packages.props` were `git add`ed (staged) but deliberately **not committed** in
   this session — committing remains the user's decision, consistent with this project's standing
   rule to never commit without being asked.
5. **CI LocalDB placement** — Split the single `backend` job into `backend-unit` (`ubuntu-latest`,
   runs `--filter "Category!=Integration"`, i.e. Domain/Application/Architecture tests, plus
   `dotnet format --verify-no-changes` and `dotnet list package --vulnerable --include-transitive`,
   neither of which existed before) and `backend-integration` (`windows-latest`, runs
   `--filter "Category=Integration"` against LocalDB). Both filter expressions were run locally
   before committing to the design — `Category!=Integration` selected exactly the 62 unit/
   architecture tests, `Category=Integration` selected exactly the 37 integration tests, with zero
   overlap or omission.
6. **ADR-010 status** — Updated to "Superseded by ADR-011", with a matching back-reference added to
   ADR-011's Context section.
7. **`.gitignore` secrets pattern** — Added `appsettings.Local.json` and `appsettings.local.json`
   alongside the existing (now also case-doubled) `appsettings.*.Local.json` /
   `appsettings.*.local.json`, so the actual .NET local-override filename convention is covered
   regardless of the case-sensitivity of the filesystem running `git status`.

## Sprint 3 findings (recorded previously, summarized here for continuity)

8. **Pagination defaults** — `page`/`pageSize` were given defaults (`1`/`20`) at the minimal-API
   parameter level, and `GlobalExceptionHandler` was taught to map `BadHttpRequestException` to
   `400` instead of the previous generic `500`.
9. **CORS** — Added a Development-only named CORS policy (`AddDevelopmentCors`), gated to
   `IsDevelopment()`, no wildcard origin, no credentials mode.

## Sprint 2 findings (recorded previously, summarized here for continuity)

15. **Missing FK setup in `TaskRepositoryTests`** — fixed by seeding real `ApplicationUser` rows
    instead of using bare random `Guid`s as owner IDs, so the tests exercise the real FK constraint
    instead of accidentally working around it.
