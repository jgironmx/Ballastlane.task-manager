# Original Prompt — Increment 0: Repository and Architectural Baseline

This file preserves the prompt provided to Claude Code for Increment 0.
[`docs/reports/final-report-1-increment-0.md`](final-report-1-increment-0.md), which reports what
was actually built in response to it. 

See also the companion prompts for
[Sprint 2](prompt-2-sprint-2-backend.md) and
[Sprint 3](prompt-3-sprint-3-angular.md).

```text
# Increment 0 — Repository and Architectural Baseline

You are working as the implementation engineer for a architecture exercise.

The solution will eventually be a full-stack task-management application using:

* ASP.NET Core Web API
* Angular
* SQL Server
* SQL Server LocalDB for local development
* SQL Server container for portable and integration-test environments
* Entity Framework Core
* ASP.NET Core Identity
* JWT bearer authentication
* Clean Architecture
* Test-Driven Development
* xUnit
* FluentAssertions
* Architecture tests
* API integration tests
* Angular tests

For this increment, create only the repository foundation, project structure, architecture documentation, dependency rules, and build-quality configuration.

Do not implement business functionality yet.

---

## 1. Objective

Create a clean, buildable, testable solution baseline that establishes:

1. Project boundaries.
2. Dependency direction.
3. Naming conventions.
4. Shared build configuration.
5. Testing projects.
6. Architectural documentation.
7. Local development documentation.
8. Initial Git and editor configuration.
9. Quality gates that later increments must respect.

The result must compile and all tests must pass, even if the test projects initially contain only architecture or placeholder smoke tests.

---

## 2. Application name

Use the following root name unless the repository already has an established name:

```text
Ballastlane.Tasks
```

Create:

```text
Ballastlane.Tasks.sln
```

Do not rename an existing repository or root folder unless necessary.

---

## 3. Target framework

Use:

```text
.NET 10
```

Reason:

* It is an LTS release.
* It reduces environment compatibility risk.
* It supports ASP.NET Core Identity, EF Core, Web API, OpenAPI, and modern C#.

Use the latest compatible patch versions available in the current environment.

Do not upgrade to a preview framework.

---

## 4. Required repository structure

Create the following structure:

```text
src/
  Ballastlane.Tasks.Domain/
  Ballastlane.Tasks.Application/
  Ballastlane.Tasks.Infrastructure/
  Ballastlane.Tasks.Api/

tests/
  Ballastlane.Tasks.Domain.Tests/
  Ballastlane.Tasks.Application.Tests/
  Ballastlane.Tasks.Infrastructure.IntegrationTests/
  Ballastlane.Tasks.Api.IntegrationTests/
  Ballastlane.Tasks.ArchitectureTests/

client/
  ballastlane-tasks-web/

docs/
  architecture/
  decisions/
  ai/
  presentation/

