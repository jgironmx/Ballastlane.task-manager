# ADR-001 — Clean Architecture modular monolith

## Status

Accepted

## Context

Ballastlane.Tasks is a personal task-management application developed as a technical engineering
exercise: a single user-facing SPA, a single API, and a single relational database. The scope is
small (task CRUD, user ownership, authentication) and the timeline is short. The architecture needs
to:

* Demonstrate clear separation of concerns and dependency inversion, since that is an explicit goal of
  the exercise.
* Be testable in isolation (domain rules, use cases, HTTP contracts) without standing up a database or
  a web server for every test.
* Stay simple enough to implement and review within a constrained timeframe, without sacrificing the
  architectural rigor the exercise is meant to demonstrate.

## Decision

Use **Clean Architecture inside a modular monolith**: a single deployable API process internally
divided into `Domain`, `Application`, `Infrastructure`, and `Api` projects with an enforced,
one-directional dependency graph (`Domain <- Application <- Infrastructure`, `Api -> Application`,
`Api -> Infrastructure`). Dependency direction is verified by automated architecture tests, not just
documentation.

## Alternatives considered

**Microservices.** Rejected. The application has one bounded context (personal task management) and
one team. Microservices would add network boundaries, distributed transactions, service discovery, and
deployment complexity with no corresponding benefit at this scale — pure accidental complexity for an
interview exercise.

**Single-project layered application** (one project with folders for controllers, services, data
access). Rejected. Without separate assemblies, dependency direction is only a convention that the
compiler cannot enforce — nothing stops a "domain" class from calling into EF Core. A modular monolith
with project-level boundaries makes the dependency rules structurally enforceable and testable.

**Event sourcing.** Rejected. Task management here does not need an append-only event log, temporal
queries, or event replay. A conventional CRUD-oriented model with an EF Core-backed relational store is
sufficient and far simpler to build, test, and explain.

## Consequences

* Adding a feature typically touches multiple projects (Domain entity, Application use case,
  Infrastructure implementation, API endpoint) — more ceremony than a single-project app, but each
  piece is independently testable.
* The dependency rules are enforced by `tests/Ballastlane.Tasks.ArchitectureTests`, so violations fail
  CI rather than being caught only in review.
* Should the application later need to be split into independently deployable services, the existing
  project boundaries (Domain/Application/Infrastructure) already approximate service boundaries,
  reducing (but not eliminating) the cost of that future split.
