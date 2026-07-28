# Additional diagrams

Supplementary Mermaid diagrams that go beyond what's already in
[`solution-overview.md`](solution-overview.md) (which has its own system-context, container, and
error-handling diagrams — not repeated here to avoid duplication).

## Authentication sequence

```mermaid
sequenceDiagram
    participant U as User
    participant S as Angular AuthStore
    participant E as POST /api/auth/login
    participant I as IdentityService
    participant T as TokenService
    participant St as sessionStorage

    U->>S: submit login form
    S->>E: POST { email, password }
    E->>I: verify credentials (UserManager)
    alt invalid credentials
        I-->>E: failure (generic — no email-existence leak)
        E-->>S: 401 Problem Details
    else valid credentials
        I-->>E: ApplicationUser
        E->>T: issue JWT (sub, email, given_name, family_name, jti)
        T-->>E: signed token, expiresAtUtc
        E-->>S: 200 { user, accessToken, expiresAtUtc }
        S->>St: TokenStorageService.setSession()
        S->>U: navigate to /tasks
    end
```

## Task request sequence

```mermaid
sequenceDiagram
    participant U as User (Angular UI)
    participant T as TaskService
    participant Int as authInterceptor
    participant E as TaskEndpoints
    participant H as Use-case handler
    participant R as ITaskRepository (EF Core)
    participant D as SQL Server

    U->>T: create/edit/list/change status/delete
    T->>Int: HTTP request
    Int->>Int: same-origin check (URL API) + attach Authorization: Bearer
    Int->>E: request with header
    E->>H: map request -> command/query
    H->>H: ownerId = ICurrentUser.UserId (JWT sub claim)
    H->>R: query/command scoped to ownerId
    R->>D: EF Core query/command
    D-->>R: rows
    R-->>H: domain entities / DTOs
    H-->>E: Result.Success / Result.Failure(UseCaseError)
    E-->>T: 2xx or Problem Details (400/404/409)
    T-->>U: update signals, show notification
```

## AI-assisted engineering workflow

```mermaid
flowchart LR
    A["Architectural decision\n(developer — recorded as an ADR)"] --> B["Scoped implementation task"]
    B --> C["Generated implementation\n(Claude Code)"]
    C --> D["Diff review\n(developer reads before accepting)"]
    D --> E["Build and tests\n(dotnet/npm, architecture tests)"]
    E --> F{"Passes?"}
    F -- "no — real defect found" --> G["Correction\n(documented in docs/ai/04-corrections.md)"]
    G --> E
    F -- yes --> H["Accepted implementation"]
```

This loop — not a single generate-and-ship step — is why `docs/ai/03-review-findings.md` and
`docs/ai/04-corrections.md` contain real entries rather than being empty. See
[`docs/ai/01-development-workflow.md`](../ai/01-development-workflow.md) for the full description of
how architectural analysis and implementation were divided between tools.
