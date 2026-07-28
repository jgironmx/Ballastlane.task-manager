# Original Prompt — Sprint 3: Angular Authentication and Task CRUD

This file preserves the prompt provided to Claude Code for Sprint 3.

[`docs/reports/final-report-3-sprint-3-angular.md`](final-report-3-sprint-3-angular.md), which
reports what was actually built in response to it. 

See also the companion prompts for
[Increment 0](prompt-1-increment-0.md) and
[Sprint 2](prompt-2-sprint-2-backend.md).

```text
# Sprint 3 — Angular Authentication and Task CRUD

You are the implementation engineer for the Ballastlane.Tasks architecture exercise.

The backend is already complete and validated. It provides:

* ASP.NET Core Identity registration and login
* JWT bearer authentication
* SQL Server persistence
* Per-user task ownership
* Task CRUD endpoints
* RFC 7807 Problem Details
* OpenAPI
* Development seed data

The Angular 22 application currently exists only as a shell.

Your task is to implement the complete Angular frontend required by the architecture exercise without changing backend behavior unless a genuine API incompatibility is discovered.

---

# 1. Precondition and Git safety

Before making frontend changes:

1. Run:

```bash
git status --short
```

2. Confirm that the backend has been committed and the working tree is clean.

3. If the working tree is not clean, stop and report the state. Do not mix frontend work with uncommitted backend changes.

4. Record the current backend commit hash:

```bash
git rev-parse --short HEAD
```

Do not commit automatically at the end of this sprint unless explicitly instructed.

---

# 2. Objective

Implement a responsive Angular SPA where users can:

1. Register.
2. Log in.
3. Remain authenticated across browser refreshes.
4. Log out.
5. View their current profile.
6. View their own tasks.
7. Create a task.
8. Edit a task.
9. Change task status.
10. Delete a task.
11. Receive useful validation and API error messages.
12. Be redirected appropriately when authentication expires.

The Angular application must integrate with the existing backend contracts rather than inventing new API behavior.

---

# 3. Inspect the backend first

Before writing Angular code, inspect the actual backend contracts.

Identify and document:

* Register request and response.
* Login request and response.
* Current-user response.
* Task response.
* Create-task request.
* Update-task request.
* Change-status request.
* Paged or non-paged task-list response.
* Problem Details response structure.
* JWT expiration metadata.
* Actual enum serialization values.
* Actual API base routes.

Do not infer these contracts from the sprint prompt when the source code provides the exact answer.

Create TypeScript interfaces that match the backend JSON precisely.

Do not change the backend solely to simplify frontend implementation.

If a real incompatibility exists, report it before changing backend code.

---

# 4. Angular technical baseline

Use the existing Angular 22 workspace.

Use:

* Standalone components.
* Angular routing.
* Reactive Forms.
* `HttpClient`.
* Functional interceptors where appropriate.
* Functional guards where appropriate.
* Signals for local and authentication state.
* RxJS for asynchronous HTTP flows.
* Strict TypeScript.
* SCSS.
* Angular built-in control flow syntax.
* Vitest, matching the existing Angular workspace.

Do not introduce:

* NgRx.
* Akita.
* Redux.
* PrimeNG.
* A complex state library.
* A generic API repository abstraction.
* Generated API clients unless one already exists.
* Server-side rendering.
* Micro-frontends.

Keep the frontend proportional to the exercise.

---

# 5. Recommended frontend structure

Organize the application approximately as follows:

```text
src/app/
  core/
    auth/
      auth.models.ts
      auth.service.ts
      auth.store.ts
      auth.guard.ts
      guest.guard.ts
      auth.interceptor.ts
      token-storage.service.ts

    http/
      api-error.model.ts
      api-error.service.ts
      http-error.interceptor.ts

    config/
      api-config.ts

  shared/
    components/
      loading-indicator/
      confirmation-dialog/
      empty-state/
      field-error/

    utilities/
    validators/

  layout/
    application-shell/
    header/
    navigation/

  features/
    auth/
      login/
      register/

    tasks/
      task.models.ts
      task.service.ts
      task-list/
      task-form/
      task-create-page/
      task-edit-page/
      task-status-badge/

    profile/
      profile-page/

  app.routes.ts
  app.config.ts
  app.component.ts
