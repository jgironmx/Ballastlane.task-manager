# Original Prompt — Sprint 2: Complete Backend Implementation

This file preserves the prompt provided for Sprint 2.

[`docs/reports/final-report-2-sprint-2-backend.md`](final-report-2-sprint-2-backend.md), which
reports what was actually built in response to it. 

See also the companion prompts for
[Increment 0](prompt-1-increment-0.md) and
[Sprint 3](prompt-3-sprint-3-angular.md).

```text
# Sprint 2 — Complete Backend Implementation

You are the implementation engineer for the Ballastlane.Tasks architecture exercise.

The repository already contains the architectural baseline:

* .NET 10
* Clean Architecture
* Domain, Application, Infrastructure, and API projects
* Backend test projects
* Architecture tests
* Angular 22 workspace shell
* Shared build and package configuration
* CI
* ADRs and architecture documentation

Your task is to first close the minor baseline corrections and then implement the complete backend required by the exercise.

Do not implement Angular functionality in this sprint.

---

# Part A — Close Increment 0

Before implementing backend functionality, inspect the current repository and complete these actions.

## A1. Add missing architecture rule

Add an architecture test proving:

```text
Infrastructure must not depend on API.
```

The test must be meaningful and non-vacuous.

Expected architecture direction:

```text
Application → Domain

Infrastructure → Application
Infrastructure → Domain

API → Application
API → Infrastructure
```

Forbidden production references:

```text
Domain → Application
Domain → Infrastructure
Domain → API

Application → Infrastructure
Application → API

Infrastructure → API
```

## A2. Verify project references

Run:

```bash
dotnet list src/Ballastlane.Tasks.Application reference
dotnet list src/Ballastlane.Tasks.Infrastructure reference
dotnet list src/Ballastlane.Tasks.Api reference
```

Report the actual references.

Correct any deviation from the expected dependency graph.

## A3. Verify .NET SDK selection

Ensure the repository has a stable `.NET 10` `global.json`.

Requirements:

* Stable SDK only.
* No preview SDK.
* `allowPrerelease` must be false.
* The version and roll-forward policy must work locally and in CI.

Run:

```bash
dotnet --version
dotnet --info
```

Update the README prerequisites with the actual supported SDK strategy.

## A4. Verify package health

Run:

```bash
dotnet list package --include-transitive
dotnet list package --vulnerable --include-transitive
```

Confirm that the explicit `Microsoft.OpenApi` pin remains necessary and compatible.

Do not upgrade unrelated packages merely because a newer version exists.

Document any intentional package pin.

## A5. Verify Angular environment

Run:

```bash
node --version
npm --version
npx ng version
```

Document the versions used and the supported Node requirement in the Angular README or root README.

---

# Part B — Backend objective

Implement a complete ASP.NET Core backend for a personal task-management system.

The backend must satisfy:

* User creation.
* User login.
* ASP.NET Core Identity.
* JWT bearer authentication.
* One anonymous endpoint.
* Protected endpoints.
* SQL Server persistence.
* Task CRUD.
* Per-user task ownership.
* Business validation.
* Seeded demo user and tasks.
* Unit and integration tests.
* OpenAPI documentation.
* Clean Architecture boundaries.

Do not introduce unnecessary enterprise frameworks.

---

# Part C — Domain model

Implement the domain model in:

```text
Ballastlane.Tasks.Domain
```

## C1. TaskItem

Create a `TaskItem` aggregate or entity.

Recommended data:

```text
Id: Guid
OwnerId: Guid
Title: string
Description: string?
Status: TaskItemStatus
DueDate: DateOnly? or DateTimeOffset?
CreatedAtUtc: DateTimeOffset
UpdatedAtUtc: DateTimeOffset?
```

Use either `DateOnly?` for a date-only business due date or `DateTimeOffset?` if the existing architectural conventions require a timestamp.

Choose one and document the decision.

Avoid naming the entity `Task`.

## C2. Task status

Use a small explicit status enum:

```text
Pending
InProgress
Completed
```

Do not add unnecessary statuses.

## C3. Domain invariants

Enforce these rules:

1. Owner ID cannot be empty.
2. Title is required.
3. Title cannot exceed 200 characters.
4. Description cannot exceed 2,000 characters.
5. New tasks begin as `Pending`.
6. Created timestamp is assigned by the application using an abstract clock.
7. Due date cannot be earlier than the current business date when creating a task.
8. A task may be updated only through explicit methods.
9. Status changes must use an explicit domain method.
10. Task ownership cannot be changed after creation.

Keep HTTP, Identity, EF Core, and validation-framework dependencies out of Domain.

## C4. Domain tests

Use TDD-style tests for:

* Valid task creation.
* Empty title rejection.
* Whitespace-only title rejection.
* Title length rejection.
* Description length rejection.
* Empty owner rejection.
* Past due-date rejection.
* Initial status is Pending.
* Valid updates.
* Status changes.
* Ownership remains immutable.

Do not write tests only for property getters.

---

# Part D — Application layer

Implement use cases in:

```text
Ballastlane.Tasks.Application
```

Use a feature-oriented organization.

Example:

```text
Features/
  Authentication/
    Register/
    Login/
    GetCurrentUser/

  Tasks/
    Create/
    GetById/
    List/
    Update/
    ChangeStatus/
    Delete/