scripts/
```

Do not create actual Angular application code unless Angular tooling is already configured and initialization can be done without introducing application functionality.

If Angular CLI is available, initialize only the Angular workspace and baseline application shell.

If Angular CLI is unavailable, create the `client/ballastlane-tasks-web/README.md` placeholder documenting the intended Angular setup command for a later increment.

Do not install global tools automatically.

---

## 5. Backend projects

Create these projects.

### Domain

```text
Ballastlane.Tasks.Domain
```

Project type:

```text
Class Library
```

Responsibilities:

* Entities
* Value objects
* Domain enums
* Domain services when justified
* Domain exceptions or domain result types
* Business invariants

Restrictions:

* Must not reference ASP.NET Core.
* Must not reference Entity Framework Core.
* Must not reference ASP.NET Core Identity.
* Must not reference Infrastructure.
* Must not reference API.
* Must not reference Application.

Do not add domain entities in this increment.

---

### Application

```text
Ballastlane.Tasks.Application
```

Project type:

```text
Class Library
```

Responsibilities:

* Use cases
* Commands and queries
* Application DTOs
* Ports and abstractions
* Validation orchestration
* Authorization rules based on abstract current-user information
* Transaction boundaries through abstractions

Allowed dependency:

```text
Application → Domain
```

Restrictions:

* Must not reference Infrastructure.
* Must not reference API.
* Must not reference Entity Framework Core.
* Must not reference ASP.NET Core Identity.
* Must not reference HTTP-specific types.

Do not implement use cases in this increment.

---

### Infrastructure

```text
Ballastlane.Tasks.Infrastructure
```

Project type:

```text
Class Library
```

Future responsibilities:

* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* JWT token generation
* Repository implementations
* Database migrations
* Seed data
* External service adapters

Allowed dependencies:

```text
Infrastructure → Application
Infrastructure → Domain
```

Do not install EF Core, Identity, or JWT packages in this increment unless they are required only to prepare a minimal dependency-registration shell.

Preferred approach:

* Do not add those packages yet.
* Add them in the specific infrastructure increments where they are used.

---

### API

```text
Ballastlane.Tasks.Api
```

Project type:

```text
ASP.NET Core Web API
```

Future responsibilities:

* HTTP endpoints
* API contracts
* Authentication middleware
* Authorization policies
* Problem Details
* OpenAPI
* Dependency-injection composition
* Request/response mapping

Allowed dependencies:

```text
API → Application
API → Infrastructure
```

The API may act as the composition root.

Keep only the default health or weather placeholder if needed for compilation, but prefer removing generated sample functionality.

Do not implement task controllers or authentication controllers in this increment.

Create one minimal endpoint:

```http
GET /health
```

Expected response:

```json
{
  "status": "Healthy"
}
```

This endpoint must remain anonymous.

It may use a minimal API mapping or a small controller, but do not introduce a business layer for it.

---

## 6. Test projects

Create the following test projects using xUnit.

### Domain tests

```text
Ballastlane.Tasks.Domain.Tests
```

Reference:

```text
Ballastlane.Tasks.Domain
```

Add one simple assembly smoke test or placeholder test proving the project runs.

Do not test nonexistent business behavior.

---

### Application tests

```text
Ballastlane.Tasks.Application.Tests
```

References:

```text
Ballastlane.Tasks.Application
Ballastlane.Tasks.Domain
```

Add one smoke test only.

---

### Infrastructure integration tests

```text
Ballastlane.Tasks.Infrastructure.IntegrationTests
```

References:

```text
Ballastlane.Tasks.Infrastructure
Ballastlane.Tasks.Application
Ballastlane.Tasks.Domain
```

Add one placeholder integration-test category test.

Do not add Testcontainers or SQL Server setup yet.

Clearly mark the test using a trait such as:

```csharp
[Trait("Category", "Integration")]
```

---

### API integration tests

```text
Ballastlane.Tasks.Api.IntegrationTests
```

References:

```text
Ballastlane.Tasks.Api
Ballastlane.Tasks.Application
Ballastlane.Tasks.Infrastructure
```

Add:

```text
Microsoft.AspNetCore.Mvc.Testing
```

Create an integration test using:

```csharp
WebApplicationFactory<Program>
```

Test:

```http
GET /health
```

Validate:

* HTTP 200.
* Response content indicates `Healthy`.

Expose `Program` for integration testing if necessary.

---

### Architecture tests

```text
Ballastlane.Tasks.ArchitectureTests
```

Use either:

* NetArchTest.Rules
* ArchUnitNET

Choose one and explain the choice in documentation.

Add architecture tests proving:

1. Domain does not depend on Application.
2. Domain does not depend on Infrastructure.
3. Domain does not depend on API.
4. Application does not depend on Infrastructure.
5. Application does not depend on API.
6. Domain does not depend on Entity Framework Core.
7. Domain does not depend on ASP.NET Core.
8. Application does not depend on Entity Framework Core.
9. Application does not depend on ASP.NET Core Identity.

Architecture tests must be meaningful and must fail if a forbidden dependency is introduced.

Do not create tests that always pass because no types were discovered.

Ensure each target assembly contains at least one marker type so architecture tests can reliably load and inspect it.

Suggested marker types:

```text
DomainAssemblyMarker
ApplicationAssemblyMarker
InfrastructureAssemblyMarker
ApiAssemblyMarker
```

Keep them internal or public according to what the selected architecture library requires.

---

## 7. Project references

Configure exactly these production references:

```text
Application → Domain