```

You may adapt this structure to existing Angular conventions, but maintain clear feature boundaries.

Avoid creating abstractions used by only one trivial class unless they improve clarity.

---

# 6. Environment and API configuration

Configure the backend base URL through Angular environments or an equivalent typed configuration mechanism.

Development example:

```text
https://localhost:<backend-port>
```

Do not scatter API URLs throughout services.

Use one central base URL.

Update:

```text
environment.ts
environment.development.ts
```

or the workspace's actual environment structure.

Document how to change the backend URL.

Do not commit production secrets. The API URL is not a secret.

---

# 7. Authentication models

Create frontend models matching the real API.

Expected conceptual models may include:

```typescript
export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthenticatedUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: AuthenticatedUser;
}
```

These are examples only.

Inspect and match the backend's exact property names and response shape.

Do not model Identity-internal fields.

---

# 8. Token-storage decision

Implement a small `TokenStorageService`.

For this exercise, store the JWT in:

```text
sessionStorage
```

Recommended rationale:

* It survives browser refreshes within the same tab.
* It is cleared when the browser session ends.
* It has a smaller persistence window than `localStorage`.
* It remains simple for a demonstration SPA.

Document that browser storage remains accessible to JavaScript and therefore depends on strong XSS prevention.

State that a production system may instead use:

```text
Secure
HttpOnly
SameSite
cookies
```

depending on architecture and CSRF strategy.

Do not claim session storage is fully secure.

Store only the minimum required data:

* Access token.
* Expiration time when needed.

Prefer reloading the current profile through `/api/auth/me` rather than trusting a persisted full user object indefinitely.

---

# 9. Authentication store

Create a small authentication state service or store using signals.

Suggested state:

```typescript
interface AuthState {
  user: AuthenticatedUser | null;
  initialized: boolean;
  loading: boolean;
}
```

Expose read-only state such as:

```text
user
isAuthenticated
isInitialized
isLoading
```

Responsibilities:

* Initialize authentication at startup.
* Check stored token.
* Remove expired tokens.
* Load `/api/auth/me` for a valid token.
* Register.
* Login.
* Logout.
* Clear authentication after a `401`.
* Avoid duplicate initialization requests.

Do not manually decode JWT claims as the source of truth for profile data.

JWT decoding may be used only to inspect expiration if the API already returns no expiration metadata, but prefer server-returned expiration metadata.

Do not install a JWT decoding package unless necessary.

---

# 10. Authentication interceptor

Implement a functional HTTP interceptor.

Behavior:

* Attach:

```http
Authorization: Bearer <token>
```

to API requests when a token exists.

* Do not attach it to unrelated external URLs.
* Do not attach an empty or expired token.
* Avoid modifying requests unnecessarily.
* Do not log the JWT.
* Do not add custom retry behavior for authentication requests.

Use exact backend base-URL matching.

---

# 11. Unauthorized response handling

Implement consistent `401` handling.

When an authenticated API request returns `401`:

1. Clear the token.
2. Clear authentication state.
3. Redirect to `/login`.
4. Preserve the originally requested frontend URL when practical.
5. Display a message such as:

```text
Your session expired. Please sign in again.
```

Avoid redirect loops for:

```text
/api/auth/login
/api/auth/register
```

Do not treat normal invalid-login responses as an expired session.

---

# 12. Route guards

Implement:

## Auth guard

Protect:

```text
/tasks
/tasks/new
/tasks/:id/edit
/profile
```

Behavior:

* Wait until authentication initialization completes.
* Allow authenticated users.
* Redirect anonymous users to `/login`.
* Preserve a return URL.

## Guest guard

Protect:

```text
/login
/register
```

Behavior:

* Redirect already authenticated users to `/tasks`.

Do not determine authentication only by the presence of a string in storage.

Use initialized authentication state.

---

# 13. Routes

Implement routes similar to:

```text
/                  → redirect to /tasks or /login
/login             → login
/register          → registration
/tasks             → task list
/tasks/new         → create task
/tasks/:id/edit    → edit task
/profile           → current profile
**                 → not-found page
```

Lazy-load feature routes or standalone components when straightforward.

Do not introduce routing complexity merely for appearance.

---

# 14. Application layout

Create a clean application shell.

Authenticated layout should include:

* Application name.
* Tasks navigation.
* Profile navigation.
* Current user name or email.
* Logout action.

Anonymous pages should use a simpler centered layout.

Requirements:

* Responsive from mobile width upward.
* Keyboard-accessible navigation.
* Visible focus indicators.
* Semantic HTML.
* No horizontal scrolling at typical mobile widths.
* No browser-console warnings.

Do not spend excessive time building a custom design system.

Use plain SCSS or an existing lightweight setup.

Do not add Angular Material unless it materially accelerates implementation and the dependency is explicitly justified.

Preferred choice: implement clean SCSS without a UI library.

---

# 15. Registration screen

Implement a reactive form with:

```text
First name
Last name
Email
Password
Confirm password
```

Client validation:

* All fields required.
* Valid email format.
* Password minimum requirements matching the backend documentation.
* Confirm password must match.
* Trim names and email appropriately.
* Do not trim passwords.

Behavior:

* Disable submit while processing.
* Display field-level validation.
* Display backend validation errors.
* Display duplicate-email conflict safely.
* On successful registration, navigate to login.
* Show a confirmation message.

Because the backend does not issue a token on registration, do not automatically treat the user as authenticated.

Do not duplicate every Identity password rule if the backend may change. Implement enough client guidance, but treat backend validation as authoritative.

---

# 16. Login screen

Implement a reactive form with:

```text
Email
Password
```

Behavior:

* Validate required fields.
* Disable submit while processing.
* Do not disclose whether the email exists.
* Display the backend's generic invalid-credentials message.
* Store the JWT only after successful login.
* Load or set the authenticated profile.
* Redirect to the preserved return URL or `/tasks`.
* Include a link to registration.

Do not log credentials or tokens.

Do not place demo credentials automatically in production configuration.

You may display demo credentials in Development mode only if clearly labeled.

---

# 17. Profile screen

Implement a simple protected profile page displaying:

```text
First name
Last name
Email
User ID, optional and visually de-emphasized
```

Load profile information through authentication state or `/api/auth/me`.

No profile editing is required.

No password-change functionality is required.

---

# 18. Task models

Create TypeScript models that match the backend.

Expected conceptual model:

```typescript
export type TaskItemStatus =
  | 'Pending'
  | 'InProgress'
  | 'Completed';

