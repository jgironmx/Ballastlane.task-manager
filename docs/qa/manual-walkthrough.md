# Manual walkthrough

A step-by-step sequence for running the application end to end and confirming it behaves as
documented — useful for a contributor or reviewer verifying a clean checkout, or for exercising the
full user flow by hand. See the root [README](../../README.md) §2 for the underlying Quick Start
commands this walkthrough assumes.

## Before you start

```bash
# Terminal 1
cd src/Ballastlane.Tasks.Api
dotnet user-secrets set "Jwt:SigningKey" "local-dev-only-signing-key-not-for-prod-32chars-min"   # first time only
cd ../..
dotnet run --project src/Ballastlane.Tasks.Api

# Terminal 2
cd client/ballastlane-tasks-web
npm start
```

Have `https://localhost:7111/openapi/v1.json` (or your OpenAPI UI of choice) and
`http://localhost:4200` both open in browser tabs before starting.

## Sequence

1. **Start the API** — Terminal 1: `dotnet run --project src/Ballastlane.Tasks.Api`; confirm the log
   shows migrations applied, demo user seeded, listening on `http://localhost:5276` /
   `https://localhost:7111`.
2. **Start Angular** — Terminal 2: `npm start`; confirm the log shows the dev server ready at
   `http://localhost:4200`.
3. **Log in using demo credentials** — open `http://localhost:4200`, confirm the redirect to
   `/login` (unauthenticated -> `authGuard` redirect), then log in with
   `demo@ballastlane.local` / `Demo1234!` (root README §14).
4. **Confirm the seeded tasks** — the four demo tasks (one pending, one in-progress, one completed,
   one with a future due date), and the status/search filters.
5. **Create a valid task** — title + description + a future due date; confirm the new task appears
   immediately (fresh `GET /api/tasks` after the mutation, no optimistic client cache — root
   README §17).
6. **Confirm client-side validation** — attempt to submit with an empty title, and attempt a past
   due date on create; both should be rejected client-side before any request is sent.
7. **Edit the created task** — change its title/description; confirm past-due validation is relaxed
   on edit (a task already overdue should still be editable).
8. **Change its status** — cycle it `Pending` -> `InProgress` -> `Completed` using the per-row
   status control; confirm status is shown with text, not color alone.
9. **Delete it** — trigger the confirmation dialog, cancel once to confirm it doesn't delete
   prematurely, then confirm.
10. **Log out** — use the header's logout action; confirm the session clears and the app redirects
    to `/login`.
11. **Confirm the protected-route redirect** — attempt to navigate directly to
    `http://localhost:4200/tasks` while logged out; confirm the redirect back to `/login` with a
    `returnUrl`.
12. **Open the OpenAPI UI**.
13. **Confirm JWT authorization** — click "Authorize," paste in a JWT copied from a fresh
    `POST /api/auth/login` response, call `GET /api/tasks` directly from the UI; then remove
    authorization and call it again to confirm the `401` Problem Details body
    (`urn:ballastlane-tasks:error:auth.required`).
14. **Run the test suites** — `dotnet test --configuration Release` and
    `npm test -- --watch=false` in a spare terminal, or check the CI run.
15. **Review the architecture diagrams** — [`../architecture/solution-overview.md`](../architecture/solution-overview.md)
    and [`../architecture/diagrams.md`](../architecture/diagrams.md).
16. **Review the AI-assisted engineering evidence** — [`../ai/03-review-findings.md`](../ai/03-review-findings.md)
    and [`../ai/04-corrections.md`](../ai/04-corrections.md), e.g. the pagination-validation fix.

## Recovery steps

* **LocalDB is unavailable** — run `sqllocaldb start MSSQLLocalDB` (or `SqlLocalDB start MSSQLLocalDB`
  on a fresh machine that's never created the instance: `sqllocaldb create MSSQLLocalDB` first). If
  still failing, confirm `sqllocaldb info` lists the instance at all — if not, LocalDB itself isn't
  installed (see root README §8).
* **LocalDB startup fails entirely** — fall back to
  `dotnet test --configuration Release --filter "Category!=Integration"`, which doesn't need a
  database, and note that live LocalDB startup failed.
* **A port is already in use** — `netstat -ano | findstr :5276` (or `:7111` / `:4200`) to find the
  offending process, or override the port: `dotnet run --project src/Ballastlane.Tasks.Api --urls
  http://localhost:5300`, and update the Angular dev proxy/`API_BASE_URL` (see
  `client/ballastlane-tasks-web/README.md`) to match.
* **The HTTPS certificate is untrusted** — run `dotnet dev-certs https --trust` once, or just use the
  `http://localhost:5276` origin instead of the `https` one; both are equivalent for this purpose.
* **Seed data is missing** — confirm `ASPNETCORE_ENVIRONMENT=Development` (the seeder only runs in
  Development — root README §12); seeding is idempotent, so restarting the API is always safe.
* **The frontend API URL is wrong** — check `client/ballastlane-tasks-web/src/environments/
  environment.development.ts`'s `apiBaseUrl` matches the port the API actually started on (see the
  backend's own startup log).
* **`npm start` fails** — delete `client/ballastlane-tasks-web/.angular` (build cache) and retry; if
  it still fails, run `npm ci` again to rule out a corrupted `node_modules`, and confirm the Node
  version matches root README §8's prerequisite.
* **The OpenAPI UI doesn't load** — confirm the API is actually running in Development
  (`ASPNETCORE_ENVIRONMENT=Development` — it's intentionally not registered otherwise, root README
  §13); hit `/openapi/v1.json` directly first to confirm the document itself is being served before
  suspecting the UI.
