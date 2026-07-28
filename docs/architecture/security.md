# Security overview

The actual security posture of the implementation, grounded in source — every claim below is
verified against code, not asserted. See [ADR-003](../decisions/ADR-003-identity-jwt.md),
[ADR-008](../decisions/ADR-008-cross-user-404.md), and [ADR-011](../decisions/ADR-011-spa-token-storage.md)
for the full reasoning behind each decision.

## Identity

* **Password handling**: never touched directly — ASP.NET Core Identity's `UserManager<ApplicationUser>`
  hashes and verifies passwords; no custom hashing code exists anywhere in this codebase.
* **Registration**: `RegisterUserHandler` validates email/first name/last name before calling
  `IIdentityService.CreateUserAsync`; a duplicate email returns `409 Conflict` with a Problem Details
  body, not a raw exception.
* **Failed login**: `LoginUserHandler` returns the same generic `401` for both "unknown email" and
  "wrong password" — the API never reveals whether an email is registered.
* **Lockout**: `InfrastructureServiceCollectionExtensions.cs` — 5 failed attempts triggers a 5-minute
  lockout (`Lockout.MaxFailedAccessAttempts = 5`, `Lockout.DefaultLockoutTimeSpan =
  TimeSpan.FromMinutes(5)`), Identity's built-in mechanism, not custom code.
* **Identity data exposure**: `UserDto`/`AuthenticatedUser` responses expose only `id`, `email`,
  `firstName`, `lastName` — never `PasswordHash`, `SecurityStamp`, or `ConcurrencyStamp`.

## JWT

* **Claims**: `sub` (user id), `email`, `given_name`, `family_name`, `jti` — no roles/permissions
  claim exists (no role administration is implemented).
* **Signing**: HMAC-SHA256, symmetric key from `Jwt:SigningKey`.
* **Issuer/audience**: both validated (`ValidateIssuer`/`ValidateAudience` = `true`), values from
  configuration.
* **Expiration**: `Jwt:ExpirationMinutes`, default 60; `ValidateLifetime = true`, 30-second clock skew.
* **Secret configuration**: `Jwt:SigningKey` is never committed; bound via `IOptions<JwtOptions>` with
  `.Validate(...).ValidateOnStart()` — the application refuses to start if the key is missing or
  under 32 characters, rather than starting with a weak or absent key.
* **Token storage**: `sessionStorage` on the client — see Angular section below.
* **`401` behavior**: a missing, invalid, or expired token is handled by a `JwtBearerEvents.OnChallenge`
  handler, producing the same Problem Details shape (`application/problem+json`,
  `urn:ballastlane-tasks:error:auth.required`) as every other `401` in the API — an earlier
  implementation fell through to the framework's default `401`, a different body shape than the
  application layer's own `UseCaseError.Unauthorized().ToProblem()`. Covered by
  `AuthEndpointTests.ProtectedEndpoint_WithoutToken_ShouldReturnProblemDetailsBody`.

## Authorization

* **Ownership from the JWT `sub` claim**: every task handler resolves the acting user via
  `ICurrentUser.UserId`, sourced from the validated JWT (`CurrentUser.cs` reads
  `HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)`), never from a request body or route
  parameter.
* **User id never trusted from client input**: no endpoint accepts an `ownerId`/`userId` field on any
  request DTO (`CreateTaskRequest`, `UpdateTaskRequest`, `ChangeTaskStatusRequest`).
* **Repository queries are user-scoped**: `ITaskRepository`'s methods all take an `ownerId` parameter
  supplied by the handler, not the caller; `TaskRepository`'s EF Core queries filter by it directly.
* **Cross-user access returns `404`**: a task belonging to another user is reported identically to a
  task that doesn't exist — deliberately, to avoid revealing which task ids exist to a caller who
  doesn't own them (see [ADR-008](../decisions/ADR-008-cross-user-404.md)).
* **Route guards are not a security boundary**: `authGuard`/`guestGuard` decide what the Angular SPA
  renders; they have no bearing on what the API allows. Calling the API directly with a valid token
  bypasses every guard, because the actual authorization check happens server-side, on every request,
  independent of the client.

## Angular

* **Token attached only to same-origin/configured-API requests**: `authInterceptor` compares
  `new URL(request.url, window.location.origin).origin` against the API's own resolved origin before
  attaching `Authorization`. This replaced an earlier `request.url.startsWith(baseUrl)` check that was
  vacuously `true` for every request when `baseUrl` is `''` (the production, relative-`/api`
  configuration), which would have attached the token to any outgoing HTTP call, including a
  third-party one. Covered by `auth.interceptor.spec.ts`'s test suite for a relative `API_BASE_URL`,
  including a case proving an absolute external URL is still excluded even when `API_BASE_URL` is `''`.
* **No `innerHTML`, no `bypassSecurityTrust*`**: confirmed absent anywhere in
  `client/ballastlane-tasks-web/src/app` — all dynamic content goes through Angular's default output
  sanitization/binding.
* **`sessionStorage` trade-off**: readable by any script on the page — not immune to XSS, only
  shorter-lived than `localStorage`. The actual defense against token theft here is XSS hygiene (no
  unsafe HTML binding), not the storage location itself. See
  [ADR-011](../decisions/ADR-011-spa-token-storage.md) for the full trade-off and the `HttpOnly`
  cookie alternative.

## Known omissions

* **No refresh-token flow** — an expired access token forces re-authentication from scratch, rather
  than a silent-renewal flow. Deliberate scope decision, see
  [ADR-011](../decisions/ADR-011-spa-token-storage.md).
* **No multi-factor authentication.**
* **No password-recovery flow** (no "forgot password" endpoint or email-based reset).
* **No email confirmation.**
* **No role-based access control** — every authenticated user has identical, self-scoped access;
  there is no administrator role.

None of these are implemented, and none are asserted as done elsewhere in this repository.
