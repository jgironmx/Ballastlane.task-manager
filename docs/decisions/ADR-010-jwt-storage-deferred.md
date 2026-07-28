# ADR-010 — JWT storage on the client is deferred to the Angular sprint

## Status

Superseded by [ADR-011](ADR-011-spa-token-storage.md) — Sprint 3 made and recorded the actual
client-side token storage decision (`sessionStorage`). This ADR's deferral is preserved as a record
of the reasoning at the time; it no longer describes current behavior.

## Context

This sprint (Sprint 2) implements the backend: `POST /api/auth/login` issues a JWT, and protected
endpoints validate it via the `Authorization: Bearer <token>` header. It does **not** implement any
Angular authentication UI or HTTP client wiring (see Part L, scope restrictions, and
[ADR-004](ADR-004-angular-spa.md)). That leaves an open question this ADR intentionally does *not*
answer yet: where and how the Angular SPA stores the token between page loads, and how it attaches it
to outgoing requests.

## Decision

**Defer the client-side token storage/attachment decision to the Angular authentication sprint
(Sprint 3).** This sprint only guarantees the server-side contract: a bearer token with `sub`,
`email`, `given_name`, `family_name`, and `jti` claims, a `Jwt:ExpirationMinutes`-bounded lifetime,
and standard `Authorization: Bearer` validation. Nothing about the response shape
(`{ user, accessToken, tokenType, expiresAtUtc }` — see `LoginResponse`) presumes a particular client
storage mechanism, so the decision can be made with full context once the Angular auth UI and HTTP
interceptor (already planned in [ADR-004](ADR-004-angular-spa.md)) are actually being built —
weighing `localStorage`/`sessionStorage` (simple, but readable by any script — XSS-exposed) against an
in-memory-only store refreshed via a silent re-auth flow (safer, more complex) is a frontend-sprint
concern, not a backend one.

## Consequences

* No refresh-token endpoint, silent-renewal flow, or client storage code exists yet — deliberately
  out of scope for this sprint (see Part L).
* The Angular sprint must pick a storage strategy before implementing the HTTP interceptor; this ADR
  should be updated (or superseded) once that decision is made.
* Because the backend doesn't presume a storage mechanism, this choice can change later without any
  backend contract change.
