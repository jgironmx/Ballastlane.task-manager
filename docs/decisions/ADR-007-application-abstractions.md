# ADR-007 — Application abstractions and the omission of IUnitOfWork

## Status

Accepted

## Context

The Application layer needs ports for persistence, identity, token issuance, the current user, and
time — `ITaskRepository`, `IIdentityService`, `ITokenService`, `ICurrentUser`, and `IClock`. A
generic `IUnitOfWork` abstraction is a common companion to a repository port, but should only be
introduced if it adds real coordination value beyond what a single `DbContext` already provides.

## Decision

**Implement `ITaskRepository`, `IIdentityService`, `ITokenService`, `ICurrentUser`, and `IClock`.
Omit `IUnitOfWork`.**

This exercise has exactly one EF Core-backed aggregate (`TaskItem`) and no use case that needs to
coordinate writes across multiple aggregates or repositories in a single transaction. `ITaskRepository`
exposes its own `SaveChangesAsync`, which — in the Infrastructure implementation — simply forwards to
`ApplicationDbContext.SaveChangesAsync`. That `DbContext` instance *is* the transaction boundary: EF
Core already batches all tracked changes into one transaction on `SaveChanges`. A separate
`IUnitOfWork` wrapping the same `DbContext` would only rename `SaveChangesAsync` without adding
coordination that doesn't already exist — pure ceremony for this exercise's scope.

User registration (`IIdentityService.CreateUserAsync`) does not participate in this transaction
boundary either: ASP.NET Core Identity's default `UserStore` auto-saves through its own
`SaveChangesAsync` call inside `UserManager.CreateAsync`. No use case in this exercise needs "create a
user and a task in one atomic transaction," so this split is safe.

**No generic repository.** `ITaskRepository` exposes task-specific, intention-revealing methods
(`GetByIdAsync(taskId, ownerId, ...)`, `ListAsync(ownerId, status, searchText, page, pageSize, ...)`)
rather than generic `Add<T>`/`GetById<T>`/`Query<T>` methods. This keeps ownership scoping
(`ownerId` is a required parameter on every read) impossible to forget, which a generic repository
would not enforce.

**No `IQueryable` crosses the Application boundary.** `ITaskRepository.ListAsync` returns a materialized
`IReadOnlyList<TaskItem>`; filtering, paging, and ordering all happen inside the Infrastructure
implementation against the database, not by leaking a queryable for Application to further compose
(which would blur the Application/Infrastructure boundary and make query behavior harder to test in
isolation).

## Consequences

* If a future increment needs a true cross-aggregate transaction (e.g. "create a user and seed their
  first task atomically" as an interactive use case, not the startup seeder), that is the trigger to
  introduce `IUnitOfWork` — not before.
* `ICurrentUser` and `IClock` are deliberately minimal (two members each): just enough for the
  ownership-scoping and deterministic-timestamp needs described in
  [ADR-006](ADR-006-taskitem-domain-model.md).
* `IIdentityService` and `ITokenService` return `Application`-owned types (`Result<IdentityUserInfo>`,
  `IdentityUserInfo`, `AccessToken`) — never ASP.NET Core Identity or JWT-library types — keeping
  `Application` and its unit tests free of a Microsoft.AspNetCore.Identity/JWT package reference (see
  [ADR-003](ADR-003-identity-jwt.md), enforced by the architecture tests).
