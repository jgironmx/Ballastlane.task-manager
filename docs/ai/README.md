# AI-assisted engineering workflow

This directory records how AI assistance was used during this project.

## Workflow

Architectural analysis, delivery planning, and acceptance criteria, were supported by ChatGPT in multiples iterations.
Scoped implementation tasks were executed with Claude Code providing a prompt in each iteration. 
Final decisions on architecture, security, validation, and technical review were supported, and acceptance remained the developer's responsibility throughout.

## Ground rules

* **AI is used as an implementation and analysis assistant, not as the final decision-maker.**
  Architecture, dependency rules, and technology decisions (see [`docs/decisions/`](../decisions/))
  are developer-owned; AI tools were used to accelerate analysis and implementation within
  constraints the developer set.
* **All generated code is reviewed** before being accepted, read, understood, and checked against
  the architecture rules, not merged unread.
* **Acceptance criteria are objective:** a change is only kept if it builds, its tests pass, the
  architecture tests still pass, and it does not introduce security issues (e.g., secrets in source,
  unsafe deserialization, missing authorization).
* **Representative generated code is preserved** as evidence, alongside representative excerpts of
  the prompts that produced it, so the process can be inspected later.


## Initial Prompts

* [`prompts/prompt-0-initial-chatgpt-prompt.md`](prompt-0-initial-chatgpt-prompt.md) — Initial prompt provided to ChatGPT for the architecture definition.
* [`prompts/prompt-1-increment-0.md`](prompt-1-increment-0.md) — Sprint 2: Repository and Architectural Baseline.
* [`prompts/prompt-2-sprint-2-backend.md`](prompt-2-sprint-2-backend.md) — Sprint 2: Complete Backend Implementation
* [`prompts/prompt-3-sprint-3-angular.md`](prompt-3-sprint-3-angular.md) — Sprint 3: Angular Authentication and Task CRUD


## Evidence files

* [`01-development-workflow.md`](01-development-workflow.md) — the division of labor between
  ChatGPT and Claude Code, and representative excerpts of the prompts used to direct Claude Code's
  implementation work.
* [`02-representative-output.md`](02-representative-output.md) — representative generated code,
  before/after, for real defects found and fixed.
* [`03-review-findings.md`](03-review-findings.md) — issues found during review (correctness,
  architecture-rule violations, security, consistency), across the project's implementation phases.
* [`04-corrections.md`](04-corrections.md) — what was changed in response to each finding, and why.
* [`05-validation-results.md`](05-validation-results.md) — actual output of build, test,
  architecture-test, format, and vulnerability-audit commands run against the corrected code.

## Status

All content in this directory is drawn from actual project history — commands that were actually
run, code that was actually written, tests that actually failed and then passed. Where a document describes the ChatGPT-supported
phase of the workflow, it is stated as a factual description supplied by the repository owner, not as
a verbatim transcript — no ChatGPT prompt or output is stored in this repository.
