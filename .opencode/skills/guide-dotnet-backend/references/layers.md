# Layer boundaries and architecture sizing

## Architecture sizing

- **Small:** one Web API project with `Features`, `Domain`, `Data`, and `Common` folders.
- **Medium:** separate `Api`, `Application`, `Domain`, and `Infrastructure` projects. Add `Contracts` only for independent clients.
- **Large:** prefer business modules before microservices. Split deployment only for real ownership, scaling, reliability, or release needs.

## Dependency rules

```text
Domain <- Application <- Infrastructure <- API
```

The API composition root may reference both Application and Infrastructure. Infrastructure implements Application-owned ports.

| Layer | Owns | Avoid |
|---|---|---|
| Domain | Invariants, entities, value objects, state transitions | HTTP, EF queries, configuration, external I/O |
| Application | Use cases, orchestration, ports, DTOs, validation | HTTP status codes, infrastructure implementations |
| Infrastructure | EF Core, identity, external services, cache, jobs | Endpoint behavior, framework-free invariants |
| API | HTTP contracts, routing, auth boundary, response mapping, composition | Business rules and data-access workflows |
| Contracts | Transport data shared with independent clients | Domain behavior and persistence concerns |

## Domain

Place a rule in Domain when it can be evaluated from domain state and must hold through every caller. Protect mutations with meaningful methods and private setters. Use factories when construction itself enforces rules. Add value objects for repeated validated concepts, not every primitive.

Keep aggregate boundaries small. Use domain events only after deciding transaction, retry, and delivery semantics.

## Application

Coordinate use cases:

```text
Load state → check cross-entity policy → call domain behavior → persist → return result
```

Use Application Services for a simpler system. Use CQRS/vertical slices when many use cases, distinct reads/writes, or reusable pipelines justify it. CQRS does not require separate databases.

Define an interface in Application when Application needs an external capability. Do not create an interface for every class.

## Infrastructure

Implement persistence and external capabilities. EF Core `DbContext` already provides repository and unit-of-work behavior; add another abstraction only for a specific benefit.

Use real database integration tests when provider semantics matter. Define timeouts and safe retry rules for external calls. Use an outbox when a persisted change and external message require reliable eventual delivery.

## API

Keep endpoints limited to binding, transport checks, mapping, use-case invocation, and HTTP result mapping. Controllers and Minimal APIs are presentation choices; neither determines the internal architecture.

Treat middleware order as behavior. Verify registrations rather than assuming files are active.

## Pattern selection

| Need | Simple start | Advanced when justified |
|---|---|---|
| Use cases | Application service | CQRS/MediatR |
| Data access | DbContext | Specialized query services/specifications |
| Errors | Small Result type | Central error catalog and mapping |
| Internal reaction | Direct call | Domain event |
| Reliable external reaction | After-save call | Outbox and worker |
| Repeated reads | No cache | Query cache with invalidation policy |
| Periodic task | Hosted service | Durable job scheduler/queue |
| Structure | Layers/folders | Business modules |

## Boundary smells

- Domain references ASP.NET Core, EF implementations, or configuration.
- Controller contains queries, calculations, or entity mutations.
- Handler performs HTTP response construction.
- Infrastructure owns the business workflow.
- Generic repository only mirrors `DbSet`.
- Domain event is published before persistence although consumers assume success.
- A behavior, middleware, or endpoint exists but is never registered.
- Every trivial operation receives multiple files and abstractions without change pressure.
