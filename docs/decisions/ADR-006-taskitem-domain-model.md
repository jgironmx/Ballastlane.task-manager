# ADR-006 — TaskItem domain model

## Status

Accepted

## Context

`Ballastlane.Tasks.Domain` needed its first entity: `TaskItem`, owned by exactly one user, with a
title, optional description, status, and optional due date. Two design questions came up that the
architecture baseline (ADR-001) did not already answer: what type represents the due date, and how
timestamps are assigned without giving Domain a dependency on the system clock.

## Decision

**`DueDate` is `DateOnly?`; `CreatedAtUtc`/`UpdatedAtUtc` are `DateTimeOffset`/`DateTimeOffset?`.**

A task's due date is a business-calendar concept ("finish by March 3rd") with no meaningful
time-of-day or timezone component — modeling it as `DateTimeOffset` would invite bugs from comparing
a date against an instant across timezones (e.g. "is 2026-03-03T23:00Z before or after March 3rd?").
`DateOnly` makes that comparison unambiguous. `CreatedAtUtc`/`UpdatedAtUtc`, by contrast, are audit
instants — exactly the case `DateTimeOffset` is for, and consistent with the root README's UTC-instant
convention for persisted timestamps.

**`TaskItem.Create`/`UpdateDetails` take `nowUtc`/`currentBusinessDate` as parameters rather than
reading the clock themselves.** Domain must not depend on `DateTimeOffset.UtcNow` directly (see
[ADR-005](ADR-005-testing-strategy.md)): a static clock call inside
an entity makes creation non-deterministic and therefore harder to unit test (e.g. the "due date can't
be in the past" invariant). Instead, `Ballastlane.Tasks.Application`'s `IClock` abstraction supplies
`UtcNow`, and the use case passes it into the Domain factory/update methods. Domain stays framework-
and clock-free; determinism is a caller concern.

**The "due date can't be earlier than today" invariant is enforced only at creation, not on every
update.** An existing task's due date can legitimately drift into the past simply because time passed
while the task sat open — re-validating it on every unrelated edit (e.g. editing only the description)
would make already-overdue tasks impossible to touch. Update validates title/description length the
same way creation does, but does not re-check the due date against "today."

**`TaskItem` is mutated only through named methods (`UpdateDetails`, `ChangeStatus`), never through
public setters**, and `OwnerId` has no mutator at all after `Create` — ownership is fixed for the life
of the task. All property setters are `private`; EF Core (introduced in Infrastructure) materializes
instances through the parameterless private constructor and property reflection, so this does not
require compromising encapsulation for persistence.

## Consequences

* Comparing a due date against "today" is a plain `DateOnly` comparison — no timezone-normalization
  bugs.
* Every `TaskItem.Create`/`UpdateDetails`/`ChangeStatus` call site in Application must supply
  `IClock.UtcNow` (and its `DateOnly` projection) explicitly; this is slightly more ceremony than
  calling `DateTimeOffset.UtcNow` inline, in exchange for deterministic domain unit tests.
* If a future requirement needs due-*times* (not just due-dates), `DueDate` would need to become
  `DateTimeOffset?` and this ADR would need to be revisited — not anticipated for this exercise.
