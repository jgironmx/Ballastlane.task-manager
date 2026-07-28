
# Final Report — Initial architecture definition with ChatGPT

This is the actual final report delivered at the end of initila prompt with ChatGPT, 

See also the reports for
[Sprint 1](final-report-1-increment-0.md).
[Sprint 2](final-report-2-sprint-2-backend.md)
[Sprint 3](final-report-3-sprint-3-angular.md).

---

# Architecture analysis

This is a well-sized architecture exercise:
small enough to complete and explain, but rich enough to demonstrate authentication, authorization, ownership, validation, persistence, API design, frontend state, automated testing, and architectural boundaries.

The strongest implementation would not try to appear “enterprise-scale.” It should demonstrate disciplined engineering with the least architectural complexity necessary.

.NET 10 and Angular 22 are valid current technology choices. ASP.NET Core 10 provides the expected API, authentication, authorization, dependency injection, middleware, OpenAPI, and Problem Details capabilities. Angular 22 is currently available and emphasizes standalone components, signals, Signal Forms, and modern reactive state management.


# 1. Recommended interpretation of the exercise

I would interpret the application as a single-user-owned task system, not a shared collaboration system.

Each task:

belongs to exactly one user;
is visible only to that user;
can be modified only by that user;
has a finite, explicitly defined lifecycle;
is persisted in a relational database;
is exposed through an authenticated HTTP API;
is managed through a responsive Angular SPA.

The architecture should be designed as a modular monolith with Clean Architecture boundaries.

That means:

one deployable backend application;
one database;
one Angular frontend;
logical and project-level separation between domain, application, infrastructure, and API concerns;
no microservices, message broker, distributed cache, event bus, or distributed identity system unless the exercise explicitly asks for them.

For this scope, microservices would reduce clarity rather than demonstrate architectural maturity.

# 2. Functional requirements

## 2.1 Authentication and user management
FR-01 — Register a user

A visitor can create an account by supplying, at minimum:

email or username;
password;
optional display name.

Expected behavior:

email or username must be unique;
password must satisfy an agreed policy;
password must be hashed by an established identity library;
duplicate registration returns a predictable conflict or validation response;
successful registration either logs the user in or directs them to log in.
FR-02 — Log in

A registered user can authenticate using their credentials.

Successful login returns either:

a secure authentication cookie; or
an access token, and potentially a refresh token.

The decision between cookies and tokens should be documented rather than treated as an implementation detail.

FR-03 — Log out

A user can terminate their authenticated session.

For token authentication, logout semantics must be clearly defined. Removing a client-side access token is not the same as server-side token revocation.

FR-04 — Retrieve current user information

An authenticated user can obtain basic information about the current identity, for example:

user ID;
email;
display name.

This is useful for initializing the Angular session state.

## 2.2 Task management
FR-05 — Create a task

An authenticated user can create a task containing:

title;
description;
status;
due date.

The associated user must be determined by the authenticated identity, not accepted from a client-provided userId.

This is an important security rule. A request such as the following should not be allowed to assign ownership:

{
  "title": "Task",
  "userId": "another-user-id"
}

The API should derive ownership from the authenticated principal.

FR-06 — View the current user’s task collection

An authenticated user can list only their own tasks.

The collection should ideally support basic query behavior:

filtering by status;
sorting by due date;
optionally showing overdue tasks;
optionally searching title or description;
pagination, even if only a simple implementation.

Pagination is not strictly necessary for a demonstration, but defining it shows that the collection endpoint has been designed rather than improvised.

FR-07 — View one task

An authenticated user can retrieve one of their own tasks by identifier.

The behavior for another user’s task must be intentionally selected:

return 404 Not Found to avoid disclosing its existence; or
return 403 Forbidden to explicitly report failed authorization.

I recommend 404 for inaccessible task resources because it avoids resource enumeration and gives the same observable response for nonexistent and unauthorized IDs.

FR-08 — Update a task

An authenticated user can edit permitted properties of their own task.

The architecture should decide whether updates use:

PUT for complete replacement;
PATCH for partial modification;
task-specific commands such as POST /tasks/{id}/complete.

For this exercise, a good balance is:

PUT /api/tasks/{id} for editing task details;
POST /api/tasks/{id}/complete for the meaningful state transition.

That lets the project demonstrate both resource updates and explicit business behavior.

FR-09 — Complete a task

An authenticated user can mark their own task as completed.

