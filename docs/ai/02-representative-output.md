# 02 — Representative generated output

Two representative, complete examples of AI-generated code from Sprint 4, chosen because each
directly fixes a real defect found by following the prompt (see `03-review-findings.md`), not a
cosmetic change.

## Example 1 — Pagination validation (`ListTasksHandler.cs`)

Before (silently clamped invalid input instead of rejecting it):

```csharp
var page = Math.Max(query.Page, 1);
var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
```

After (`src/Ballastlane.Tasks.Application/Features/Tasks/List/ListTasksHandler.cs`):

```csharp
if (query.Page < 1)
{
    return Result.Failure<PagedResult<TaskDto>>(
        UseCaseError.Validation("tasks.invalid_page", "page must be 1 or greater."));
}

if (query.PageSize is < 1 or > MaxPageSize)
{
    return Result.Failure<PagedResult<TaskDto>>(
        UseCaseError.Validation("tasks.invalid_page_size", $"pageSize must be between 1 and {MaxPageSize}."));
}
```

## Example 2 — Consistent 401 Problem Details for authentication failures (`ApiServiceCollectionExtensions.cs`)

Before: `JwtBearerOptions` had no `Events`, so a missing/invalid/expired token fell through to
`JwtBearerHandler`'s own default 401 response — a different response body shape than every other
401 in the API (`UseCaseError.Unauthorized().ToProblem()`).

After:

```csharp
bearerOptions.Events = new JwtBearerEvents
{
    OnChallenge = async context =>
    {
        context.HandleResponse();

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication is required.",
            Type = "urn:ballastlane-tasks:error:auth.required",
        };

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
    },
};
```

## Example 3 — Same-origin check that doesn't break under a relative API base URL (`auth.interceptor.ts`)

Before:

```ts
if (!request.url.startsWith(baseUrl)) {
  return next(request);
}
```

This is vacuously `true` for every request when `baseUrl` is `''` (the production, relative-`/api`
configuration) — meaning the bearer token would attach to any outgoing HTTP request, including a
third-party one, in a production build.

After (`client/ballastlane-tasks-web/src/app/core/auth/auth.interceptor.ts`):

```ts
const requestOrigin = new URL(request.url, window.location.origin).origin;
const apiOrigin = new URL(baseUrl || window.location.origin, window.location.origin).origin;

if (requestOrigin !== apiOrigin) {
  return next(request);
}
```

Each of these three examples was accepted only after the corresponding test (see
`05-validation-results.md`) demonstrated the fix — not on the strength of the generated code alone.
