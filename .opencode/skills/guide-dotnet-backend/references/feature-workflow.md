# Feature tracing and implementation workflow

## Discovery

1. Locate the solution, projects, central package/build files, entry point, and tests.
2. Read project references and dependency registration.
3. Search the route or public method.
4. Follow symbols through the real runtime path.
5. Search registrations and usages before declaring code active.
6. Find tests for the same behavior.

## Feature card

```text
Feature:
Actor:
Preconditions:
Input:
Success output:
Expected errors:
Invariants:
Persistence changes:
Side effects:
Authorization:
Concurrency risks:
Observability needs:
```

## Trace path

```text
HTTP contract
→ middleware/auth
→ endpoint
→ application input
→ validation/pipeline
→ handler/service
→ domain operation
→ persistence/external calls
→ transaction/side effects
→ output mapping
→ response
```

For each stage record file, symbol, responsibility, evidence, and any concern.

## Rule placement

| Rule | Typical owner |
|---|---|
| Required field, syntax, range | Contract or validator |
| Invariant from entity state | Domain |
| Needs DB or multiple aggregates | Application policy/service |
| Route access and authentication | API |
| Provider-specific operation | Infrastructure |
| Uniqueness under concurrency | Database constraint plus error handling |

## Implementation order

1. Write scenarios and important acceptance tests.
2. Implement domain behavior and unit tests.
3. Define application input, output, and errors.
4. Add validator only for meaningful input rules.
5. Implement handler/application service.
6. Add persistence configuration and migration.
7. Add API contract and endpoint.
8. Add integration/API tests.
9. Review security, transactions, concurrency, logging, and performance.

Not every feature needs every file. Remove empty abstractions.

## Test matrix

| Risk | Test type |
|---|---|
| Invariant/state transition | Domain unit |
| Orchestration/error path | Application unit or slice |
| EF query/configuration/constraint | Database integration |
| Route/auth/serialization/status | API integration |
| External provider contract | Integration or contract test |
| Complete critical journey | Small number of end-to-end tests |

Cover success, invalid input, missing resource, forbidden access, business conflict, relevant technical failure, and concurrency where meaningful.

## Completion checklist

- Use case and actor are explicit.
- Request and response do not expose entities unnecessarily.
- Endpoint is thin.
- Validation and invariants protect the correct boundaries.
- Expected errors map consistently.
- Authorization is server-side.
- Cancellation reaches I/O.
- Query shape and indexes match access patterns.
- Transaction and side effects have defined semantics.
- Tests cover the main risk paths.
- The feature adds no pattern without a current problem.

## Learning loop

After tracing a reference feature:

1. Close the reference code.
2. Recreate a different small feature from the extracted decisions.
3. Explain each class responsibility.
4. Compare with the reference only after the first attempt.
5. Record differences as improvements, context differences, or misunderstandings.
