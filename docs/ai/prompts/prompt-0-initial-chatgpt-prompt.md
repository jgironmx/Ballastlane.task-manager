
# Original Prompt — Initial architecture definition with ChatGPT

This is the initial prompt provided to ChatGPT for the architecture definition

[`docs/reports/final-report-0-initial-chatgpt-prompt.md`](final-report-0-initial-chatgpt-prompt.md), which reports what
was the response to it. 

See also the companion prompts for
[Sprint 1](prompt-1-increment-0.md).
[Sprint 2](prompt-2-sprint-2-backend.md).
[Sprint 3](prompt-3-sprint-3-angular.md).

---

Act as an expert Software Architect. 

The goal is to implement a personal task management system.

The tasks should contain:

	Title
	Description
	Status
	Due date
	Associated user

Proposed informal user story: 

	As an authenticated user, I want to create, view, update, complete, and delete my tasks so that I can organize my work and track upcoming deadlines.

Possible acceptance criteria:

	A user can register and log in.
	An authenticated user can create a task.
	A user sees only their own tasks.
	A task requires a title, status, and valid due date according to the agreed business rules.
	A user can edit or delete only their own tasks.
	Anonymous users cannot access protected task operations.
	At least one endpoint is intentionally public to demonstrate unauthenticated access.
	Seeded credentials and sample tasks are available for demonstration.

Identify the functional and non-functional requirements.
Identify potential technical risks, 
Define the main system components
Propose an appropriate high-level architectural structure for this case. 


Some Non-functional requirements

	Clean Architecture
	Separation of concerns
	Component independence
	TDD or strong automated testing
	Readable and organized code
	Responsive frontend in Angular
	Cleanly organized components and state
	Reliable functionality
	Setup documentation
	Seeded demonstration data
	Clear presentation
	GenAI prompt-engineering evidence

Inferred quality attributes

Maintainability

The project must be understandable during a code review. Names, project boundaries, dependency direction, and tests should make the design easy to explain.

Security

Passwords must never be stored directly. Authentication, authorization, ownership checks, token handling, validation, and safe error responses must be deliberate.

Testability

Business behavior should be testable without running a database or web server. Infrastructure and API behavior should also have targeted integration tests.

Reliability

Implementation:

The API should return predictable status codes and Problem Details responses. Angular should handle failed requests without crashing or leaving inconsistent UI state.

To implement this architecture exercise, we need to create a web api app using .NET 10, 

For the front-end, we should use Angular 22,

Provide me with ideas or interpretations about the architecture and design of this architecture exercise.

You don't need to implement code.

Only make the analysis, and after that, we could make a review and discuss the architecture and decisions to take for the best implementation.
