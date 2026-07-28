# ballastlane-tasks-web

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 22.0.8.

## Status

Full authentication and task management UI, wired to the real backend:

* Register, login, log out, and stay signed in across a page refresh (session-scoped — see
  [ADR-011](../../docs/decisions/ADR-011-spa-token-storage.md)).
* View, create, edit, change the status of, and delete your own tasks.
* Route guards (`authGuard`/`guestGuard`), a functional auth interceptor, and centralized
  Problem-Details-aware error handling.
* Standalone components, Angular routing, Reactive Forms, signals for local/auth state, strict
  TypeScript, SCSS, Vitest.

See [`../../docs/decisions/ADR-004-angular-spa.md`](../../docs/decisions/ADR-004-angular-spa.md) for
the frontend architecture decision and [`../../docs/decisions/ADR-011-spa-token-storage.md`](../../docs/decisions/ADR-011-spa-token-storage.md)
for the token-storage trade-off.

## Prerequisites

* Node.js `v24.15.0`+ and npm `11.12.1`+ (validated versions).
* The backend running locally — see the root [README](../../README.md)'s "Connection string and user
  secrets" and "Migrations" sections for setup. Without it, every API call in the SPA will fail.

## Configuring the API base URL

The backend origin is set in one place: `src/environments/environment.development.ts`
(`apiBaseUrl`), consumed everywhere via the `API_BASE_URL` injection token
(`src/app/core/config/api-config.ts`) — no service hardcodes a URL. It defaults to
`http://localhost:5276`, matching the API's `http` launch profile
(`src/Ballastlane.Tasks.Api/Properties/launchSettings.json`). Change that one file if your backend
runs on a different port.

Production (`src/environments/environment.ts`) uses `apiBaseUrl: ''` deliberately — every request URL
resolves as a relative path (`/api/...`) against whatever origin actually serves the build, so it
works unmodified behind any reverse proxy or same-origin host, with no per-environment rebuild
needed. This assumes the SPA and API are served from the same origin in production; see the root
README's Trade-offs section for the alternative (a build-time-injected absolute API URL) and when to
reconsider. The `authInterceptor`'s same-origin check (which decides whether to attach the bearer
token) is computed via the `URL` API specifically so it stays correct under both the absolute
development URL and this relative production one — see `auth.interceptor.spec.ts`.

## Development CORS

The backend only allows cross-origin requests from `http://localhost:4200` (the Angular CLI's
default dev-server origin), and only in Development — see `AddDevelopmentCors` in
`src/Ballastlane.Tasks.Api/ApiServiceCollectionExtensions.cs`. No `AllowAnyOrigin`, no credentials
mode (the SPA authenticates via an `Authorization` header, not cookies). If you run `ng serve` on a
different port, add it to `Cors:AllowedOrigins` in the API's configuration.

## Demo credentials

In Development, the backend seeds a demo user automatically:

```text
Email:    demo@ballastlane.local
Password: Demo1234!
```

## Running both servers

```bash
# Terminal 1 — backend
dotnet run --project ../../src/Ballastlane.Tasks.Api

# Terminal 2 — frontend
npm ci
npm start
```

Then open `http://localhost:4200`.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

```bash
npm test -- --watch=false
```

54 tests across 11 spec files: `TokenStorageService`, `AuthStore`, `authInterceptor`, `authGuard`,
`guestGuard`, `Login`, `Register`, `TaskService`, `TaskForm`, `TaskList`, and the root `App` shell.

## Routes

| Route | Guard | Page |
|---|---|---|
| `/` | — | Redirects to `/tasks` (which itself redirects anonymous users to `/login`) |
| `/login` | `guestGuard` | Sign in |
| `/register` | `guestGuard` | Create an account |
| `/tasks` | `authGuard` | Task list, with status/search filters |
| `/tasks/new` | `authGuard` | Create a task |
| `/tasks/:id/edit` | `authGuard` | Edit a task |
| `/profile` | `authGuard` | Current user's profile |
| `**` | — | Not-found page |

## Known limitations

* No end-to-end browser test suite (Cypress/Playwright) — no browser automation tool was available
  in this development environment; covered instead by the unit/component test suite above plus a
  manual `curl`-level API contract walkthrough. See
  [`../../docs/qa/manual-verification-checklist.md`](../../docs/qa/manual-verification-checklist.md)
  for what still needs a real browser to confirm.
* No refresh-token flow — an expired session redirects to `/login` (see
  [ADR-011](../../docs/decisions/ADR-011-spa-token-storage.md)).

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
