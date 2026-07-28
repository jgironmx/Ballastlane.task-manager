// Production API URL strategy (Sprint 4 §5, Option A): relative `/api`, not a hard-coded hostname.
// An empty base means every request URL (e.g. `${apiBaseUrl}/api/auth/login`) resolves relative to
// whatever origin actually serves this build, so it works unmodified behind any reverse proxy or
// same-origin ASP.NET Core host — no per-environment rebuild needed. This assumes the SPA and API
// are served from the same origin in production; if they are ever split across origins, replace
// this with the real deployed API origin (see ADR-004 for the alternative, build-time-value
// strategy). The API base URL itself is not a secret; nothing sensitive belongs in this file.
export const environment = {
  production: true,
  apiBaseUrl: '',
};
