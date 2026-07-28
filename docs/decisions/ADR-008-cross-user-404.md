# ADR-008 — Return 404, not 403, for another user's task

## Status

Accepted

## Context

`GET/PUT/PATCH/DELETE /api/tasks/{id}` all need to reject a request for a task that exists but
belongs to a different user. Two conventional choices exist: `403 Forbidden` (the resource exists,
but you're not allowed to see it) or `404 Not Found` (as far as you're concerned, this resource
doesn't exist).

## Decision

**Return `404 Not Found`, not `403 Forbidden`, when a task exists but belongs to another user** —
identical to the response for a task ID that doesn't exist at all.

This is enforced structurally, not as an extra check: `ITaskRepository.GetByIdAsync(taskId, ownerId,
...)` always takes the current user's id as part of the lookup (see
[ADR-007](ADR-007-application-abstractions.md)), so a task belonging to someone else and a
nonexistent task produce the exact same `null` result in Application — there is no code path that
distinguishes "not found" from "found but not yours," which makes the two cases both accidentally
*and* deliberately indistinguishable at the API layer (`GetTaskByIdHandler`, `UpdateTaskHandler`,
`ChangeTaskStatusHandler`, `DeleteTaskHandler` all return the same `NotFound` `UseCaseError`).

The alternative — checking existence first, then ownership, then returning `403` — would require an
extra unscoped lookup and would leak the fact that *a* task with that ID exists to a user who has no
right to know that, for essentially no benefit in this application (there is no legitimate reason for
a client to need to distinguish "not yours" from "doesn't exist" here).

## Consequences

* Every cross-user access attempt in the API integration tests asserts `404`, not `403` — see
  `GetTaskById_WhenAnotherUsersTask_ShouldReturnNotFound` and the delete/update equivalents in
  `Ballastlane.Tasks.Api.IntegrationTests`.
* If a future requirement needs to distinguish "not found" from "found but forbidden" (e.g. shared
  tasks with partial visibility), this decision would need to be revisited — not needed for personal,
  single-owner tasks.