This should be treated as a business operation, not merely an arbitrary status assignment, if status transition rules matter.

Possible rules include:

a completed task receives completedAt;
an already completed task returns success idempotently or a conflict;
a completed task may be reopened only through a separate operation;
completion cannot be performed on a deleted task.
FR-10 — Delete a task

An authenticated user can delete their own task.

The project must decide between:

hard deletion;
soft deletion.

For a small personal task exercise, I recommend hard deletion, unless auditability or recovery is an explicit requirement. Soft deletion adds filtering, indexing, uniqueness, and lifecycle complexity that may distract from the main goals.

FR-11 — Public endpoint

At least one endpoint must allow anonymous access.

Good candidates:

GET /api/public/info;
GET /api/system/version;
GET /health;
GET /api/public/demo.

A health endpoint is useful operationally, but a simple informational endpoint may demonstrate anonymous API access more clearly during a presentation.

## 2.3 Demonstration requirements
FR-12 — Seed demonstration users

Provide documented sample credentials for development or demonstration only.

For example:

one demo user with several tasks;
a second user whose tasks prove ownership isolation.

The second user is important. A single seeded account does not convincingly demonstrate multi-user authorization.

FR-13 — Seed task scenarios

Seed tasks covering meaningful states:

pending task due in the future;
overdue task;
completed task;
task with description;
task without an optional description.
FR-14 — Responsive user interface

The Angular application should support at least:

registration;
login;
task list;
create task;
edit task;
complete task;
delete confirmation;
filters;
loading state;
empty state;
validation errors;
API failure state;
responsive mobile and desktop layouts.

# 3. Business rules that must be explicitly decided

The current acceptance criteria mention a “valid due date according to the agreed business rules,” but those rules are not yet defined.

This is one of the main areas to resolve before implementation.

## 3.1 Title rules

Recommended:

required;
trimmed before validation;
minimum length: 1 meaningful character;
maximum length: approximately 150 characters;
whitespace-only titles invalid.

Do not make the maximum unnecessarily large. A bounded title improves database design, UI layout, and validation predictability.

### 3.2 Description rules

Recommended:

optional;
maximum length: approximately 2,000–4,000 characters;
normalize empty or whitespace-only values to null.
###  3.3 Status model

Avoid a status model that is broader than the use cases.

A reasonable initial set is:

Pending;
InProgress;
Completed.

An even simpler model could be:

Open;
Completed.

For an architecture exercise, three statuses provide enough lifecycle behavior without creating artificial complexity.

Recommended transitions:

Pending -> InProgress
Pending -> Completed
InProgress -> Pending
InProgress -> Completed
Completed -> Pending, only if reopening is supported

Alternatively, use an IsCompleted flag and avoid status transitions entirely. However, because the requested entity explicitly includes a status, an enum-based lifecycle is more aligned with the exercise.

### 3.4 Due date rules

Potential interpretations:

Option A — Future dates only

The due date must be today or later.

This is simple but prevents importing an already overdue task.

Option B — Any valid date

Past dates are allowed and represent overdue work.

This is more realistic for a task system.

Option C — Future on creation, controlled on update

New tasks cannot initially be overdue, but existing tasks may become overdue naturally.

I recommend Option C:

on creation, due date cannot precede the current local date;
after creation, the date may naturally pass;
editing an overdue task can retain its due date;
changing the date should not allow an invalid value according to the defined rule.

However, using “today” introduces time-zone questions.

### 3.5 Date and time semantics

Decide whether a due date means:

a calendar date, such as 2026-08-15; or
a timestamp, such as 2026-08-15T18:00:00Z.

The wording says due date, not due time. Therefore, I recommend:

domain type: DateOnly;
API format: ISO date, YYYY-MM-DD;
no timezone conversion for the due date itself.

This avoids the common bug where a date moves backward or forward because the browser and server apply timezone conversion.

Use UTC timestamps separately for:

CreatedAt;
UpdatedAt;
CompletedAt.
### 3.6 Ownership rule

Ownership must be immutable.

Once created:

the task belongs to the authenticated creator;
task ownership cannot be reassigned through normal update operations;
the application does not expose UserId as an editable field.
### 3.7 Concurrency rule

Two browser sessions could edit the same task.

For a basic exercise, last-write-wins may be acceptable. For a stronger implementation, add optimistic concurrency using:

a database concurrency token;
an ETag;
or a version field.

A conflict could then return 409 Conflict.