```

Do not add MediatR unless it already exists and is clearly justified.

Prefer explicit use-case classes or handlers.

## D1. Application abstractions

Create only abstractions that are needed:

```csharp
ITaskRepository
IIdentityService
ITokenService
ICurrentUser
IClock
IUnitOfWork
```

If `IUnitOfWork` adds no real value because the DbContext already represents the transaction boundary, it may be omitted. Document that choice.

Do not create a generic repository.

Do not expose `IQueryable`.

## D2. Authentication use cases

Implement:

```text
RegisterUser
LoginUser
GetCurrentUser
```

Requirements:

### Register

Input:

```text
Email
Password
FirstName
LastName
```

Rules:

* Email required.
* Valid email format.
* Unique email.
* First and last name required.
* Password requirements are delegated to Identity.
* Return a safe error structure.
* Do not expose Identity internals.

### Login

Input:

```text
Email
Password
```

Rules:

* Invalid login must return a generic authentication failure.
* Do not reveal whether the email exists.
* Generate a JWT for successful login.
* Return user summary and token metadata.

### Current user

Return:

```text
Id
Email
FirstName
LastName
```

Do not expose password hash, security stamp, concurrency stamp, or other Identity internals.

## D3. Task use cases

Implement:

```text
CreateTask
GetTaskById
ListTasks
UpdateTask
ChangeTaskStatus
DeleteTask
```

Rules:

* All operations require an authenticated user.
* Owner ID comes only from `ICurrentUser`.
* Owner ID must never come from the HTTP request.
* Every read, update, and delete must be scoped to the current user.
* A user cannot discover or mutate another user's task.
* Use asynchronous methods.
* Pass `CancellationToken`.
* Return application DTOs, not domain or EF entities.

## D4. List behavior

Keep listing simple.

Support optional filters only when low-cost:

```text
Status
Search text
```

Do not implement advanced pagination unless required for a clean API contract.

A simple paginated result is acceptable if implemented consistently.

Avoid overengineering sorting and specifications.

## D5. Application tests

Mock application abstractions and test:

### Authentication

* Registration success.
* Duplicate user failure.
* Identity validation errors are mapped safely.
* Login success.
* Invalid login returns generic failure.
* Current user retrieval.

### Tasks

* Valid creation.
* Current user ID is assigned.
* Initial status is Pending.
* Invalid title is rejected.
* Past due date is rejected.
* Current user can get own task.
* Another user's task is not returned.
* Current user can update own task.
* Another user cannot update task.
* Current user can delete own task.
* Another user cannot delete task.
* Status change works.
* Unknown task returns not found.

Focus on behavior, not mock call counts alone.

---

# Part E — Infrastructure

Implement infrastructure in:

```text
Ballastlane.Tasks.Infrastructure
```

## E1. SQL Server and Entity Framework Core

Use:

```text
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Design
```

Use stable .NET 10-compatible package versions.

Create:

```text
ApplicationDbContext
TaskItemConfiguration
TaskRepository
Design-time DbContext factory, if needed
Initial migration
```

## E2. ASP.NET Core Identity

Use:

```csharp
ApplicationUser : IdentityUser<Guid>
ApplicationRole : IdentityRole<Guid>
```

Recommended user fields:

```text
FirstName
LastName
CreatedAtUtc
```

The Identity user belongs in Infrastructure.

Domain must not reference `ApplicationUser`.

## E3. DbContext

Use one context inheriting from:

```csharp
IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
```

Add:

```csharp
DbSet<TaskItem>
```

Call:

```csharp
base.OnModelCreating(builder);
```

Apply entity configurations explicitly or by assembly scanning.

## E4. Task mapping

Configure:

* Table name.
* Primary key.
* Required title.
* Maximum lengths.
* Status conversion.
* Owner foreign key to Identity user.
* Index on owner ID.
* Useful composite index such as owner and status.
* UTC timestamp persistence.
* Appropriate delete behavior.

Do not add navigation from Domain to `ApplicationUser`.

Configure the relationship from Infrastructure.

## E5. Repository behavior

Repository methods should be user-scoped where applicable.

Example:

```csharp
Task<TaskItem?> GetByIdAsync(
    Guid taskId,
    Guid ownerId,
    CancellationToken cancellationToken);
