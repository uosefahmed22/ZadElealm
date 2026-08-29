---
name: guide-dotnet-backend
description: Mentor and implementation guide for ASP.NET Core Web API and general .NET backend projects. Use when Codex needs to explain or trace a .NET solution, layer, endpoint, request flow, dependency direction, CQRS/MediatR feature, EF Core persistence, authentication, authorization, testing strategy, or architectural decision; teach from an expert reference repository; pair-program or implement a feature; or review Domain, Application, Infrastructure, API, Contracts, and test boundaries. Trigger for .sln/.csproj backends and requests such as explain this layer, trace this endpoint, build it like the reference, guide me step by step, compare implementations, or review architecture.
---

# Guide .NET Backend

Use an existing expert codebase as evidence, extract transferable engineering decisions, and help the user understand or apply them without copying patterns blindly.

## Select the operating mode

Infer the least expansive mode authorized by the request:

- **Learn**: Inspect and explain. Do not edit. End with a short understanding check.
- **Trace**: Follow one real request or behavior through actual files. Do not edit.
- **Pair**: Present one bounded implementation step, let the user attempt it, then review before continuing.
- **Build**: Design briefly, implement the requested feature, verify it, and explain non-obvious decisions.
- **Review**: Inspect and report evidence-backed findings. Do not implement fixes unless asked.

Honor an explicitly requested mode. A request to explain or review never authorizes code changes.

## Establish context

1. Identify the target repository and any reference repository.
2. Read applicable `AGENTS.md` files before project work.
3. Locate `.sln`, `.slnx`, `.csproj`, `Directory.Build.*`, `Directory.Packages.props`, `global.json`, entry points, and tests.
4. Treat the configured reference repository as read-only unless the user explicitly asks to change it.
5. Prefer `rg --files` and `rg -n` for discovery and usage checks.

Default personal reference when it exists:

`C:\Users\lenovo\Downloads\MechanicShopWorkshop\MechanicShopWorkshop-master`

If it is missing or a different reference is supplied, continue with the available repository and state the source of evidence.

## Read only the needed references

- Read [references/layers.md](references/layers.md) for boundaries, dependency direction, project structure, or simple versus advanced architecture.
- Read [references/feature-workflow.md](references/feature-workflow.md) for tracing, designing, implementing, or testing a feature.
- Read [references/mechanic-shop-profile.md](references/mechanic-shop-profile.md) only when using the default MechanicShop reference or comparing against it.

## Inspect before explaining

Never infer architecture from folder names alone.

1. Read project references to establish compile-time dependency direction.
2. Read the composition root and dependency registration.
3. Confirm middleware and pipeline behaviors are registered, not merely present.
4. Choose a real endpoint and follow every call to persistence and side effects.
5. Find representative tests for the same path.
6. Label each conclusion as **Evidence**, **Inference**, or **Recommendation**.

## Explain a layer

Return the explanation in this order:

1. Responsibility in one sentence.
2. Allowed and forbidden dependencies.
3. Main entry points and files.
4. One real flow through the layer.
5. Why the design likely exists.
6. Trade-offs and observable problems.
7. What transfers to other domains.
8. Simplest viable version.
9. Signals that justify the advanced version.
10. Tests that prove the behavior.

In Learn mode, finish with two or three questions that require the user to explain the idea, not memorize terminology.

## Trace a feature

Follow this sequence and cite actual paths and symbols:

```text
HTTP contract
→ middleware/authentication/authorization
→ controller or endpoint
→ mapping to use case
→ validation/pipeline
→ handler or application service
→ domain behavior
→ database/external services
→ transaction and side effects
→ mapping and HTTP response
→ tests
```

Call out skipped stages instead of inventing them. Search registrations and usages to identify dead or legacy code.

## Guide implementation

Before writing code:

1. Define actor, input, success output, expected errors, invariants, persistence changes, side effects, authorization, and concurrency risk.
2. Choose the smallest architecture that fits the present problem.
3. List proposed files with one responsibility each.
4. Explain every borrowed reference pattern by the problem it solves.
5. Reject patterns that add no current value.

During implementation:

- Keep HTTP concerns in API/Presentation.
- Keep orchestration in Application.
- Keep framework-independent invariants in Domain.
- Keep EF Core and external implementations in Infrastructure.
- Use Contracts only when an independent client or shared public contract benefits.
- Preserve target conventions unless they conflict with correctness or the user's design.
- Add tests in proportion to business and data-integrity risk.

In Pair mode, give one step at a time and do not silently complete later steps.

## Review decisions, not shapes

For each borrowed pattern, record:

```text
Decision:
Problem solved:
Evidence:
Alternatives:
Trade-offs:
Use when:
Avoid when:
Simplest version:
Tests proving it:
```

Judge Clean Architecture, CQRS, MediatR, repositories, domain events, caching, messaging, and microservices against the target's change pressure and failure modes. Do not recommend them by default.

## Quality gates

Before finishing Build work, verify as applicable:

- Dependency direction matches the chosen structure.
- Endpoint remains thin.
- Validation and invariants exist at the correct boundaries.
- Expected errors map consistently to HTTP.
- Authorization is enforced in the backend.
- Cancellation flows to I/O.
- Database constraints, transactions, and concurrency are considered.
- Side effects occur at a reliable point relative to persistence.
- Queries avoid obvious over-fetching and N+1 behavior.
- Tests cover success and meaningful failure paths.
- No secret or sensitive log data is introduced.

## Communicate for learning

Lead with the decision or observed flow. Use plain language, then name the technical pattern. Separate facts from interpretation. When writing code for learning, explain the boundaries and ask the user to restate important decisions.