This is not essential, but documenting whether concurrency is handled or deferred improves architectural credibility.

## 4. Non-functional requirements

### 4.1 Maintainability

The system should:

use clear project and namespace boundaries;
enforce one-way dependency direction;
keep controllers or endpoint handlers thin;
avoid business logic in Angular components;
avoid persistence logic in application use cases;
use names based on business behavior;
keep public contracts separate from persistence entities;
document important decisions.

A reviewer should be able to identify:

where business rules live;
where a use case begins;
where persistence is implemented;
where authentication and ownership are enforced;
which tests prove each behavior.
### 4.2 Security

The system should:

use ASP.NET Core Identity or another established password-hashing implementation;
never store or log plaintext passwords;
authenticate all protected routes;
authorize access at both endpoint and resource-ownership levels;
derive the current user from trusted claims;
validate all client input;
avoid exposing stack traces;
use safe Problem Details responses;
use HTTPS outside local development;
avoid sensitive data in logs;
configure CORS narrowly;
define token or cookie storage deliberately;
protect seeded credentials from production use.

ASP.NET Core treats authentication and authorization as separate concerns: authentication establishes identity, while authorization determines what that identity may access. The design should preserve that distinction.

### 4.3 Testability

Business behavior must be testable without:

a running HTTP server;
a real database;
browser automation;
external authentication services.

The domain and application layers should be independently testable.

Integration tests should cover:

API routing;
serialization;
authentication;
authorization;
database mapping;
ownership filtering;
Problem Details;
real middleware behavior.
### 4.4 Reliability

The API should:

return deterministic status codes;
use consistent error response shapes;
support cancellation;
avoid partial state changes;
use database transactions where multiple writes form one operation;
safely handle duplicate or repeated requests where appropriate.

The frontend should:

show loading state;
prevent duplicate submissions;
recover from API errors;
not optimistically mutate critical state unless rollback is defined;
preserve a usable state after 401, 403, 404, 409, and 500 responses.

ASP.NET Core provides IProblemDetailsService and AddProblemDetails for standardized API error responses.

### 4.5 Performance

The expected data volume is low, but the implementation should still:

query only the current user’s tasks;
use asynchronous database access;
avoid loading all tasks before filtering;
index ownership and common sort/filter fields;
avoid unnecessary frontend requests;
cancel stale search or filtering requests where applicable.

Suggested indexes:

Tasks(UserId)
Tasks(UserId, Status)
Tasks(UserId, DueDate)
### 4.6 Usability and accessibility

The Angular frontend should include:

keyboard-accessible controls;
visible focus indicators;
semantic labels;
readable validation feedback;
adequate contrast;
responsive layout;
confirmation for destructive actions;
clear overdue and completed states;
accessible status indicators that do not rely only on color.
### 4.7 Observability

Even a small exercise should demonstrate:

structured application logging;
correlation or trace identifiers;
request-level error logging;
health endpoint;
environment-specific logging;
no sensitive credential or token logging.
### 4.8 Portability and setup

A new reviewer should be able to run the system from documented steps.

The repository should include:

prerequisites;
database setup;
migration command;
seed instructions;
backend run command;
frontend run command;
test commands;
demo credentials;
API URL configuration;
known limitations.

A containerized database is useful but should not make the whole project impossible to review when Docker is unavailable. SQLite may be acceptable for a demonstration, although PostgreSQL or SQL Server better demonstrates real relational integration.

## 5. Proposed high-level architecture
Recommended style

Clean Architecture inside a modular monolith, with a separate Angular SPA.

┌────────────────────────────────────────────────────┐
│ Angular 22 SPA                                     │
│                                                    │
│ Pages / Features / UI State / API Client           │
└───────────────────────┬────────────────────────────┘
                        │ HTTPS / JSON
                        ▼
┌────────────────────────────────────────────────────┐
│ ASP.NET Core 10 API                                │
│                                                    │
│ Routing / Auth / Authorization / Problem Details   │
│ Request mapping / OpenAPI / Middleware             │
└───────────────────────┬────────────────────────────┘
                        │ invokes
                        ▼
┌────────────────────────────────────────────────────┐
│ Application Layer                                  │
│                                                    │
│ Use cases / Commands / Queries / Validation        │
│ Interfaces / DTO projections / Authorization rules │
└───────────────────────┬────────────────────────────┘
                        │ depends on
                        ▼
