# AGENTS.md

## Project Overview

Infinite Content AI is an enterprise-oriented content automation platform.

The initial MVP transforms a user-provided topic into:

1. A structured research artifact.
    
2. A structured script artifact.
    

The core MVP flow is:

```text
Create Project
    ↓
Create Pipeline
    ↓
Add Research Step
    ↓
Add Script Step
    ↓
Publish Pipeline
    ↓
Start Pipeline Execution
    ↓
Generate Research Artifact
    ↓
Generate Script Artifact
    ↓
Complete Pipeline Execution
```

The current implementation priority is defined by:

```text
02-Planejamento/Sprints/Sprint 00 - Fundação e Primeiro Vertical Slice.md
```

Before implementing any work, read:

```text
02-Planejamento/Backlog.md
02-Planejamento/MVP.md
04-Backend/Plano de Implementação do MVP.md
```

---

## Source of Truth

The Markdown documentation in this repository is the architectural source of truth.

Before changing code related to a layer, read its corresponding document:

```text
04-Backend/API.md
04-Backend/Application.md
04-Backend/Contracts.md
04-Backend/Data.md
04-Backend/Domain.md
04-Backend/Infrastructure.md
04-Backend/Organização por Features.md
04-Backend/Shared Kernel.md
04-Backend/Visão Geral do Backend.md
04-Backend/Worker.md
```

Also consult the architecture documents under:

```text
03-Arquitetura/
```

When documentation and existing code conflict:

1. Identify the conflict.
    
2. Prefer the documented architectural decision unless it is clearly outdated.
    
3. Do not silently change the architecture.
    
4. Explain the conflict before making a broad structural change.
    

---

## Current Sprint

The active sprint is:

```text
Sprint 00 - Fundação e Primeiro Vertical Slice
```

The current sprint must deliver:

- Solution structure.
    
- Project references.
    
- Global build configuration.
    
- Shared Kernel.
    
- Minimal Organization context.
    
- Development authentication.
    
- Project Domain Model.
    
- Create Project.
    
- Get Project.
    
- List Projects.
    
- PostgreSQL persistence.
    
- EF Core migration.
    
- Problem Details.
    
- Essential automated tests.
    
- Docker Compose with PostgreSQL.
    

Do not implement features outside the active sprint unless they are strictly required to complete its acceptance criteria.

---

## Solution Structure

Production projects must be created under `src/`:

```text
src/
├── InfiniteContentAI.Api
├── InfiniteContentAI.Application
├── InfiniteContentAI.Domain
├── InfiniteContentAI.Data
├── InfiniteContentAI.Infrastructure
├── InfiniteContentAI.Worker
├── InfiniteContentAI.Contracts
└── InfiniteContentAI.SharedKernel
```

Tests must be created under `tests/`.

Suggested test projects:

```text
tests/
├── InfiniteContentAI.ArchitectureTests
├── InfiniteContentAI.SharedKernel.UnitTests
├── InfiniteContentAI.Domain.UnitTests
├── InfiniteContentAI.Application.UnitTests
├── InfiniteContentAI.Data.IntegrationTests
└── InfiniteContentAI.Api.IntegrationTests
```

Create only the test projects required by the current sprint.

---

## Allowed Project References

Use the documented dependency direction.

### SharedKernel

```text
SharedKernel
    → no internal project dependencies
```

### Contracts

```text
Contracts
    → no internal project dependencies
```

### Domain

```text
Domain
    → SharedKernel
```

### Application

```text
Application
    → Domain
    → Contracts
    → SharedKernel
```

### Data

```text
Data
    → Application
    → Domain
    → SharedKernel
```

### Infrastructure

```text
Infrastructure
    → Application
    → Domain
    → Contracts
    → SharedKernel
```

Infrastructure must not reference Data.

### API

```text
Api
    → Application
    → Data
    → Infrastructure
    → Contracts
    → SharedKernel
```

### Worker

```text
Worker
    → Application
    → Data
    → Infrastructure
    → Contracts
    → SharedKernel
```

Do not create circular references.

---

## Architectural Rules

The following rules are mandatory.

1. Domain must remain framework-independent.
    
2. Domain must not reference EF Core.
    
3. Domain must not reference ASP.NET Core.
    
4. Domain must not reference RabbitMQ, Redis, Azure, OpenAI, n8n or logging frameworks.
    
5. Application must not access `DbContext`.
    
6. Application must not reference external provider SDKs.
    
7. API endpoints must not access the database directly.
    
8. Worker consumers must not access `DbContext` directly.
    
9. Data owns EF Core, PostgreSQL mappings, repositories and migrations.
    
10. Infrastructure owns external integrations.
    
11. Infrastructure must not contain `DbContext`, migrations or PostgreSQL repositories.
    
12. Contracts must contain only distributed communication contracts.
    
13. Shared Kernel must remain small and technology-independent.
    
14. Expected failures must use the Result Pattern.
    
15. Exceptions are reserved for unexpected failures or invalid internal usage.
    
16. Every tenant-scoped operation must use `OrganizationId`.
    