Infrastructure → Application
Infrastructure → Domain

API → Application
API → Infrastructure
```

Do not add:

```text
Domain → anything
Application → Infrastructure
Application → API
Infrastructure → API
```

Tests may reference the projects they test.

---

## 8. Shared build configuration

Create:

```text
Directory.Build.props
```

Configure:

```xml
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
<AnalysisLevel>latest-recommended</AnalysisLevel>
<Deterministic>true</Deterministic>
```

Apply settings carefully so generated code or known third-party analyzer warnings do not make the baseline impossible to build.

Do not suppress warnings globally without explanation.

If a warning must be suppressed:

1. Suppress it as narrowly as possible.
2. Document the reason.
3. Do not suppress correctness or nullability warnings merely to make the build pass.

Create:

```text
Directory.Packages.props
```

Enable central package management:

```xml
<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
```

Centralize versions for all NuGet packages added in this increment.

Suggested packages:

```text
Microsoft.NET.Test.Sdk
xunit
xunit.runner.visualstudio
coverlet.collector
FluentAssertions
Microsoft.AspNetCore.Mvc.Testing
NetArchTest.Rules or ArchUnitNET
```

Do not add unused packages.

---

## 9. Code formatting and repository settings

Create or update:

```text
.editorconfig
.gitattributes
.gitignore
```

### `.editorconfig`

Configure:

* UTF-8.
* Final newline.
* Four spaces for C#.
* Two spaces for JSON, YAML, and TypeScript.
* File-scoped namespaces preferred.
* `var` only when type is apparent, or use an explicit consistent rule.
* Braces required.
* Private fields prefixed with underscore.
* Nullable analysis respected.
* Using directives sorted.
* No trailing whitespace.

Avoid excessive or controversial style rules.

### `.gitattributes`

Configure consistent text normalization.

Use:

```text
* text=auto
```

Add suitable overrides for:

```text
*.cs
*.csproj
*.sln
*.json
*.md
*.yml
*.yaml
*.ts
*.html
*.scss
```

Do not introduce platform-specific line-ending churn unnecessarily.

### `.gitignore`

Include:

* Visual Studio files.
* Rider files.
* VS Code local files where appropriate.
* `bin/`
* `obj/`
* Angular `node_modules/`
* Angular build output.
* Test results.
* Coverage output.
* Local database files if any.
* User secrets.
* Environment files containing secrets.
* Docker local overrides when appropriate.

Do not ignore documentation or committed configuration templates.

---

## 10. API baseline

Configure the API with:

* HTTPS redirection when appropriate.
* OpenAPI/Swagger in Development.
* Minimal health endpoint.
* No sample weather forecast code.
* No authentication yet.
* No Entity Framework yet.
* No Identity yet.
* No JWT yet.
* No CORS policy unless needed for the initialized Angular shell.

If CORS is added, document that it is a development placeholder and do not use `AllowAnyOrigin` together with credentials.

Add:

```text
appsettings.json
appsettings.Development.json
```

Do not add secrets.

Add a placeholder connection-string key only if needed for documentation:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

Prefer leaving the connection string out until the SQL Server increment rather than adding a misleading value.

---

## 11. Angular baseline

If Angular CLI is available, create an Angular application using:

* Standalone components.
* Routing enabled.
* SCSS.
* Strict mode.
* No server-side rendering unless already required by the environment.
* No state-management library.
* No Angular Material yet.
* No authentication functionality.
* No task functionality.

Application name:

```text
ballastlane-tasks-web
```

Create only:

* Application shell.
* Placeholder home page.
* Placeholder `/health` or `/about` route if useful.
* Production build configuration.
* Initial unit test.
* Initial README.

Do not call the backend yet unless it can be done with one trivial health service and test without introducing unnecessary complexity.

If Node or Angular CLI is unavailable:

* Do not install global tools.
* Document the required commands in `client/ballastlane-tasks-web/README.md`.
* Continue with the backend baseline.

---

## 12. Documentation

Create:

```text
README.md
```

Include:

1. Project overview.
2. Current status.
3. Planned technology stack.
4. Repository structure.
5. Architecture dependency direction.
6. Prerequisites.
7. Build commands.
8. Test commands.
9. API startup command.
10. Current health endpoint.
11. Angular status.
12. Planned LocalDB setup.
13. Planned Docker SQL Server setup.
14. Statement that no production secrets belong in source control.
15. Link to architectural decision records.

Clearly mark features that are planned but not yet implemented.

Do not claim that Identity, SQL Server, JWT, task CRUD, or Angular integration already exists.

---

## 13. Architecture document

Create:

```text
docs/architecture/solution-overview.md
```

Include:

### System purpose

A personal task-management application where registered users will manage their own tasks.

### Informal user story

```text
As a registered user,
I want to create, view, update, complete, and delete my personal tasks,
so that I can organize my work and track upcoming deadlines.
```

### Context diagram

Use Mermaid.

Show:

```text
User
Angular SPA
ASP.NET Core API
SQL Server
```

Mark SQL Server, Identity, JWT, and task functionality as planned.

### Container or component diagram

Use Mermaid.

Show:

```text
API
Application
Domain
Infrastructure
Angular
SQL Server
```

### Dependency direction

Document:

```text
Domain ← Application ← Infrastructure
                  ↑
                 API