export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  status: TaskItemStatus;
  dueDate: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}
```

Match the exact backend JSON.

If enums serialize numerically, do not assume strings. Either:

* Match numeric serialization accurately, or
* Report that string serialization should be added to the backend if this is an API usability defect.

Do not silently map unknown status values to valid ones.

---

# 19. Task service

Implement a focused `TaskService`.

Methods should correspond to the real backend:

```text
getTasks
getTaskById
createTask
updateTask
changeStatus
deleteTask
```

Pass optional filters only if supported by the API.

Do not create a generic CRUD service.

Do not retain stale duplicated task state inside the service unless necessary.

Use typed responses.

---

# 20. Task list page

Create a protected task-list page.

Display:

* Title.
* Description preview.
* Status.
* Due date.
* Created or updated date when useful.
* Edit action.
* Delete action.
* Status-change control or action.

Provide:

* Loading state.
* Empty state.
* Error state.
* Retry action.
* Create-task action.

Optional filters, only when backend supports them:

* Status filter.
* Search text.

Avoid implementing client-only filtering when the API already supports filtering unless it is clearly documented.

Responsive behavior:

* Desktop may use a table.
* Mobile should use cards or a layout that remains readable.
* Actions must remain accessible.

---

# 21. Due-date display

The backend uses `DateOnly?`.

Treat date-only values as calendar dates, not UTC instants.

Avoid code that transforms:

```text
2026-07-26
```

into a previous or next date because of timezone conversion.

Prefer displaying date-only strings using a date-only formatter or controlled parsing.

Do not use:

```typescript
new Date('2026-07-26')
```

without understanding UTC parsing behavior.

Create a small date-only utility if necessary.

Document this distinction because it is an important design decision.

---

# 22. Create-task page

Implement a reactive form:

```text
Title
Description
Due date
```

Validation:

* Title required.
* Maximum 200 characters.
* Description maximum 2,000 characters.
* Due date cannot be earlier than the current local business date.
* Backend remains authoritative.

Request must not include:

```text
OwnerId
UserId
Status
CreatedAtUtc
UpdatedAtUtc
```

Behavior:

* Submit once.
* Disable while saving.
* Display validation failures.
* On success, navigate to the task list.
* Show success feedback.

New tasks must receive Pending status from the backend.

Do not send Pending explicitly unless the API contract requires it.

---

# 23. Edit-task page

Load the task by route ID.

Handle:

* Loading.
* Invalid ID.
* Not found.
* API error.
* Successful load.

Form:

```text
Title
Description
Due date
```

Do not include ownership.

Do not edit status through the details form unless the backend update contract includes status.

Use the dedicated status endpoint for status changes.

On successful update:

* Navigate back to the list or remain on the page with clear feedback.
* Choose one consistent interaction and document it.

---

# 24. Status changes

Provide an accessible control for:

```text
Pending
In Progress
Completed
```

Use the dedicated:

```http
PATCH /api/tasks/{id}/status
```

Behavior:

* Disable the affected control during update.
* Update the UI only after success, or implement a safe rollback if using optimistic updates.
* Display backend errors.
* Do not let one status update block the complete page unnecessarily.

Prefer a straightforward server-confirmed update over complex optimistic behavior.

---

# 25. Delete workflow

Provide a confirmation dialog or accessible inline confirmation.

Message should identify the task title.

On confirmation:

* Call delete endpoint.
* Disable duplicate submission.
* Remove the task from the visible list after success.
* Show success feedback.

On `404`:

* Remove the stale task from the UI.
* Display a non-alarming message that it is no longer available.

Do not use the browser's native `confirm()` unless time constraints make it necessary. A small reusable accessible confirmation component is preferred.

---

# 26. API error handling

The backend returns Problem Details.

Create a typed model compatible with its real response.

Expected fields may include:

```typescript
export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}
```

Inspect the actual backend.

Create an error-normalization service that maps:

* Network failure.
* Validation errors.
* `401`.
* `404`.
* `409`.
* Unexpected server failure.

Do not display raw stack traces or serialized objects to the user.

Do not hide errors only in the console.

Console logging may be used in Development, but user-facing feedback is required.

---

# 27. Notifications

Implement a minimal application notification system.

Support:

```text
Success
Error
Information
```

Use:

* A simple signal-based notification service.
* An accessible live region.
* Auto-dismiss for success messages.
* Manual dismissal for errors when useful.

Do not add a large notification package.

---

# 28. Loading and submission behavior

Ensure:

* Buttons are disabled during active submissions.
* Duplicate registration/login/task submissions are prevented.
* Page-level data loading has visible feedback.
* Controls remain usable after errors.
* Subscriptions are safely managed.

Prefer Angular's async patterns, signals, and built-in destruction helpers.

Avoid manual subscription accumulation.

---

# 29. Accessibility baseline

Validate:

* Every input has a label.
* Validation messages are associated with inputs.
* Buttons have clear accessible text.
* Status is not communicated by color alone.
* Dialog focus is managed.
* Keyboard users can operate all actions.
* Heading hierarchy is sensible.
* Focus moves meaningfully after navigation or validation when practical.
* Color contrast is reasonable.

Do not claim full WCAG compliance unless it is actually audited.

---

# 30. Frontend testing

Use Vitest and Angular testing utilities.

Implement meaningful tests.

## 30.1 Authentication service/store tests

Test:

* Successful login stores token.
* Failed login does not store token.
* Logout clears token and user.
* Initialization with no token produces anonymous state.
* Initialization with valid token loads current user.
* Initialization with expired or rejected token clears state.

## 30.2 Token storage tests

Test:

* Store token.
* Retrieve token.
* Clear token.
* Expiration behavior if implemented.

## 30.3 Auth interceptor tests

Test:

* Attaches bearer token to backend requests.
* Does not attach token when absent.
* Does not attach token to external URLs.

## 30.4 Route guard tests

Test:

* Authenticated user is allowed.
* Anonymous user is redirected to login.
* Guest guard redirects authenticated users.

## 30.5 Login component tests

Test:

* Invalid form prevents submit.
* Valid form calls login.
* Backend error is displayed.
* Successful login navigates correctly.

## 30.6 Registration component tests

Test:

* Required validation.
* Password confirmation.
* Successful registration.
* Duplicate-email response.

## 30.7 Task service tests

Test each API method:

* Correct verb.
* Correct route.
* Correct request body.
* Typed response handling.

## 30.8 Task-list component tests

Test:

* Loading state.
* Empty state.
* Rendered tasks.
* Delete flow.
* Status-change flow.
* API error display.

## 30.9 Task-form tests

Test:

* Required title.
* Maximum lengths.
* Past due date.
* Valid request mapping.
* Owner/status fields are absent from requests.

Do not create tests that only verify component construction.

---

# 31. End-to-end manual validation

Run backend and Angular simultaneously.

Validate this flow manually:

```text
1. Open the Angular application.
2. Register a new user.
3. Navigate to login.
4. Login.
5. Confirm protected navigation works.
6. View seeded or empty task list.
7. Create a task.
8. Verify it appears.
9. Edit the task.
10. Change its status.
11. Refresh the browser.
12. Confirm authentication remains active.
13. Delete the task.
14. Log out.
15. Confirm protected routes redirect to login.
16. Attempt invalid login.
17. Confirm safe error message.
```

Also verify:

* No browser-console errors.
* No failed network calls during normal usage.
* Authorization header is present on protected requests.
* Authorization header is absent on anonymous auth requests when appropriate.
* Another user's task is not visible.

If practical, create a second user and validate task isolation through the UI.

---

# 32. CORS

Inspect existing backend CORS configuration.

Configure Development CORS only as narrowly as needed for the Angular development origin.

Example:

```text
http://localhost:4200
```

Requirements:

* No `AllowAnyOrigin` with credentials.
* No unnecessary production-wide permissive policy.
* Document development origin configuration.
* Prefer configurable allowed origins.

If the backend already permits the correct development origin, do not modify it.

Any backend CORS change must be minimal, documented, and tested.

---

# 33. README and documentation

Update:

```text
README.md
client/ballastlane-tasks-web/README.md
docs/architecture/solution-overview.md
docs/presentation/outline.md
```

Document:

* Angular prerequisites.
* Node and npm versions.
* Backend startup.
* Frontend startup.
* API base-URL configuration.
* Development CORS.
* Login and registration behavior.
* Demo credentials.
* Token-storage decision.
* Security trade-off.
* Available routes.
* Frontend architecture.
* Test commands.
* Current implementation status.
* Known limitations.

Update status tables accurately.

Do not claim Docker support exists.

---

# 34. Architecture documentation

Update the architecture diagram to show actual implementation status:

```text
Browser
  ↓