```

List queries must filter by owner at the database level.

Do not load all tasks and filter in memory.

Do not return `IQueryable`.

## E6. Identity service

Implement `IIdentityService` using:

```text
UserManager<ApplicationUser>
SignInManager<ApplicationUser>
```

Use Identity for:

* Password hashing.
* User creation.
* Password verification.
* Email uniqueness.
* Lockout configuration.
* User retrieval.

Do not implement custom password hashing.

## E7. JWT token service

Implement `ITokenService`.

JWT should contain:

```text
sub: user ID
email
given_name
family_name
jti
```

Configuration must include:

```text
Issuer
Audience
SigningKey
ExpirationMinutes
```

Requirements:

* Signing key must not be committed.
* Development secrets should use user secrets or environment variables.
* README may show a clearly fake development placeholder.
* Validate issuer.
* Validate audience.
* Validate lifetime.
* Validate signing key.
* Use UTC.
* Do not log tokens.

## E8. Current user service

Implement `ICurrentUser` using `IHttpContextAccessor` in Infrastructure or API composition.

It should provide:

```text
IsAuthenticated
UserId
```

User ID must come from the `sub` claim.

Handle invalid or absent user IDs safely.

## E9. Clock

Implement:

```text
SystemClock
```

The application and domain operations must not call `DateTime.UtcNow` or `DateTimeOffset.UtcNow` directly where deterministic testing matters.

---

# Part F — API

Implement API endpoints in:

```text
Ballastlane.Tasks.Api
```

Controllers must remain thin.

Use explicit request and response contracts.

## F1. Authentication endpoints

Implement:

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

Expected behavior:

### Register

* `201 Created` or `200 OK`.
* Choose one contract and document it.
* Return safe user data.
* Do not automatically issue a token unless deliberately chosen and documented.

### Login

* `200 OK`.
* Return JWT and user summary.
* Invalid credentials return `401 Unauthorized`.
* Do not reveal whether the user exists.

### Me

* Requires authentication.
* Returns current user profile.
* Anonymous request returns `401`.

## F2. Public endpoint

Keep:

```http
GET /health
```

Anonymous and returning `200`.

## F3. Task endpoints

Implement:

```http
GET    /api/tasks
GET    /api/tasks/{id}
POST   /api/tasks
PUT    /api/tasks/{id}
PATCH  /api/tasks/{id}/status
DELETE /api/tasks/{id}
```

Expected responses:

```text
GET list:
200 OK