┌────────────────────────────────────────────────────┐
│ Domain Layer                                       │
│                                                    │
│ Task aggregate / TaskStatus / Business rules       │
│ Domain errors / Value concepts                     │
└────────────────────────────────────────────────────┘

  Infrastructure implements Application interfaces

┌────────────────────────────────────────────────────┐
│ Infrastructure                                     │
│                                                    │
│ EF Core / Identity / Database / Time / Migrations  │
│ Repository or DbContext implementations / Seeding  │
└───────────────────────┬────────────────────────────┘
                        ▼
               ┌─────────────────┐
               │ SQL Database    │
               └─────────────────┘
## 6. Dependency direction

The core dependency rule should be:

Domain <- Application <- API
          ^
          |
     Infrastructure

More precisely:

Domain depends on nothing project-specific.
Application depends on Domain.
Infrastructure depends on Application and Domain.
API depends on Application and references Infrastructure only for composition and startup registration.
Angular communicates only through HTTP contracts.

A practical .NET solution could contain:

src/
  TaskManager.Domain/
  TaskManager.Application/
  TaskManager.Infrastructure/
  TaskManager.Api/

frontend/
  task-manager-web/

tests/
  TaskManager.Domain.Tests/
  TaskManager.Application.Tests/
  TaskManager.Infrastructure.IntegrationTests/
  TaskManager.Api.IntegrationTests/

This is sufficient. I would not create separate projects for every technical pattern.

## 7. Main backend components
### 7.1 Domain layer

The domain layer contains business concepts and invariants.

Main aggregate

A naming issue exists: Task conflicts conceptually with System.Threading.Tasks.Task.

Prefer a business-specific name such as:

TodoItem;
UserTask;
WorkItem;
TaskItem.

I recommend TaskItem.

Possible shape:

TaskItem
- Id
- OwnerId
- Title
- Description
- Status
- DueDate
- CreatedAt
- UpdatedAt
- CompletedAt
Domain behavior

Instead of public property setters, prefer meaningful behavior:

TaskItem.Create(...)
task.UpdateDetails(...)
task.Start(...)
task.Complete(...)
task.Reopen(...)

This centralizes invariants.

However, avoid manufacturing domain complexity. This is not necessarily a full Domain-Driven Design system. A focused entity with controlled state transitions is enough.

Domain value concepts

Potential value objects:

TaskTitle;
perhaps UserId.

But creating value objects for every primitive may be excessive for this exercise.

A balanced option:

use a TaskTitle value object only if it meaningfully centralizes normalization and length rules;
use strongly typed IDs only if they remain easy to map, serialize, and explain.
### 7.2 Application layer

The application layer coordinates use cases.

Suggested use cases:

Authentication
- RegisterUser
- LoginUser
- GetCurrentUser

Tasks
- CreateTask
- GetTask
- ListTasks
- UpdateTask
- CompleteTask
- ReopenTask
- DeleteTask

Each use case should be responsible for:

receiving an application request;
validating use-case-specific rules;
loading required state;
checking ownership;
invoking domain behavior;
persisting changes;
returning a result or defined application error.
Commands and queries

CQRS can be used as a code organization technique without separate databases or messaging.

Example:

CreateTaskCommand
UpdateTaskCommand
CompleteTaskCommand
DeleteTaskCommand

GetTaskByIdQuery
GetCurrentUserTasksQuery

This is reasonable, but introducing a mediator library is optional.

Important distinction:

CQRS organization does not require MediatR.

Direct use-case services or handlers can be clearer in a small exercise.

### 7.3 Infrastructure layer

Responsibilities:

EF Core DbContext;
entity mappings;
database migrations;
ASP.NET Core Identity persistence;
current time provider;
seeding;
application interface implementations;
database transaction behavior.
Repository decision

There are two defensible options.

Option 1 — Repository abstraction

Application defines:

ITaskRepository
IUnitOfWork

Infrastructure implements them.

Advantages:

obvious boundary;
application tests can use fakes;
persistence details are hidden.

Risks:

generic repositories often duplicate EF Core;
query capabilities can become awkward;
projection and pagination may be inefficient.
Option 2 — Application database abstraction

Application defines a narrow interface such as:

IApplicationDbContext

It exposes only needed sets and save behavior.

Advantages:

less abstraction ceremony;
EF Core query composition remains strong;
common in pragmatic Clean Architecture implementations.

Risk:

application becomes indirectly coupled to EF-style query semantics.

For this exercise, I would choose either:

specific task repository interfaces, not a generic repository; or
a narrowly scoped application DbContext interface.

I would reject a generic IRepository<T> because it usually weakens rather than improves the design.

### 7.4 API layer

Responsibilities:

route definitions;
authentication configuration;
authorization policies;
request model binding;
mapping request contracts to application requests;
mapping outcomes to HTTP responses;
Problem Details;
OpenAPI;
dependency composition.

Controllers or minimal APIs are both valid.

Recommendation

Use endpoint groups or thin controllers, based on what is easiest to explain consistently.

Controllers may be slightly easier for a traditional architecture review:

AuthController
TasksController
PublicController

Minimal APIs can also be clean if endpoint definitions do not contain business behavior.

The important architectural decision is not controller versus minimal API. It is whether transport concerns remain separate from business use cases.

## 8. Authentication and authorization design
### 8.1 Recommended identity implementation

Use ASP.NET Core Identity for:

user storage;
password hashing;
password policy;
login validation;
identity lifecycle.

Do not create custom password hashing.

### 8.2 Cookie versus JWT
Secure cookie approach

Best when:

SPA and API are deployed under the same site;
browser use is the only target;
the team wants to minimize token exposure to JavaScript.

Advantages:

HttpOnly cookies reduce token theft through direct JavaScript access;
browser manages cookie transport.

Requirements:

CSRF protection;
appropriate SameSite;
HTTPS;
credential-aware CORS if origins differ.
JWT approach

Best when:

frontend and API are clearly separated;
mobile or third-party clients may be added;
token-based authentication is part of the exercise.

Advantages:

easy to demonstrate with Swagger and API clients;
clear API authentication model.

Risks:

insecure browser storage;
refresh-token complexity;
revocation semantics;
accidental token logging.
Architectural recommendation

For an assessment-oriented Angular plus API project, I would choose one of these two explicit profiles:

Preferred security profile
ASP.NET Core Identity;
secure HttpOnly cookie;
Angular and API deployed on the same site or carefully configured origins;
CSRF protection.
Preferred demonstration profile
ASP.NET Core Identity;
short-lived JWT access token;
no refresh token unless long sessions are required;
keep the token in memory where practical;
document logout and expiration limitations.

For a small exercise, adding refresh-token rotation can consume substantial effort without improving the core task-management demonstration.

### 8.3 Ownership authorization

Endpoint authentication alone is insufficient.

This is unsafe:

GET /tasks/{taskId}
-> retrieve task by ID
-> return it

Correct logic must constrain access:

GET task WHERE task.Id = requestedId
         AND task.OwnerId = currentUserId

This should be enforced at multiple levels:

protected endpoint;
current-user identity service;
ownership-aware query;
application-level ownership rule;
tests using two different users.

Do not load all tasks and filter them in Angular.

## 9. API design

A concise API surface could be:

POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/logout
GET    /api/auth/me

GET    /api/tasks
GET    /api/tasks/{id}
POST   /api/tasks
PUT    /api/tasks/{id}
POST   /api/tasks/{id}/complete
POST   /api/tasks/{id}/reopen
DELETE /api/tasks/{id}

GET    /api/public/info
GET    /health
Suggested status codes
Registration
201 Created     account created
400 Bad Request invalid input
409 Conflict    duplicate email or username
Login
200 OK           authenticated
400 Bad Request  malformed input
401 Unauthorized invalid credentials

Avoid responses that reveal whether a particular account exists.

Create task
201 Created
400 Bad Request
401 Unauthorized

Use a location header pointing to the created resource.

List tasks
200 OK
401 Unauthorized

An empty collection should return 200 OK with an empty array or paged result, not 404.

Get task
200 OK
401 Unauthorized
404 Not Found
Update
200 OK or 204 No Content
400 Bad Request
401 Unauthorized
404 Not Found
409 Conflict, if concurrency is supported
Delete
204 No Content
401 Unauthorized
404 Not Found

## 10. Error model

All errors should follow a consistent Problem Details shape.

Example:

{
  "type": "https://example.test/problems/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/tasks",
  "traceId": "00-...",
  "errors": {
    "title": [
      "Title is required."
    ],
    "dueDate": [
      "Due date cannot be before today."
    ]
  }
}

Define a small error taxonomy:

validation;
unauthorized;
forbidden, if exposed;
not found;
conflict;
unexpected error.

Do not expose:

stack traces;
database exception messages;
table names;
SQL;
token details;
sensitive user information.