Angular SPA
  ↓ HTTPS/JSON
ASP.NET Core API
  ↓
Application
  ↓
Domain
  ↓
Infrastructure
  ↓
SQL Server
```

Document:

* Angular authentication flow.
* JWT interceptor.
* Route guards.
* Auth state.
* Task feature boundaries.
* Problem Details handling.
* DateOnly handling.
* Session-storage trade-off.

Add an ADR if no existing ADR covers client token storage.

Suggested ADR:

```text
ADR-011 — Store development SPA access token in sessionStorage
```

Status:

```text
Accepted
```

Include production alternatives and security consequences.

---

# 35. Scope restrictions

Do not implement:

* Refresh tokens.
* Password reset.
* Email verification.
* Multi-factor authentication.
* Social login.
* Role administration.
* NgRx.
* SSR.
* PWA.
* Offline synchronization.
* WebSockets.
* Drag-and-drop boards.
* Calendar integrations.
* File attachments.
* Rich-text editor.
* Dark-mode system.
* Internationalization framework.
* Advanced animation.
* Docker support unless needed for frontend execution.
* Cypress or Playwright unless already installed and the essential work is fully complete.

Do not expand scope beyond the requirements.

---

# 36. Backend changes

Backend changes are allowed only for genuine frontend integration defects, such as:

* Incorrect CORS.
* API contract inconsistency.
* Missing JSON enum configuration.
* Incorrect Problem Details serialization.
* OpenAPI mismatch.

Before changing backend code:

1. Identify the defect.
2. Explain why the frontend cannot correctly consume the existing API.
3. Make the smallest possible correction.
4. Add or update backend tests.
5. Report the change explicitly.

Do not refactor backend code during this sprint merely because you prefer a different style.

---

# 37. Required validation commands

From repository root:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet format --verify-no-changes
```

