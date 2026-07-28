# ADR-003 — ASP.NET Core Identity and JWT

## Status

Accepted — implemented.

## Context

Registered users must be able to authenticate and manage only their own tasks. The stack specifies
ASP.NET Core Identity for user/credential management and JWT bearer tokens for authenticating the
Angular SPA against the API.

## Decision

* **User management:** ASP.NET Core Identity (`UserManager<ApplicationUser>`, password hashing, user
  store, lockout) owns user accounts and credentials. `ApplicationUser : IdentityUser<Guid>` and
  `ApplicationRole : IdentityRole<Guid>` live in `Ballastlane.Tasks.Infrastructure.Identity`.
* **Authentication transport:** JWT bearer tokens. The Angular SPA authenticates once (`POST
  /api/auth/login`) and sends the resulting token as an `Authorization: Bearer <token>` header on
  subsequent API calls; the API validates it via `Microsoft.AspNetCore.Authentication.JwtBearer`.
* **Layering:** the Identity implementation lives entirely in `Ballastlane.Tasks.Infrastructure`
  (`IdentityService : IIdentityService`, `TokenService : ITokenService`).
  `Ballastlane.Tasks.Application` depends only on its own abstractions (`IIdentityService`,
  `ITokenService`, `ICurrentUser`) — it never references `Microsoft.AspNetCore.Identity` types
  directly, enforced by an architecture test. This keeps use cases testable without spinning up
  Identity's infrastructure.
* **`SignInManager<TUser>` is deliberately not used**, even though it is the more commonly documented
  Identity entry point for login. `SignInManager` depends on ASP.NET Core's shared-framework HTTP and
  authentication-scheme types (`IHttpContextAccessor`, `IAuthenticationSchemeProvider`), which are not
  published as a standalone NuGet package for .NET 5+ — using it from
  `Ballastlane.Tasks.Infrastructure` (a plain class library, by design framework-isolated per
  [ADR-001](ADR-001-clean-architecture-modular-monolith.md)) would require adding a
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to a project that otherwise has no web
  dependency. Instead, `IdentityService.ValidateCredentialsAsync` uses
  `UserManager.CheckPasswordAsync` plus `UserManager`'s own lockout members
  (`IsLockedOutAsync`/`AccessFailedAsync`/`ResetAccessFailedCountAsync`), which reproduce the same
  password-check-plus-lockout behavior without the extra framework dependency.
* **`ICurrentUser` is implemented in the API project**, not Infrastructure, for the same reason:
  reading the authenticated user's claims from `IHttpContextAccessor`/`HttpContext.User` needs the
  ASP.NET Core web framework, which the `Api` project already has natively via
  `Microsoft.NET.Sdk.Web`. The interface itself still lives in `Application`.

## Security and testing implications

* **Security:** JWT signing keys and other Identity-related secrets are never committed to source
  control (see the root [README](../../README.md)); they are supplied via `dotnet user-secrets` in
  development. Tokens carry `sub` (user id), `email`, `given_name`, `family_name`, and `jti` claims,
  are signed with HMAC-SHA256, and expire after `Jwt:ExpirationMinutes` (default 60). Refresh tokens
  are explicitly out of scope for this project (see [ADR-011](ADR-011-spa-token-storage.md) for the
  full trade-off) — the SPA re-authenticates from scratch after expiry.
* **Testing:** because `Application` depends on abstractions rather than `UserManager`, use cases are
  unit-tested with hand-written in-memory fakes for `IIdentityService`/`ITokenService`/`ICurrentUser`
  (see [ADR-005](ADR-005-testing-strategy.md)), without a database or Identity's stores. API
  integration tests exercise the real JWT-issuing and validation pipeline end-to-end using
  `WebApplicationFactory<Program>` and real tokens (no mocked authentication middleware).

## Consequences

* `Ballastlane.Tasks.Infrastructure` has no dependency on the ASP.NET Core shared framework at all —
  it remains a plain, portable class library, consistent with the framework-isolation principle in
  [ADR-001](ADR-001-clean-architecture-modular-monolith.md).
* `Application`'s authorization rules are expressed against `ICurrentUser` (an abstraction), keeping
  business rules independent of the specific authentication mechanism.
* If a future increment needs cookie-based or external-provider sign-in, that is the trigger to
  introduce `SignInManager` (and the accompanying `FrameworkReference`) — not before.