```

Clarify that API is the composition root and Infrastructure implements Application abstractions.

### Principles

Document:

* Dependency inversion.
* Separation of concerns.
* Framework isolation.
* Thin controllers.
* Server-controlled ownership.
* Explicit use cases.
* Behavioral testing.
* No premature distributed architecture.

### Current versus planned status

Use a clear table:

```text
Component | Status
```

Mark the current increment accurately.

---

## 14. Architecture decision records

Create the following ADRs.

### ADR-001 — Clean Architecture modular monolith

File:

```text
docs/decisions/ADR-001-clean-architecture-modular-monolith.md
```

Decision:

Use Clean Architecture inside a modular monolith.

Document:

* Context.
* Decision.
* Alternatives considered.
* Consequences.
* Status: Accepted.

Rejected alternatives:

* Microservices.
* Single-project layered application.
* Event sourcing.

Explain that the application is too small to justify distributed architecture.

---

### ADR-002 — SQL Server and LocalDB

File:

```text
docs/decisions/ADR-002-sql-server-localdb.md
```

Decision:

* SQL Server provider.
* LocalDB for Windows local development.
* SQL Server container as portable alternative.
* Azure SQL or managed SQL Server as likely production path.

Document portability considerations.

Do not implement SQL Server yet.

---

### ADR-003 — ASP.NET Core Identity and JWT

File:

```text
docs/decisions/ADR-003-identity-jwt.md
```

Decision:

* ASP.NET Core Identity for user management.
* JWT bearer tokens for Angular-to-API authentication.
* Identity implementation remains in Infrastructure.
* Application depends on abstractions rather than `UserManager` or `SignInManager`.

Document security and testing implications.

Do not implement Identity or JWT yet.

---

### ADR-004 — Angular SPA

File:

```text
docs/decisions/ADR-004-angular-spa.md
```

Decision:

Use Angular as the frontend.

Document:

* Standalone components.
* Reactive forms.
* Feature-oriented organization.
* Route guards.
* HTTP interceptor.
* No NgRx initially.
* State-management complexity should remain proportional to the application.

---

### ADR-005 — Testing strategy

File:

```text
docs/decisions/ADR-005-testing-strategy.md
```

Document the distinction between:

* Domain unit tests.
* Application unit tests.
* Infrastructure integration tests.
* API integration tests.
* Architecture tests.
* Angular unit tests.

State that EF Core repository behavior will eventually be tested against SQL Server rather than relying solely on EF Core InMemory.

---

## 15. AI documentation baseline

Create:

```text
docs/ai/README.md
```

Document the planned evidence:

```text
01-original-prompt.md
02-generated-output.md
03-review-findings.md
04-corrections.md
05-validation-results.md
```

Explain:

* AI will be used as an implementation assistant.
* Architecture remains human-controlled.
* Generated code must be reviewed.
* Build, tests, security, and architecture rules determine acceptance.
* Representative generated code will be included.
* The exact prompt will be preserved.

Do not fabricate AI results yet.

---

## 16. Presentation baseline

Create:

```text
docs/presentation/outline.md
```

Add the planned presentation sections:

1. Problem and user story.
2. Architecture.
3. Domain model.
4. SQL Server and persistence.
5. Identity and authentication.
6. API design.
7. Angular design.
8. TDD and testing.
9. AI usage and critical review.
10. Demo.
11. Trade-offs.
12. Production roadmap.

Mark unfinished sections as planned.

---

## 17. Development scripts

Create cross-platform scripts where practical.

Suggested scripts:

```text
scripts/build.ps1
scripts/test.ps1
scripts/build.sh
scripts/test.sh
```

They should run:

```text
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