From Angular directory:

```bash
npm ci
npm test -- --watch=false
npm run build
```

If the existing Vitest command differs, use the workspace-defined non-watch CI command.

Also run:

```bash
git diff --check
git status --short
git diff --stat
```

Run the backend and frontend and complete the manual walkthrough.

Do not claim browser validation passed unless it was actually performed.

---

# 38. Required final report

Return the following sections.

## 1. Executive summary

What was implemented and what remains.

## 2. Backend contract discovered

List the actual contracts consumed by Angular.

## 3. Frontend architecture

Explain:

* Feature structure.
* Auth state.
* Token storage.
* Interceptor.
* Guards.
* Error normalization.
* Notifications.
* Task state strategy.

## 4. Routes

List every Angular route and its guard.

## 5. Components and services

Group created and modified files by feature.

## 6. Authentication workflow

Explain:

```text
Registration
Login
Storage
Initialization
Authenticated request
401 handling
Logout
```

## 7. Task workflow

Explain:

```text
List
Create
Edit
Status change
Delete
```

## 8. Tests

Report frontend test counts by service/component area.

## 9. Validation

Report exact outcomes for:

```text
dotnet build
dotnet test
dotnet format
npm test
npm build
manual browser walkthrough
browser console
git diff --check
```

## 10. Backend changes

List every backend change, or explicitly state none.

## 11. Accessibility

Report what was validated and what was not formally audited.

## 12. Security considerations

Explain:

* Session storage trade-off.
* XSS implications.
* JWT expiration behavior.
* No refresh token.
* Protected routes are UX only; API remains the true authorization boundary.

## 13. Deviations

List every deviation from this prompt.

## 14. Known limitations

Only actual limitations.

## 15. Git status

Include:

```bash
git status --short
git diff --stat
```

## 16. Recommended next sprint

Recommend:

```text
Sprint 4 — Quality hardening, documentation, and delivery readiness
```

Do not commit automatically.

Do not claim completion unless the complete authentication and Task CRUD workflow works through the browser.
```
