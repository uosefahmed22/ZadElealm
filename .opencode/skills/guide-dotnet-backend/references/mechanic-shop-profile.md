# MechanicShop reference profile

## Repository

`C:\Users\lenovo\Downloads\MechanicShopWorkshop\MechanicShopWorkshop-master`

Treat this repository as read-only unless the user explicitly asks to modify it.

## Confirmed architecture

- ASP.NET Core Web API using Controllers.
- Separate Blazor WebAssembly client.
- Domain, Application, Infrastructure, API, Contracts, and test projects.
- CQRS/vertical slices with MediatR.
- EF Core with SQL Server.
- ASP.NET Core Identity and JWT.
- FluentValidation and a custom Result pattern.
- Domain events, SignalR notifications, Hybrid Cache, background service, Serilog, and OpenTelemetry.
- Domain unit, Application unit, Application slice/subcutaneous, and API integration tests.

## Dependency direction

```text
Domain <- Application <- Infrastructure <- API
Client -> Contracts <- API
```

## Representative flow

Use `CreateWorkOrder` when a complete mutation example is needed:

```text
CreateWorkOrderRequest
→ WorkOrdersController.Create
→ CreateWorkOrderCommand
→ MediatR pipeline
→ CreateWorkOrderCommandValidator
→ CreateWorkOrderCommandHandler
→ IWorkOrderPolicy and EF queries
→ WorkOrder.Create
→ AppDbContext.SaveChangesAsync
→ audit interceptor/domain event
→ SignalR notifier
→ WorkOrderMapper
→ Result.Match
→ 201 or ProblemDetails
```

Always reopen the actual files before citing details; this profile is navigation, not a substitute for code evidence.

## Transferable strengths

- Thin controllers.
- Feature-based Application organization.
- Private entity mutation and explicit domain operations.
- Application-owned ports implemented by Infrastructure.
- Expected Result errors separated from unexpected exceptions.
- Separate EF configurations.
- Multiple test levels and test data factories.
- `TimeProvider` in testable infrastructure paths.

## Known cautions to verify

- Minimal API `Endpoints` files appear present while runtime maps Controllers; search for mappings before treating them as active.
- `RequestLogContextMiddleware` exists; verify it is registered.
- `LoggingBehaviour` exists; verify preprocessor registration for the installed MediatR version.
- Domain events are dispatched before `base.SaveChangesAsync`; consumers may observe a notification before persistence succeeds.
- `CreateWorkOrderCommandHandler` is long and duplicates some policy-style checks.
- `WorkOrderPolicy.IsVehicleAlreadyScheduled` contains an apparently unused materializing query before `AnyAsync`.
- An authorization attribute is duplicated on the labor assignment action.
- Some validation reads system time directly while other code uses `TimeProvider`.
- Application references some framework/infrastructure-oriented packages, so the Clean Architecture boundary is pragmatic rather than pure.

Reopen and confirm exact code before turning these cautions into current findings.