ProblemDetails is specifically intended as a machine-readable HTTP API error representation, and ASP.NET Core also provides validation-oriented Problem Details support.

##11. Angular architecture
Recommended frontend style

Use:

standalone components;
lazy-loaded feature routes;
signals for local and feature state;
a thin API client layer;
feature-oriented folders;
centralized session state;
no global state library unless complexity justifies it.

Angular officially supports signals as a granular reactive state mechanism, and Angular 22 includes stable Signal Forms capabilities.

Suggested structure
src/app/
  core/
    auth/
      auth.service.ts
      auth.store.ts
      auth.guard.ts
      auth.interceptor.ts
    api/
      api-error.ts
      problem-details.ts
    layout/
    config/

  shared/
    ui/
      loading-indicator/
      error-message/
      confirm-dialog/
    utilities/

  features/
    auth/
      pages/
        login-page/
        register-page/
      components/
      models/

    tasks/
      pages/
        task-list-page/
        task-create-page/
        task-edit-page/
      components/
        task-list/
        task-card/
        task-form/
        task-filters/
      data-access/
        task-api.service.ts
        task.store.ts
      models/
        task.model.ts
        task-status.ts
        task-query.ts

  app.routes.ts
  app.config.ts
Component responsibilities
Pages

Responsible for:

route-level orchestration;
loading feature state;
passing state to components;
navigating after successful operations.
Presentational components

Responsible for:

displaying inputs;
emitting user intent;
no direct HTTP calls;
limited local UI state.
API services

Responsible for:

typed HTTP calls;
request and response contracts;
no visual state.
Feature store

Responsible for:

task collection state;
selected task;
filters;
loading flags;
operation errors;
state transitions after create, update, complete, or delete.
State recommendation

For this scope, avoid NgRx unless the exercise specifically expects it.

A signals-based feature store is sufficient:

tasks
selectedTask
filter
isLoading
isSaving
error

Computed state can derive:

filteredTasks
overdueTasks
completedCount
pendingCount

A state library would become justified if the application later introduces:

complex cross-feature workflows;
offline synchronization;
undo/redo;
extensive normalized entities;
complex side-effect coordination.
# 12. Frontend error and session behavior

The frontend should deliberately handle:

400
map validation errors to fields;
retain user-entered values;
show general errors when not field-specific.
401
clear invalid session state;
redirect to login;
preserve intended destination where appropriate.
403
display an access-denied message if the API exposes forbidden responses.
404
show that the task does not exist or is unavailable;
return to the list without crashing.
409
explain that the task changed or the requested operation conflicts;
reload current server state.
500
show a nontechnical message;
preserve the current page where possible;
offer retry;
display a trace ID when available for diagnostics.
##13. Testing strategy

A strong test pyramid is more valuable than trying to maximize raw test count.

### 13.1 Domain tests

Fast unit tests covering:

title validation;
creation rules;
due-date rules;
status transitions;
completion timestamp;
reopening;
prohibited transitions;
normalization.

These tests should use no mocks if possible.

### 13.2 Application tests

Test use cases with controlled dependencies:

create task assigns authenticated user;
list returns only current user’s tasks;
update rejects inaccessible tasks;
delete rejects inaccessible tasks;
complete invokes valid transition;
not found is returned appropriately;
cancellation is passed through;
persistence is called only on successful operations.

Mocks or hand-written fakes are appropriate here.

### 13.3 Infrastructure integration tests

Test against a real relational provider where feasible:

EF mappings;
migrations;
indexes;
constraints;
identity storage;
seeding;
queries by owner;
date mapping;
concurrency behavior, if supported.

Avoid relying exclusively on EF Core’s in-memory provider because its behavior can differ from a relational database.

### 13.4 API integration tests

Run the actual ASP.NET Core pipeline and verify:

anonymous public endpoint succeeds;
anonymous task access returns 401;
valid login succeeds;
invalid login returns 401;
user A cannot retrieve user B’s task;
user A cannot modify user B’s task;
invalid requests return Problem Details;
create returns 201;
delete returns 204;
nonexistent task returns 404;
seeded credentials work.

These are among the most important tests because authentication and middleware behavior are difficult to prove using isolated unit tests alone.

### 13.5 Angular unit tests

Cover:

feature store state transitions;
filters and computed state;
form validation;
error mapping;
auth guard;
API interceptor;
key presentational components.
### 13.6 End-to-end tests

