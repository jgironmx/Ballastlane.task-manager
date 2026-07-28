# 05 — Validation results

Actual output from commands run against the corrected code during a hardening implementation phase,
not summarized or assumed. Timestamps/durations are from the actual run.

## Backend build

```text
$ dotnet build --configuration Release
Compilación correcta.
    0 Advertencia(s)
    0 Errores
Tiempo transcurrido 00:00:11.37
```

## Backend full test suite

```text
$ dotnet test --configuration Release --no-build
Correctas! - Con error: 0, Superado: 17, Omitido: 0, Total: 17 - Ballastlane.Tasks.Domain.Tests.dll
Correctas! - Con error: 0, Superado: 34, Omitido: 0, Total: 34 - Ballastlane.Tasks.Application.Tests.dll
Correctas! - Con error: 0, Superado: 11, Omitido: 0, Total: 11 - Ballastlane.Tasks.ArchitectureTests.dll
Correctas! - Con error: 0, Superado: 11, Omitido: 0, Total: 11 - Ballastlane.Tasks.Infrastructure.IntegrationTests.dll
Correctas! - Con error: 0, Superado: 26, Omitido: 0, Total: 26 - Ballastlane.Tasks.Api.IntegrationTests.dll
```

Total: 99/99 passed (17 Domain + 34 Application [was 29 before Sprint 4's 5 new pagination-validation
tests] + 11 Architecture + 11 Infrastructure integration + 26 Api integration [was 20 before Sprint 3,
then +6 in Sprint 4: 3 pagination-validation cases and 1 Problem Details body assertion, replacing/
extending the prior count]).

The one initial failure during this sprint, caught before being accepted, was
`ProtectedEndpoint_WithoutToken_ShouldReturnProblemDetailsBody` expecting
`application/problem+json` but observing `application/json` — `HttpResponse.WriteAsJsonAsync`
overwrites a manually-set `Response.ContentType`, so the content type had to be passed as the
method's own `contentType` parameter instead. Documented here because it is real evidence of
a test catching an incorrect first attempt, not because the eventual fix was hard.

## CI test-filter split (validated locally before trusting the CI YAML)

```text
$ dotnet test Ballastlane.Tasks.sln --no-build --configuration Release --filter "Category!=Integration"
Domain.Tests: 17 passed | Application.Tests: 34 passed | ArchitectureTests: 11 passed
(Infrastructure.IntegrationTests and Api.IntegrationTests: 0 matched, correctly excluded)

$ dotnet test Ballastlane.Tasks.sln --no-build --configuration Release --filter "Category=Integration"
Infrastructure.IntegrationTests: 11 passed | Api.IntegrationTests: 26 passed
(Domain.Tests, Application.Tests, ArchitectureTests: 0 matched, correctly excluded)
```

## Code formatting

```text
$ dotnet format --verify-no-changes
(exit 0, no output — no formatting violations)
```

## NuGet vulnerability audit

```text
$ dotnet list package --vulnerable --include-transitive
El proyecto "Ballastlane.Tasks.Domain" ... no tiene paquetes vulnerables en los orígenes actuales.
El proyecto "Ballastlane.Tasks.Application" ... no tiene paquetes vulnerables en los orígenes actuales.
El proyecto "Ballastlane.Tasks.Infrastructure" ... no tiene paquetes vulnerables en los orígenes actuales.
El proyecto "Ballastlane.Tasks.Api" ... no tiene paquetes vulnerables en los orígenes actuales.
(and all 5 test projects: same result)
```
No vulnerable packages in any of the 9 projects.

## Frontend test suite

```text
$ npm test -- --watch=false
 Test Files  11 passed (11)
      Tests  54 passed (54)
   Duration  4.46s
```
(52 before Sprint 4; +2 new `auth.interceptor.spec.ts` cases covering the relative-URL fix.)

## Frontend production build

```text
$ npm run build
Application bundle generation complete. [3.203 seconds]
Initial total: 278.54 kB raw / 75.12 kB estimated transfer
Output location: .../client/ballastlane-tasks-web/dist/ballastlane-tasks-web
```

## npm audit

```text
$ npm audit
3 moderate severity vulnerabilities (@hono/node-server -> @modelcontextprotocol/sdk -> @angular/cli, dev-only)

$ npm audit --omit=dev
found 0 vulnerabilities
```
Unchanged from Sprint 3 — dev-tooling only, production dependency tree remains clean.
