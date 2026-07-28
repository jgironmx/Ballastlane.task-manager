# Manual verification checklist

No browser automation tool (`chromium-cli`, Playwright, Cypress) was available in the environment
this project was built in (confirmed absent: none of the three were found on `PATH`). Everything in
this checklist was validated instead via automated component/unit tests and a manual HTTP-level
(`curl`) contract walkthrough against the real running backend — **not** an actual browser session.
This checklist is what still needs a contributor with a real browser to confirm. See
[`../screenshots/README.md`](../screenshots/README.md) for the matching screenshot checklist.

**As of this document's last update, no item below has been confirmed in an actual browser.** Do not
represent any item as verified until it has actually been checked.

## Setup

- [ ] Chrome or Edge, latest stable.
- [ ] Desktop width (e.g. 1440px+).
- [ ] Mobile responsive mode (DevTools device toolbar, e.g. 375px — iPhone SE/12/13 class).
- [ ] Browser console open (check for errors/warnings throughout, not just at the end).
- [ ] Network panel open (check for failed requests, unexpected status codes, or unexpectedly slow
      calls).

## Registration and login

- [ ] Register a new account with a valid email/password — succeeds, redirects appropriately.
- [ ] Register with an already-used email — shows a clear error, no console error.
- [ ] Register with an invalid password (too short, etc.) — client-side validation blocks submission
      before any request is sent.
- [ ] Log in with the demo account (`demo@ballastlane.local` / `Demo1234!`).
- [ ] Log in with a wrong password — generic error, doesn't reveal whether the email exists.
- [ ] **Refresh the page while logged in** — session persists, no forced re-login (`sessionStorage`
      restore).

## Task CRUD

- [ ] The four seeded demo tasks are visible after logging in as the demo user.
- [ ] Create a task with just a title — succeeds.
- [ ] Create a task with a title, description, and future due date — succeeds, due date displays
      correctly (see Date-only display below).
- [ ] **Invalid form**: submit with an empty title — client-side validation blocks it, no request
      sent (check Network panel).
- [ ] **Invalid form**: attempt a past due date on create — blocked client-side.
- [ ] Edit a task's title/description — updates immediately.
- [ ] Edit an already-overdue task — allowed (past-due validation is relaxed on edit, not create).
- [ ] Change a task's status through all three values — status is shown with text, not color alone.
- [ ] Delete a task — confirmation dialog appears; cancel once (task remains), then confirm (task is
      removed).

## Session and isolation

- [ ] **Session expiration simulation** (if practical): manually clear/corrupt the stored token via
      DevTools Application tab, then trigger an API call — confirm redirect to `/login` with a clear
      message, not a silent failure or a raw error dump.
- [ ] Log out — session clears, redirected to `/login`.
- [ ] **Cross-user isolation**: register/log in as a second user, confirm the first user's tasks are
      not visible.
- [ ] **Protected-route redirect**: while logged out, navigate directly to `http://localhost:4200/tasks`
      — redirected to `/login` with a `returnUrl`.

## API surface

- [ ] Open the OpenAPI UI (`/openapi/v1.json` or a UI pointed at it), confirm it loads (only expected
      in Development).
- [ ] Click "Authorize," paste a real JWT (`Bearer <token>` from a login response), call
      `GET /api/tasks` — succeeds.
- [ ] Remove authorization, call `GET /api/tasks` again — `401`, `application/problem+json` body.

## Accessibility

- [ ] **Keyboard navigation**: tab through the login form, task list, and task form without a mouse —
      focus order is logical, nothing is unreachable.
- [ ] **Dialog focus**: open the delete-confirmation dialog — focus moves into it (e.g. to the cancel
      button).
- [ ] **Escape** closes the delete-confirmation dialog.
- [ ] Form validation errors are announced/associated with their fields (not just colored red).

## Date-only display

- [ ] A task's due date displays as the same calendar day it was entered, regardless of the browser's
      local timezone — this is the specific bug class `DateOnly` handling is meant to prevent; worth
      testing with the OS timezone set behind UTC if possible (e.g. US Pacific) to actually exercise
      the scenario the implementation protects against.

## Security spot-check

- [ ] **No token sent to a third-party origin**: with DevTools Network panel open, trigger any
      request to a non-API origin if the app ever makes one (e.g. a favicon or font from a CDN) and
      confirm no `Authorization` header is present on it. This behavior is covered by an Angular unit
      test (`auth.interceptor.spec.ts`) but has not been confirmed via an actual browser network
      capture — worth a direct check.