Keep the set small and valuable:

register or log in;
create a task;
view it in the list;
edit it;
mark it complete;
delete it;
verify protected-route behavior;
verify ownership isolation, if practical.

### 14. Main technical risks
Risk 1 — Overengineering Clean Architecture

Possible symptoms:

too many projects;
interfaces with only one trivial implementation;
generic repository plus unit of work plus mediator plus pipeline behaviors;
domain events without consumers;
mapping through five object types;
complex result wrappers around simple operations.

Mitigation:

create boundaries only where they protect a meaningful concern;
document why each abstraction exists;
prefer direct, testable use-case handlers;
keep the deployment as a modular monolith.
Risk 2 — Underengineering ownership security

Possible symptoms:

UI hides other users’ tasks but API does not;
API queries by task ID only;
update request accepts userId;
controller checks authentication but not ownership.

Mitigation:

ownership-aware database queries;
current-user abstraction;
two-user integration tests;
immutable owner ID.
Risk 3 — Ambiguous due-date behavior

Possible symptoms:

frontend and backend disagree;
UTC conversion changes dates;
tasks become overdue unexpectedly;
server tests depend on the actual current date.

Mitigation:

use DateOnly;
inject a clock or date provider;
document create/update rules;
test boundary dates.
Risk 4 — Improper token handling

Possible symptoms:

long-lived JWT in local storage;
tokens logged;
refresh token without rotation;
logout presented as revocation when it is not;
CORS configured as AllowAnyOrigin.

Mitigation:

choose and document an authentication model;
use secure cookie where appropriate;
keep access tokens short-lived;
restrict origins;
test expiry and unauthorized behavior.
Risk 5 — Identity leaking into the domain model

ASP.NET Identity’s user entity should not dominate all business design.

Mitigation:

represent ownership through an identifier;
keep login and credential behavior in identity infrastructure;
avoid making task behavior depend directly on Identity framework types.
Risk 6 — Frontend state becoming inconsistent

Possible symptoms:

deleted task remains visible;
failed completion still appears complete;
repeated clicks create duplicates;
401 leaves the UI believing the user is logged in.

Mitigation:

centralize feature state;
model loading and mutation states;
use immutable updates;
disable duplicate submissions;
reload or roll back after failure.
Risk 7 — Excessive mapping and DTO duplication

Separate contracts are appropriate, but excessive one-to-one models can become noise.

Mitigation:

Use clear distinctions:

API request contract;
application command/query;
domain entity;
API response.

Do not introduce another model layer unless it serves a concrete purpose.

Risk 8 — Seeded credentials leaking to production

Mitigation:

seed only in development or explicit demo environment;
use environment checks;
clearly label credentials;
never use production secrets in source control;
optionally require a configuration flag.
Risk 9 — Tests that prove implementation rather than behavior

Possible symptoms:

tests assert every mocked call;
controllers are heavily unit-tested while the real pipeline is not;
authorization attributes are never integration-tested;
implementation refactors break many tests.

Mitigation:

prioritize observable behavior;
use domain tests and API integration tests;
mock only true boundaries;
avoid tests tied to private method structure.
Risk 10 — GenAI evidence contaminating production documentation

The requirement asks for prompt-engineering evidence. This should not mean committing every exploratory prompt or generated note.

Mitigation:

Create a curated section such as:

docs/
  architecture/
  decisions/
  testing/
  genai-evidence/

The GenAI evidence should show:

selected prompts;
expected output;
how output was reviewed;
what was accepted;
what was rejected;
corrections made by the developer;
validation commands and test results.

This demonstrates responsible use rather than unreviewed code generation.

##15. Architecture decision records

A small set of ADRs would strengthen the exercise considerably.

Suggested ADRs:

ADR-001: clean-architecture-modular-monolith.md
ADR-002: sql-server-localdb.md
ADR-003: identity-jwt.md
ADR-004: angular-spa.md
ADR-005: testing-strategy.md
ADR-006: taskitem-domain-model.md
ADR-007: application-abstractions.md
ADR-008: cross-user-404.md
ADR-009: single-dbcontext.md
ADR-010: jwt-storage-deferred.md
ADR-011: spa-token-storage.md
 

Each ADR can be brief:

context;
decision;
alternatives;
consequences.
##16. Suggested deployment structure

For an architecture exercise, keep deployment simple.

Option A — Single origin
Angular static files
        +
ASP.NET Core API
        +
SQL database

