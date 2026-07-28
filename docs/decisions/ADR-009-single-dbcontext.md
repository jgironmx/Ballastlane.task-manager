# ADR-009 — Single DbContext for Identity and application data

## Status

Accepted

## Context

The application needs both ASP.NET Core Identity's tables (`AspNetUsers`, `AspNetRoles`, etc.) and
its own `Tasks` table. EF Core supports either one `DbContext` covering both, or two separate
contexts (an Identity-owned one and an application-owned one) — sometimes on separate databases.

## Decision

**One `ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`**, adding a
single `DbSet<TaskItem> Tasks`, covers both Identity and application data in one database and one
migration history.

This was chosen over two contexts because:

* `TaskItem.OwnerId` has a real foreign key to `AspNetUsers.Id` (see
  [E4](../../src/Ballastlane.Tasks.Infrastructure/Persistence/Configurations/TaskItemConfiguration.cs)),
  with cascade delete — a user's tasks are deleted when the user is. Cross-database (or even
  cross-context, same-database) foreign keys are not something EF Core's migrations model natively;
  keeping both in one context/database keeps that constraint simple and enforced by SQL Server itself,
  not application code.
* The exercise's scope (one user table, one task table) does not need Identity and application data
  to scale, deploy, or migrate independently — the two-context split exists in larger systems to
  decouple exactly those concerns, which don't apply here.
* One migration history means "apply migrations" is a single `dotnet ef database update` — see the
  root [README](../../README.md) — rather than a coordinated multi-context migration sequence.

## Consequences

* `Ballastlane.Tasks.Infrastructure` has exactly one migrations folder
  (`Persistence/Migrations`) and one design-time model.
* If Identity data volume or lifecycle ever needs to diverge sharply from task data (e.g. a
  centralized auth service shared across multiple applications), that is the trigger to split into
  separate contexts/databases — not needed for this exercise.
* Domain still never references `ApplicationUser` — the FK is configured entirely in
  `TaskItemConfiguration` via a shadow relationship (see [ADR-003](ADR-003-identity-jwt.md)), so this
  single-context choice does not weaken the Domain/Infrastructure boundary.
