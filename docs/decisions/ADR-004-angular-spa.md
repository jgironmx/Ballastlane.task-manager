# ADR-004 — Angular SPA

## Status

Accepted

## Context

The stack specifies Angular as the frontend for a small, single-user-scoped task management UI:
login, a task list, and task CRUD forms. The application does not need multi-framework flexibility,
server-side rendering, or heavyweight state management — the primary state is "the current user's
tasks," which the API already owns.

## Decision

Use Angular, with:

* **Standalone components** — no `NgModule` boilerplate; each component/route declares its own
  dependencies directly.
* **Reactive forms** — for task and authentication forms, since they need validation, testability, and
  synchronous access to form state.
* **Feature-oriented organization** — code is grouped by feature (e.g., `pages/home`, and later
  `features/tasks`, `features/auth`) rather than by technical type (all components in one folder, all
  services in another).
* **Route guards** — to protect authenticated routes once authentication exists, rather than checking
  auth state ad hoc inside components.
* **HTTP interceptor** — a single interceptor will attach the JWT bearer token to outgoing API requests
  and centralize handling of `401`/`403` responses, once authentication exists.
* **No NgRx initially** — the app's state is small (current user, current task list) and is well served
  by Angular signals and simple services backed by `HttpClient`. A state-management library is not
  introduced preemptively.

State-management complexity should stay proportional to the application: if task-related state grows
complex enough that services and signals become unwieldy, that is the trigger to reconsider NgRx or a
similar library — not a decision made up front.

## Initial scope

At the time this decision was accepted, the workspace shipped only a placeholder shell: a standalone
`App` root, placeholder `Home` (`/`) and `About` (`/about`) pages, routing, SCSS, strict mode, and one
Angular CLI-generated production build configuration. No authentication or task UI existed yet — this
ADR's decision (standalone components, Reactive Forms, feature-oriented organization, route guards,
an HTTP interceptor, no NgRx initially) was a plan for the frontend work that would follow, not a
description of what already existed.

## Outcome

The plan was carried out. The final Angular implementation includes:

* **Authentication**: registration, login, logout, and a session that survives a browser refresh
  (`AuthStore`, signal-based — see [ADR-011](ADR-011-spa-token-storage.md)).
* **Protected routes**: `authGuard`/`guestGuard`, both waiting on `AuthStore.initialize()` before
  deciding, with `returnUrl` preserved for post-login redirect.
* **Profile**: a dedicated `/profile` route showing the current user.
* **Task CRUD**: list (with status/search filters), create, edit, per-row status change, and delete
  with an accessible confirmation dialog — under `features/tasks/`.
* **Standalone components** throughout — no `NgModule` was ever introduced; the placeholder `Home`/
  `About` pages from the initial scope no longer exist, replaced by the routes above plus a
  not-found page.
* **Reactive Forms** for the login, registration, and task forms, with client-side validation
  mirroring backend rules.
* **Signals** for local and auth state (`AuthStore`); **RxJS** where it fits actual asynchronous
  event streams (`HttpClient` calls) — no NgRx was introduced, consistent with this ADR's original
  "not preemptively" decision; state complexity never grew enough to trigger revisiting it.
* **Guards and interceptors**: `authGuard`/`guestGuard` as decided above, plus `authInterceptor`
  (attaches the bearer token) and `httpErrorInterceptor` (centralizes `401` handling) — the two
  interceptors this ADR anticipated as one combined interceptor ended up split by concern.

Because there was no `NgModule` layer, adding each new page meant adding a standalone component and a
route entry — no module wiring, as anticipated.
