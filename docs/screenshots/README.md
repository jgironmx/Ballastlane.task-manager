# Screenshots — manual capture checklist

No browser automation tool (`chromium-cli`, Playwright, Cypress) was available in the development
environment this project was built in, and installing one was out of scope (confirmed via
`which chromium-cli` / `which playwright` / `which cypress` — none found). No screenshots exist here
yet.

Whoever next has real browser access should capture the following, following
[`../qa/manual-walkthrough.md`](../qa/manual-walkthrough.md) for the exact steps to reach each
screen, and save them here with these exact filenames:

- [ ] `login.png` — the login screen (`/login`), empty state.
- [ ] `task-list.png` — the task list (`/tasks`) with the four seeded demo tasks visible, desktop
      width.
- [ ] `task-form.png` — the create-task form (`/tasks/new`), mid-fill, showing at least one field.
- [ ] `mobile-task-list.png` — the task list at a common mobile width (e.g. 375px, iPhone SE/12/13
      class), to confirm no horizontal scrolling or clipped content.
- [ ] `swagger.png` — the OpenAPI UI at `/openapi/v1.json` (or a Scalar/Swagger UI pointed at it),
      with the "Authorize" dialog open showing the bearer scheme.

**Do not use placeholder or stock images.** If real browser access isn't available when this is
next picked up, leave this checklist as-is rather than substituting anything — an honest gap is more
useful than a misleading screenshot.