Avoid duplicated logic where possible.

If Angular was initialized, optionally add scripts for:

```text
npm ci
npm test
npm run build
```

Do not assume Docker is installed in this increment.

---

## 18. Continuous integration baseline

Create:

```text
.github/workflows/ci.yml
```

Backend job:

1. Checkout.
2. Setup .NET 10.
3. Restore.
4. Build in Release.
5. Run tests in Release.
6. Collect test results.

If Angular was initialized, add a separate frontend job:

1. Setup supported Node LTS.
2. `npm ci`.
3. Run tests non-interactively.
4. Build production bundle.

Do not add SQL Server services or Docker-dependent tests yet.

Do not hide test failures.

---

## 19. Naming and code conventions

Use:

* `TaskItem` rather than `Task`.
* `Guid` for domain and user identifiers.
* `DateTimeOffset` for business timestamps unless a later ADR changes it.
* UTC for persisted timestamps.
* `CancellationToken` on asynchronous application and infrastructure operations in later increments.
* Explicit request and response contracts.
* No generic repository.
* No `IQueryable` outside Infrastructure.
* No EF entities returned directly from API endpoints.
* No framework types inside Domain.

These are documented decisions only in this increment.

Do not implement the domain yet.

---

## 20. Forbidden work in this increment

Do not implement:

* `TaskItem`.
* Task status enum.
* Task repositories.
* EF Core DbContext.
* SQL Server migrations.
* Identity users.
* Registration.
* Login.
* JWT generation.
* Authorization policies.
* Task controllers.
* Task CRUD.
* Seed data.
* Angular login.
* Angular task pages.
* Docker Compose.
* Testcontainers.
* MediatR.
* AutoMapper.
* FluentValidation.
* Serilog.
* NgRx.
* CQRS frameworks.
* Message queues.
* Microservices.

Do not add libraries merely because they may be used later.

---

## 21. Required validation

After implementation, run:

```text
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet format --verify-no-changes
```

If Angular was initialized, also run:

```text
npm ci
npm test -- --watch=false
npm run build
```

Adapt the test command to the configured Angular runner.

Also run:

```text
git diff --check
```

Report:

1. Files created.
2. Files modified.
3. Project references.
4. NuGet packages added.
5. Build result.
6. Test result by project.
7. Architecture-test result.
8. Formatting result.
9. Angular initialization status.
10. Warnings.
11. Deviations from this prompt.
12. Open issues.
13. Suggested next increment.

Do not claim a validation passed unless the command was actually executed successfully.

---

## 22. Final response format

Return the result using these sections:

```text
1. Summary
2. Repository structure
3. Project dependency graph
4. Files created
5. Packages added
6. Architecture rules implemented
7. Documentation created
8. Validation commands and results
9. Deviations or limitations
10. Git status
11. Recommended next increment
```

Include concise evidence.

Do not make unrelated code changes.

Do not commit automatically unless explicitly instructed.

Preserve the working tree and report its final state.
```
