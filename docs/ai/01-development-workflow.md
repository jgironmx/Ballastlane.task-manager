# 01 — Development workflow

How this project was actually built: the division of labor between the developer, ChatGPT, and
Claude Code, followed by representative excerpts of the prompts used to direct scoped implementation
work.

## Workflow description

*The following is a factual description of the development workflow, supplied by the repository
owner.*

Architectural analysis, delivery planning, acceptance criteria, and technical review were supported
by ChatGPT. 
Scoped implementation tasks, repository-analysis tasks were executed with Claude Code, directed by a detailed, constrained prompt per
implementation phase. 
Final decisions on architecture, security, validation, technical review were and acceptance remained the developer's responsibility throughout: no generated change was accepted automatically. 
Acceptance was based on code review, builds, unit tests, integration tests, architecture tests, vulnerability
checks, and live validation against the running application.

This is a two-tool workflow, not a single generate-and-ship step, which is why
[`03-review-findings.md`](03-review-findings.md) and
[`04-corrections.md`](04-corrections.md) contain
real findings and real corrections rather than being empty.

## Representative excerpts

The full prompts used to direct each implementation phase 
The excerpts below are genuinely representative and are
reproduced exactly as given to Claude Code.

### Pagination and filtering verification

```text
The task list uses default pagination.

Validate:

* Omitting `page` and `pageSize` works.
* Invalid zero or negative values produce `400`.
* Excessively large page sizes are bounded or rejected.
* Angular consumes the real paginated response correctly.
* Empty results render correctly.
* Seed data appears predictably.

Do not implement advanced sorting or filtering unless already supported.
```

### Production API URL strategy

```text
Resolve the current missing production Angular API URL strategy.

Choose one simple approach:

### Option A — relative `/api`

Use when Angular and the API are expected to be served under the same origin.
...
Preferred choice for this exercise:

Production Angular API base URL: relative path
Development Angular API base URL: configured localhost HTTPS URL

Do not introduce runtime configuration infrastructure unless needed.

Ensure frontend tests cover API URL construction where practical.
```

### Git precondition check

```text
Before making changes:

git status
git log --oneline --decorate -5

Confirm that the current implementation is committed.

If it is not committed, stop and report the exact Git state.
```

This last excerpt is included because following it literally — actually running the commands and
reading the output rather than assuming the repository was in a known state — is what surfaced the
`Directory.Build.props` / `Directory.Packages.props` gap documented in
[`03-review-findings.md`](03-review-findings.md).
