# ADR-011 — Store the development SPA access token in sessionStorage

## Status

Accepted for this engineering exercise.

## Context

This ADR supersedes [ADR-010](ADR-010-jwt-storage-deferred.md), which deliberately deferred this
decision during the backend sprint until the Angular auth UI was actually being built.

The Angular SPA needs to hold the JWT returned by `POST /api/auth/login` somewhere between page
loads so a refresh doesn't force a re-login, and needs to attach it to every authenticated request.
The realistic options are `localStorage`, `sessionStorage`, an in-memory-only store, or moving to
`Secure`/`HttpOnly`/`SameSite` cookies issued by the server.

## Decision

**Store the access token (and its `expiresAtUtc`) in `sessionStorage`**, via
`TokenStorageService` (`client/ballastlane-tasks-web/src/app/core/auth/token-storage.service.ts`).

Rationale:

* It survives a browser refresh within the same tab (unlike an in-memory-only store, which would
  force a re-login on every reload — poor UX for a demo exercise).
* It is cleared automatically when the tab/browser session ends — a materially smaller persistence
  window than `localStorage`, which survives indefinitely until explicitly cleared.
* It stays simple: no cookie attributes, no CSRF token dance, no server-side session store — proportional
  to this exercise's scope.
* **Only the token and its expiration are stored — never the full user profile.** `AuthStore`
  re-fetches the profile from `GET /api/auth/me` on initialization rather than trusting a persisted
  object indefinitely, so a revoked/stale token is caught immediately (see
  `AuthStore.initialize()`).

## Security trade-offs (explicitly not glossed over)

* **`sessionStorage` is still readable by any JavaScript running on the page.** It offers no
  protection against XSS: a successful script-injection attack can read the token directly, same as
  `localStorage`. Its only advantage over `localStorage` is a shorter persistence window, not immunity
  to theft. This SPA's real defense against that class of attack is not token storage location — it's
  the usual XSS hygiene (Angular's built-in output sanitization, no unsafe HTML binding, no
  `bypassSecurityTrust*` calls anywhere in this codebase).
* **A production system handling real user data should instead consider `Secure`, `HttpOnly`,
  `SameSite` cookies** issued by the API. `HttpOnly` makes the token unreadable to JavaScript entirely
  (immune to token theft via XSS), at the cost of needing a CSRF mitigation strategy (double-submit
  cookie, `SameSite=Strict`/`Lax`, or a synchronizer token), since the browser now attaches the
  cookie automatically to matching-origin requests.
* **No refresh token exists.** When the JWT expires (`Jwt:ExpirationMinutes`, default 60 — see
  [ADR-003](ADR-003-identity-jwt.md)), the next authenticated request gets a `401`, the
  `httpErrorInterceptor` clears the session and redirects to `/login`. The user re-authenticates from
  scratch. This is acceptable for a demo; a production system would very likely add a refresh-token
  flow — explicitly out of scope for this project (see "Production improvements" in the root
  [README](../../README.md)).
* **Route guards are UX only, not the authorization boundary.** `authGuard`/`guestGuard` decide what
  the SPA *renders* — they have no bearing on what the API actually allows. Every task operation is
  independently authorized server-side against the JWT's `sub` claim (see
  [ADR-008](ADR-008-cross-user-404.md) and `ICurrentUser`); a client-side guard bypass (e.g. calling
  the API directly with curl) gains nothing an attacker couldn't already do by calling the API
  directly with a valid token for their own account.

## Consequences

* No new dependency was needed (`sessionStorage` is a browser built-in) — consistent with the
  "no complex state library, no generic API client" scope constraints for this sprint.
* If a future increment needs "stay signed in across tabs" or "survive closing the browser," that is
  the trigger to revisit this decision — `localStorage` or a refresh-token-backed cookie flow would
  both need re-evaluating the trade-offs above, not just swapping the storage call.