17. Cancellation tokens must be propagated through asynchronous operations.
    
18. Do not add abstractions without an immediate use case.
    
19. Do not implement speculative future features.
    
20. Do not place production source code inside documentation folders.
    

---

## Layer Responsibilities

### Domain

Responsible for:

- Entities.
    
- Aggregate Roots.
    
- Value Objects.
    
- Domain rules.
    
- State transitions.
    
- Domain Events.
    
- Domain errors.
    

Domain methods must protect invariants.

Avoid public property setters.

---

### Application

Responsible for:

- Commands.
    
- Queries.
    
- Handlers.
    
- Validators.
    
- Use-case orchestration.
    
- Repository abstractions.
    
- External-service abstractions.
    
- Transaction boundaries through abstractions.
    
- Mapping Domain results to application results.
    

Application must not depend on concrete infrastructure.

---

### Data

Responsible for:

- `ApplicationDbContext`.
    
- Entity Framework Core.
    
- PostgreSQL.
    
- Entity mappings.
    
- Repository implementations.
    
- Query implementations.
    
- Migrations.
    
- Unit of Work implementation.
    
- Outbox and Inbox persistence when introduced.
    

Data must not call external APIs.

---

### Infrastructure

Responsible for:

- AI providers.
    
- RabbitMQ.
    
- Redis.
    
- Azure Blob Storage.
    
- Key Vault.
    
- n8n.
    
- External HTTP clients.
    
- Resilience policies.
    
- External-service health checks.
    

Infrastructure must not contain relational persistence.

---

### API

Responsible for:

- HTTP endpoints.
    
- Request and response contracts.
    
- Authentication.
    
- Authorization.
    
- Problem Details.
    
- OpenAPI.
    
- HTTP-specific validation.
    
- Mapping application results to HTTP responses.
    

Endpoints must remain thin.

---

### Worker

Responsible for:

- Hosting consumers.
    
- Hosting background services.
    
- Dispatching application commands.
    
- Message acknowledgement.
    
- Retry coordination.
    
- Dead-letter handling.
    
- Graceful shutdown.
    
- Recovery jobs.
    

Workers must not contain business rules.

---

### Contracts

Responsible for:

- Message envelopes.
    
- Distributed commands.
    
- Integration events.
    
- Message versioning.
    
- Inter-process communication contracts.
    

Contracts must use interoperable primitive types.

---

### Shared Kernel

Initially contains only:

- `ErrorType`.
    
- `Error`.
    
- `Result`.
    
- `Result<T>`.
    
- `Entity<TId>`.
    
- `AggregateRoot<TId>`.
    
- `IDomainEvent`.
    
- `IClock`.
    
- `PaginatedResult<T>`.
    

Do not turn Shared Kernel into a general utilities project.

---

## MVP Scope

The MVP includes:

- Projects.
    
- Pipelines.
    
- Research Step.
    
- Script Step.
    
- Pipeline Executions.
    
- Step Executions.
    
- Research Artifacts.
    
- Script Artifacts.
    
- Fake AI Provider.
    
- One real AI provider.
    
- PostgreSQL.
    
- EF Core.
    
- RabbitMQ.
    
- Transactional Outbox.
    
- Inbox.
    
- Worker.
    
- Basic retry and recovery.
    

The MVP does not include:

- Redis.
    
- n8n.
    
- Video generation.
    
- Audio generation.
    
- Image generation.
    
- Thumbnail Agent.
    
- SEO Agent.
    
- Publishing Agent.
    
- Analytics Agent.
    
- Translation Agent.
    
- Billing.
    
- Subscriptions.
    
- Public social-media publishing.
    
- Visual workflow editor.
    
- Multi-region deployment.
    
- Advanced approval workflows.
    

Do not implement out-of-scope items unless explicitly requested.

---

## Current Vertical Slice

The first vertical slice is Project management.

Required endpoints:

```text
POST /api/v1/projects
GET  /api/v1/projects/{projectId}
GET  /api/v1/projects
```

The flow must be:

```text
HTTP Endpoint
    ↓
Application Command or Query
    ↓
Domain
    ↓
Repository abstraction
    ↓
Data implementation
    ↓
PostgreSQL
```

The slice must include:

- Domain tests.
    
- Application tests.
    
- Data integration tests.
    
- API integration tests.
    
- Organization isolation.
    
- Problem Details.
    
- Pagination for listing.
    

---

## Organization Isolation

The system is multi-tenant.

All tenant-owned records must include `OrganizationId`.

Rules:

1. Do not trust an OrganizationId received from the request body.
    
2. Resolve the current Organization from a trusted identity context.
    
3. Repository and query methods must require or apply OrganizationId.
    
4. A resource from another Organization must normally return `404`.
    
5. Add cross-tenant tests for every tenant-scoped feature.
    
6. Do not expose whether a resource exists in another tenant.
    

For Sprint 00, fake authentication is allowed only in Development and Test.

---

## Result Pattern

Expected failures must use:

```text
Result
Result<T>
Error
ErrorType
```

Error codes must be stable.

Examples:

```text
Project.NameRequired
Project.NameTooLong
Project.NotFound
Identity.OrganizationRequired
```

Error descriptions may change.

Do not use descriptions as programmatic identifiers.

Do not return HTTP status codes from Domain or Shared Kernel.

---

## Coding Conventions

Use:

- Modern C#.
    
- Nullable reference types.
    
- Async APIs where appropriate.
    
- `CancellationToken`.
    
- Immutable records for contracts and read models.
    
- Strongly typed IDs in Domain.
    
- UUID v7 for newly generated identifiers.
    
- `DateTimeOffset` for timestamps.
    
- UTC for stored instants.
    
- Explicit access modifiers.
    
- File-scoped namespaces when consistent with the project.
    
- Small classes with a single responsibility.
    
- Constructor injection.
    
- Feature-based organization.
    

Avoid:

- Generic repository abstractions.
    
- Public setters on Domain entities.
    
- Static service locators.
    
- Hidden global state.
    
- Lazy loading.
    
- `.Result` and `.Wait()`.
    
- `async void`.
    
- Catching `Exception` without rethrowing or explicit handling.
    
- Logging secrets or sensitive payloads.
    
- Creating interfaces for every class without a boundary or testing reason.
    
- Generic `Helper`, `Manager` or `Utils` classes.
    

---

## Persistence Conventions

Use PostgreSQL and Entity Framework Core.

Conventions:

- `snake_case` database names.
    
- UUID columns for identifiers.
    
- `timestamp with time zone` for instants.
    
- Explicit `IEntityTypeConfiguration<T>` mappings.
    
- No EF Core attributes in Domain.
    
- `AsNoTracking` for read-only queries.
    
- Projection to read models.
    
- No generic repository.
    
- Deterministic ordering for pagination.
    
- Organization filtering in every tenant-scoped query.
    

Do not enable lazy loading.

Do not run production migrations automatically from every API or Worker instance.

---

## Testing Rules

Use the smallest appropriate test level.

### Unit tests

Use for:

- Shared Kernel.
    
- Domain rules.
    
- Application handlers with fakes.
    
- Error classification.
    
- Value Objects.
    

### Integration tests

Use for:

- EF Core mappings.
    
- PostgreSQL constraints.
    
- Repositories.
    
- Queries.
    
- API endpoints.
    
- Authentication and Organization isolation.
    

Use a real PostgreSQL instance or container for persistence tests.

Do not use EF Core InMemory to validate relational behavior.

Every bug fix should include a regression test when practical.

---

## Change Discipline

Before modifying files:

1. Read the active Sprint.
    
2. Identify the related backlog item.
    
3. Read the relevant architecture document.
    
4. Inspect existing code.
    
5. Make the smallest coherent change.
    
6. Run relevant tests.
    
7. Run the full build before finishing.
    

Do not perform broad refactors unrelated to the selected backlog item.

Do not rename architectural concepts without explaining the impact.

Do not modify Obsidian configuration unless explicitly requested.

Preserve:

```text
.obsidian/
```

---

## Package Discipline

Before adding a NuGet package:

1. Confirm that the current task requires it.
    
2. Prefer standard .NET capabilities when sufficient.
    
3. Avoid packages for trivial utilities.
    
4. Use compatible package versions across projects.
    
5. Do not add future infrastructure packages early.
    

For the first solution-creation task, do not add:

- EF Core.
    
- Npgsql.
    
- RabbitMQ.
    
- Redis.
    
- OpenAI SDK.
    
- Azure SDK.
    
- Polly or resilience packages.
    

Those packages must be added only when their implementation phase begins.

---

## Commands to Run

Before completing an implementation task, run the relevant commands.

At minimum:

```bash
dotnet restore
dotnet build
dotnet test
```

When migrations or formatting are involved, also run the appropriate commands.

Do not claim that a command succeeded unless it was actually executed successfully.

If a command fails:

1. Report the failure.
    
2. Diagnose it.
    
3. Fix it when within the task scope.
    
4. Re-run the command.
    
5. Report any remaining blocker honestly.
    

---

## Completion Report

At the end of a task, report:

1. Backlog items implemented.
    
2. Files created.
    
3. Files modified.
    
4. Project references added or changed.
    
5. Packages added.
    
6. Migrations created.
    
7. Commands executed.
    
8. Build and test results.
    
9. Known limitations.
    
10. Recommended next backlog item.
    

Do not continue automatically into the next story unless explicitly requested.

---

## First Codex Task

The first implementation task must be limited to:

```text
US-001 — Criar a solution
US-002 — Configurar padrões globais
```

The first task must:

- Create the solution.
    
- Create the eight production projects.
    
- Create the initial `src` and `tests` structure.
    
- Configure allowed project references.
    
- Create `Directory.Build.props`.
    
- Create `.editorconfig`.
    
- Run restore and build.
    
- Stop before implementing Shared Kernel or Projects.
    

Do not add persistence, messaging or AI packages during this task.