GET by ID:
200 OK
404 Not Found

POST:
201 Created
Location header
Created representation

PUT:
200 OK or 204 No Content
Choose and document consistently

PATCH status:
200 OK or 204 No Content
Choose and document consistently

DELETE:
204 No Content

Invalid input:
400 Bad Request

Anonymous request:
401 Unauthorized
```

For resources belonging to another user, prefer returning `404` rather than revealing existence with `403`.

Document this security decision.

## F4. API contracts

Request models must not include:

```text
OwnerId
UserId
CreatedAtUtc
UpdatedAtUtc
```

Create request:

```text
Title
Description
DueDate
```

Update request:

```text
Title
Description
DueDate
```

Status request:

```text
Status
```

## F5. Problem Details

Use ASP.NET Core Problem Details.

Implement centralized handling for:

```text
Validation failure
Not found
Authentication failure
Conflict
Unexpected exception
```

Do not expose stack traces or internal exception messages in production responses.

Use a consistent error contract.

## F6. OpenAPI

Configure OpenAPI/Swagger in Development.

Document:

* JWT bearer scheme.
* Protected endpoints.
* Request models.
* Response status codes.

The user should be able to authenticate through Swagger.

---

# Part G — Validation

Use one clear validation strategy.

You may use:

* Explicit validators in Application, or
* FluentValidation if its addition is justified and remains limited.

Do not duplicate the same rules inconsistently in Domain, Application, and API.

Recommended division:

```text
API:
Transport/model binding validation.

Application:
Use-case validation.

Domain:
Business invariants that must always hold.
```

Ensure validation errors map to Problem Details.

---

# Part H — Seed data

Create an idempotent development seeder.

Seed this demo user:

```text
Email: demo@ballastlane.local
Password: Demo1234!
First name: Demo
Last name: User
```

Use `UserManager` to create the user.

Never hard-code a password hash.

Seed at least four tasks:

1. Pending task.
2. In-progress task.
3. Completed task.
4. Task with a future due date.

Do not seed an invalid past-due task if the domain disallows creation of past-due tasks.

If an overdue example is desired, create it using a documented seed-only mechanism or omit it.

The seeder must not duplicate data on repeated startup.

Run it only in Development unless explicitly configured.

---

# Part I — Configuration

Add configuration for:

```text
ConnectionStrings:DefaultConnection
Jwt:Issuer
Jwt:Audience
Jwt:SigningKey
Jwt:ExpirationMinutes
```

For LocalDB, use a development connection string similar to:

```text
Server=(localdb)\MSSQLLocalDB;
Database=BallastlaneTasksDb;
Trusted_Connection=True;
MultipleActiveResultSets=true;
TrustServerCertificate=True
```

Do not commit real secrets.

Provide safe instructions for:

```bash
dotnet user-secrets set
```

Add Docker SQL Server configuration only when it can be done cleanly without distracting from the sprint.

LocalDB is the primary local setup.

---

# Part J — Integration tests

## J1. Infrastructure integration tests

Prefer real SQL Server behavior.

Use one of:

1. SQL Server Testcontainers, preferred when Docker is available.
2. LocalDB test database as a documented fallback.

Do not use EF Core InMemory as the only persistence validation.

Test:

* Migration/database creation.
* User creation with Identity.
* Task persistence.
* Task retrieval by owner.
* Cross-user task isolation.
* Update.
* Delete.
* Unique email behavior.

If Docker is unavailable, report the environment limitation honestly.

Do not silently replace SQL Server tests with SQLite.

## J2. API integration tests

Use:

```csharp
WebApplicationFactory<Program>
```

Test at least:

### Public/authentication

* Health returns `200`.
* Protected endpoint without JWT returns `401`.
* Register succeeds.
* Duplicate registration fails safely.
* Login succeeds.
* Invalid login returns `401`.
* Current user endpoint works.

### Tasks

* Authenticated task creation returns `201`.
* Created response has correct owner-independent DTO.
* List returns the current user's tasks.
* Get returns own task.
* Another user cannot access the task.
* Update works.
* Invalid update returns `400`.
* Status change works.
* Delete returns `204`.
* Deleted task returns `404`.

Do not mock away the complete authentication pipeline in every API test.

Use real JWT generation for representative integration tests.

---

# Part K — Documentation

Update:

```text
README.md
docs/architecture/solution-overview.md
docs/decisions/
docs/ai/
docs/presentation/outline.md
```

README must include:

* Backend implemented status.
* Prerequisites.
* LocalDB setup.
* Connection-string configuration.
* User-secrets commands.
* Migration command.
* Database update command.
* API startup.
* Swagger URL.
* Demo credentials.
* Test commands.
* Current Angular status.
* API endpoints.
* Security notes.
* Known limitations.

Create or update ADRs when decisions differ from the baseline.

Add an ADR or decision note for:

```text
DateOnly versus DateTimeOffset due dates
404 for cross-user resources
Single Identity/application DbContext
JWT storage deferred to frontend sprint
```

---

# Part L — Scope restrictions

Do not implement:

* Angular authentication screens.
* Angular task CRUD.
* Refresh tokens.
* Email confirmation workflows.
* Password reset UI.
* Multi-factor authentication.
* External identity providers.
* Role administration.
* Microservices.
* Message queues.
* Event sourcing.
* Redis.
* NgRx.
* SignalR.
* Generic repositories.
* MediatR unless already present and justified.
* AutoMapper unless already present and justified.
* Complex domain events.
* Full observability platform.

Keep the project exercise-sized.

---

# Part M — Git strategy

Part A must be committed separately as:

```text
chore: establish clean architecture solution baseline
```

Backend implementation should remain uncommitted until validation and architectural review are complete.

Do not automatically commit Part B through Part L.

At the end, report the recommended backend commit message:

```text
feat: implement authenticated task management backend
```

---

# Part N — Required validation

Run:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet format --verify-no-changes
git diff --check
git status --short
git diff --stat
```