The API hosts or sits behind the same origin as the frontend.

Advantages:

simpler authentication cookies;
simpler CORS;
simple deployment story.
Option B — Separate frontend and API
Angular static hosting
        |
        | HTTPS
        ▼
ASP.NET Core API
        |
        ▼
SQL database

Advantages:

realistic frontend/backend separation;
independent deployment.

Costs:

CORS;
cookie-domain or token decisions;
more deployment configuration.

For a technical exercise, either is valid. If cookie authentication is chosen, Option A is generally simpler and safer.

## 17. Suggested data model
User
----
Id
Email
NormalizedEmail
PasswordHash
DisplayName
CreatedAt
...

TaskItem
--------
Id
OwnerId
Title
Description
Status
DueDate
CreatedAt
UpdatedAt


Relationship:

User 1 ─────────── * TaskItem

Constraints:

Title: required, bounded length
Description: optional, bounded length
Status: required
DueDate: required
OwnerId: required foreign key

Invariant:

Status == Completed  => CompletedAt is not null
Status != Completed  => CompletedAt is null

Whether that invariant is enforced only in the domain or also by a database constraint is a design decision. Defense in depth is desirable, but complicated database checks may reduce provider portability.

## 18. Suggested task query response

Instead of returning a bare array, a lightweight paged result is more extensible:

{
  "items": [
    {
      "id": "7f...",
      "title": "Prepare architecture review",
      "description": "Review API boundaries and tests",
      "status": "inProgress",
      "dueDate": "2026-08-01",
      "createdAt": "2026-07-28T12:00:00Z",
      "updatedAt": "2026-07-28T12:30:00Z",
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}

isOverdue is a useful read-model projection. It does not necessarily need to be stored because it can be derived from status, due date, and the current date.

## 19. Recommended implementation boundaries

Domain owns
valid task state;
status transitions;
title and date invariants;
completion behavior.
Application owns
use-case orchestration;
current-user requirement;
ownership decisions;
persistence coordination;
application-level validation;
not-found and conflict outcomes.
Infrastructure owns
database;
EF Core;
Identity persistence;
seed execution;
technical clock implementation;
migrations.
API owns
HTTP semantics;
authentication middleware;
request and response contracts;
status codes;
Problem Details;
OpenAPI.
Angular owns
presentation;
browser interaction;
feature state;
client-side validation for usability;
route protection for user experience.

Angular route guards are not security boundaries. The backend remains authoritative.

## 20. Recommended scope for the first implementation
Must-have
registration and login;
authenticated task CRUD;
complete operation;
ownership isolation;
one anonymous endpoint;
seeded two-user demonstration;
consistent Problem Details;
Angular responsive UI;
unit and integration tests;
setup guide;
architecture diagram;
ADRs;
curated GenAI evidence.
Valuable additions
filtering by status;
sorting by due date;
overdue indicator;
pagination;
reopen operation;
optimistic concurrency;
health checks;
structured logging and trace IDs.
Defer unless explicitly required
task sharing;
teams;
roles beyond authenticated user;
subtasks;
attachments;
reminders;
email notifications;
recurrence;
real-time updates;
offline mode;
audit history;
microservices;
event sourcing;
message queues;
external identity provider;
elaborate refresh-token infrastructure.

## 21. Final architectural recommendation

The best design for this exercise is:

A Clean Architecture modular monolith using ASP.NET Core 10, ASP.NET Core Identity, EF Core, a relational database, standardized Problem Details, and an Angular 22 standalone SPA organized by features with signals-based state.

The most important architectural qualities to demonstrate are not the number of patterns used. They are:

Ownership is secure by construction.
Business behavior is testable outside the API and database.
HTTP behavior is consistent and integration-tested.
Frontend state remains predictable after success or failure.
Dependencies point toward business behavior.
Architectural choices are documented and proportionate to the problem.
GenAI output is visibly reviewed rather than blindly accepted.

The main unresolved decisions to review before implementation are:

cookie authentication versus JWT;
exact task status transitions;
precise due-date rules;
hard versus soft deletion;
repository abstraction versus application DbContext abstraction;
whether concurrency control belongs in the first version;
database provider;
controllers versus minimal API endpoint groups.

My initial preference is: secure cookie authentication, DateOnly due dates, hard deletion, three task statuses, specific application use-case handlers, a narrow persistence abstraction, thin controllers, SQL-backed integration tests, and Angular signals without NgRx.