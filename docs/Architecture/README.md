# Architecture Documentation

This directory documents the architecture of the E-Commerce modular monolith, including its application layers, dependency boundaries, bounded contexts, and specialized read-side subsystem.

The documentation describes the intended architectural structure and the responsibilities of each layer. It complements the root [`architecture.md`](../../architecture.md), which provides the high-level architectural overview.

## Documents

| Document                                        | Description                                                                       |
| ----------------------------------------------- | --------------------------------------------------------------------------------- |
| [API Layer](api-layer.md)                       | Responsibilities and architectural boundaries of the API layer.                   |
| [Application Layer](application-layer.md)       | Application-layer responsibilities, orchestration, and dependency boundaries.     |
| [Domain Layer](domain-layer.md)                 | Domain-layer responsibilities, domain modeling, and core business rules.          |
| [Infrastructure Layer](infrastructure-layer.md) | Infrastructure responsibilities and implementation of technical abstractions.     |
| [Context Map](context-map.md)                   | Relationships and integration boundaries between bounded contexts.                |
| [ReadModel Layer](readmodel-layer.md)           | Purpose, responsibilities, and boundaries of the specialized read-side subsystem. |

## Architectural Boundaries

The solution separates business logic, application orchestration, external interfaces, and technical implementation.

The primary dependency direction is:

**API → Application → Domain**

Infrastructure implements abstractions defined by the inner layers. The Domain layer remains independent of infrastructure frameworks and implementation details.

The ReadModel is a specialized subsystem for read workloads that benefit from dedicated query models or data-access strategies. It complements the write-side architecture rather than requiring every query to use a separate read model.

## Scope

This directory focuses on architectural structure and responsibilities.

For related documentation, see:

* [Business Features](../business-features/) — business requirements, workflows, references, and integration guides.
* [Technical Capabilities](../technical-capabilities/) — cross-cutting technical capabilities and their implementation guidance.
* [Architecture Decisions](../decisions/) — recorded architectural decisions and their rationale.
* [Root Architecture Overview](../../architecture.md) — the high-level architectural overview.

## Maintenance

Keep these documents aligned with the actual solution structure and established architectural decisions.

When an architectural boundary or responsibility changes, update the affected documentation and, where appropriate, record the rationale in the Architecture Decision Records.