Run package validation:

```bash
dotnet list package --vulnerable --include-transitive
```

Run database validation:

```bash
dotnet ef migrations list
dotnet ef database update
```

Run the API and manually validate:

```text
GET /health
POST /api/auth/register
POST /api/auth/login
GET /api/auth/me
POST /api/tasks
GET /api/tasks
GET /api/tasks/{id}
PUT /api/tasks/{id}
PATCH /api/tasks/{id}/status
DELETE /api/tasks/{id}
```

Validate Swagger authentication.

If Docker-based integration tests are implemented, run them and report whether Docker was available.

---

# Part O — Final report

Return:

## 1. Executive summary

What was implemented and what remains.

## 2. Baseline corrections

* Architecture test added.
* References verified.
* SDK verified.
* Packages verified.
* Angular environment verified.
* Baseline commit hash.

## 3. Architectural decisions

Explain:

* Domain model.
* Due-date type.
* Identity placement.
* JWT design.
* Repository design.
* Ownership enforcement.
* Error handling.
* Cross-user `404` decision.
* Seeder design.

## 4. Files created and modified

Group by project.

## 5. Database

* Context.
* Entities.
* Identity tables.
* Migration.
* Indexes.
* LocalDB configuration.
* Seeder.

## 6. Endpoints

List methods, routes, authentication requirement, and status codes.

## 7. Tests

Report test counts by project and category.

## 8. Validation results

Include exact command outcomes.

## 9. Warnings and vulnerabilities

Report honestly.

## 10. Deviations

List every deviation from this prompt.

## 11. Git status

Include:

```bash
git status --short
git diff --stat
```

## 12. Risks and known limitations

Include only real limitations.

## 13. Recommended next sprint

The next sprint should be:

```text
Sprint 4 — Quality hardening, documentation, and delivery readiness
```

Do not claim success unless build, tests, migrations, and relevant manual checks actually passed.
```
