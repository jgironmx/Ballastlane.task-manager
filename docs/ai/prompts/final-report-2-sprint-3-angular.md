# Final Report — Sprint 3: Angular Authentication and Task CRUD Frontend

This report covers Sprint 3, delivered by Claude Code in response to
[the original prompt](original-prompt-3-sprint-3-angular.md) — the Angular frontend (auth +
task CRUD) wired to the real Sprint 2 backend, with only minimal, justified backend changes.

See also the reports for:
[Increment 0](final-report-1-increment-0.md).
[Sprint 2](final-report-2-sprint-2-backend.md).

---

## 1. Executive summary

Sprint 3 built a complete Angular 22 SPA — registration, login, session handling, and full task
CRUD (list, create, edit, status change, delete) — against the live ASP.NET Core backend from
Sprint 2. Two backend defects surfaced only through real frontend integration were fixed, each
minimized, explained, and covered by new tests, per the sprint's explicit constraint.

## 2. Git precondition

As in Sprint 2, git identity was unconfigured and the tree still uncommitted. Asked via
`AskUserQuestion` again at the start of this sprint; **user again chose to proceed without
committing**. Nothing has been committed at any point in this project — HEAD remains the original
`Initial commit` (`d849580`).

## 3. Core infrastructure (`core/`)
- `core/auth/*`: `AuthService` (register/login/me/logout HTTP calls), a signal-based `AuthStore`
  (current user, authenticated flag), functional route guards (`authGuard`, `guestGuard`), an
  `authInterceptor` (attaches the bearer token to outgoing requests), and `TokenStorage`
  (wraps `sessionStorage`).
- `core/http/*`: typed `ApiError` model + service, and an `httpErrorInterceptor` that normalizes
  backend `ProblemDetails` responses into a consistent shape for the UI.
- `core/notifications/*`: a minimal toast/notification service for success/error feedback.
- `core/config/api-config.ts`: centralizes the API base URL.

## 4. Shared and layout
- `shared/components/*` (e.g. confirmation dialog), `shared/utilities/date-only.ts` (parses/
  formats `yyyy-MM-dd` strings **without ever constructing a `Date` object**, avoiding UTC-midnight
  off-by-one-day bugs), `shared/validators/password-match.validator.ts`.
- `layout/header/header.ts`: session-aware nav (login/register vs. user menu/logout).

## 5. Features
- `features/auth/{login,register}`: Reactive Forms with client-side validation mirroring backend
  rules, server error surfacing via the http-error interceptor.
- `features/tasks/*`: list (with pagination matching the backend's `page`/`pageSize` contract),
  create, edit, status-change, delete — all signal-driven, no NgRx.
- `features/profile/profile-page`: displays the current user (`GET /api/auth/me`).
- Routing: `app.routes.ts` lazy-loads all feature routes; `pages/not-found/not-found.ts` for
  unmatched paths.

## 6. Session storage decision
JWTs are stored in `sessionStorage`, not `localStorage` or an `HttpOnly` cookie. Documented in
**ADR-011** (SPA token storage) as a deliberate trade-off for this exercise's scope (simpler than
a cookie + CSRF-token scheme, shorter-lived exposure than `localStorage`, at the cost of losing
the session on tab close and remaining vulnerable to XSS-based token theft same as any JS-readable
storage). **ADR-010** records that a `refresh-token` rotation scheme was considered and explicitly
deferred as out of scope.

## 7. Backend changes (minimized, justified, tested)
Exactly two, both required by real frontend integration — no other backend code was touched:
1. **Development-only CORS policy** added so the Angular dev server (`http://localhost:4200`)
   could call the API (`http://localhost:5xxx`) during local development. Named policy, no
   `AllowAnyOrigin`, no credentials mode, gated to `IsDevelopment()`.
2. **`BadHttpRequestException` → 400, not 500**: `GET /api/tasks`'s `page`/`pageSize` int-bound
   minimal-API parameters had no defaults, so a plain `GET /api/tasks` (which the Angular list
   page issues on first load) threw `BadHttpRequestException` for the missing route/query values,
   which `GlobalExceptionHandler` was mapping to a generic 500. Fixed by giving `page`/`pageSize`
   defaults (`1`/`20`) and teaching `GlobalExceptionHandler` to map `BadHttpRequestException` to a
   400 problem response instead of 500 — a real bug, not a frontend-convenience shortcut.

Both changes are covered by new/updated backend tests (API integration tests for the CORS header
presence in Development and for the corrected 400 response).

## 8. Testing summary (full stack, end of Sprint 3)
140 automated tests total:
- Backend: 88 (Domain 17, Application 29, Architecture 11, Infrastructure.IntegrationTests 11,
  Api.IntegrationTests 20 — 2 more than Sprint 2's 20 count includes the new CORS/400 coverage).
- Frontend: 52 Angular/Vitest tests across auth store/guards/interceptor, feature components, and
  shared utilities.

## 9. Validation performed
| Validation | Result |
|---|---|
| `dotnet test --configuration Release` | ✅ 88/88 passed |
| `dotnet format --verify-no-changes` | ✅ exit 0 |
| `npm test -- --watch=false` (Vitest) | ✅ 52/52 passed |
| `npm run build` (production) | ✅ succeeded (one transient segfault on a prior run did not reproduce; rerun was clean, exit 0, valid `dist/`) |
| `npm audit` | ⚠️ 3 moderate, dev-tooling only (`@hono/node-server` → `@modelcontextprotocol/sdk` → `@angular/cli`); not fixed — resolution would require downgrading the Angular CLI, not warranted |
| 15-step manual API contract walkthrough via `curl` against the real running backend (register → login → me → list → create → list → edit → status-change → refresh-simulation → delete → 404 → invalid-login → second-user → cross-user-isolation → CORS-header-check → anonymous-no-auth-header) | ✅ all 15 steps passed |
| Real browser automation (register/login/create/status-change/edit/delete via an actual browser) | ❌ **not performed** — no browser automation tool (chromium-cli, Playwright, Cypress) was available in this environment, and installing one was out of scope. Reported honestly rather than claimed. |

## 10. Known limitations
- No real-browser end-to-end verification was performed (see above) — the curl-based API contract
  walkthrough is a partial substitute but does not exercise the actual rendered UI, client-side
  routing, or console errors.
- 3 moderate npm audit findings remain (dev tooling only, not runtime-shipped code).
- Refresh-token rotation is out of scope (ADR-010).

## 11. Git status
Still nothing committed anywhere in the project — HEAD remains `d849580` (`Initial commit`). All
Increment 0, Sprint 2, and Sprint 3 work remains in the uncommitted working tree, by the user's
explicit, repeated choice.

## 12. Recommended next steps
1. Configure git identity and make an initial real commit of the full working tree (or a series of
   sprint-scoped commits) once the user is ready.
2. Add real browser-based e2e coverage (Playwright) if/when tooling installation is approved.
3. Consider refresh-token rotation and/or `HttpOnly` cookie storage if this moves beyond an
   exercise toward production use.
