# ADR-0003: Selective CQRS

* **Status:** Accepted
* **Decision Type:** Architecture
* **Date:** 2026-10-13

## Context

The E-Commerce system uses a modular monolith with Domain-Driven Design, application-layer orchestration, and a shared persistence infrastructure.

The system handles both state-changing operations and read operations.

Commands may enforce business invariants, modify aggregates, and produce domain events or integration events. Queries may retrieve individual records, combine information from multiple modules, support reporting, or return projections optimized for API responses.

These workloads do not always have the same requirements.

Some queries are straightforward and can be implemented efficiently through existing Application and EF Core persistence abstractions. Others involve complex joins, aggregations, reporting requirements, or performance-sensitive projections that benefit from specialized SQL and direct ADO.NET access.

Applying a completely separate read model to every query would introduce additional code, mapping, maintenance, and architectural complexity without necessarily providing a corresponding benefit.

The system therefore needs a query strategy that preserves clear command responsibilities while allowing specialized read-side implementations where justified.

## Decision

The system adopts **selective Command Query Responsibility Segregation (CQRS)**.

Commands and queries have distinct responsibilities, but they do not require completely separate persistence models or infrastructure in every case.

The architecture distinguishes between:

* **Commands**, which request state changes and must preserve the relevant business invariants.
* **Queries**, which retrieve information without changing authoritative business state.
* **Dedicated ReadModel queries**, which use specialized read-side implementations when query complexity, performance, or reporting requirements justify them.

A separate read model is an optimization and organizational mechanism, not a mandatory layer for all queries.

### 1. Commands

Commands represent requests to change application state.

Command handlers coordinate the use case and interact with the relevant domain model and persistence abstractions.

Depending on the operation, a command may:

* Load and modify aggregates.
* Enforce application-level authorization.
* Invoke domain behavior that protects business invariants.
* Coordinate persistence through the Unit of Work.
* Record domain events and integration events.
* Persist outbox messages within the appropriate transaction.

Commands must not bypass the authoritative business model by directly modifying database records through a read-side query implementation.

### 2. Ordinary Queries

Queries that do not require a specialized read-side implementation may remain in the existing Application and persistence architecture.

For example, retrieving a single entity or returning a relatively simple projection may be adequately served by an EF Core query.

These queries should use the simplest implementation that satisfies the system's correctness, maintainability, and performance requirements.

A separate ReadModel implementation should not be introduced merely because a query exists.

### 3. Dedicated ReadModel Queries

The `E-Commerce.ReadModel` project provides a specialized read-side subsystem for queries whose requirements justify a separate implementation.

Suitable use cases may include:

* Complex joins across multiple tables.
* Aggregated or denormalized read projections.
* Reporting and analytical queries.
* Large result sets requiring carefully optimized SQL.
* Performance-sensitive queries for which ORM-generated SQL or ordinary persistence abstractions are insufficient.
* Read operations with a query shape substantially different from the domain model.

The ReadModel may use direct SQL and ADO.NET to control query execution, selected columns, joins, aggregation, and result mapping.

The decision to use the ReadModel must be based on identifiable query requirements rather than a blanket preference for direct SQL.

### 4. Read-Side Boundaries

The ReadModel is read-only with respect to authoritative business state.

It must not become an alternative write path for business operations or bypass aggregate invariants.

The ReadModel may have its own query models and result types because read-side representations serve different purposes from domain entities.

These projections do not need to mirror the Domain model or become domain aggregates.

### 5. Data Consistency

The ReadModel's consistency requirements depend on its implementation.

When queries read directly from the same database tables used by the write side, they may observe committed business state without requiring a separate projection-update pipeline.

When a read-side projection is separately persisted or updated asynchronously, its consistency depends on the mechanism used to maintain it. Eventual consistency must not be assumed or claimed without such an asynchronous process.

The architecture must document any consistency limitations that affect application behavior.

### 6. No Mandatory Separate Database

Selective CQRS does not require a separate read database.

The ReadModel may query the application's existing database using a dedicated data-access implementation.

A separate database, replicated data store, or independently maintained projection store should be introduced only when concrete operational, scalability, isolation, or query requirements justify the additional complexity.

## Decision Criteria

The following criteria guide the choice between an ordinary Application query and a dedicated ReadModel implementation.

| Criterion                | Ordinary Application Query                                               | Dedicated ReadModel                                                                         |
| ------------------------ | ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------- |
| Query complexity         | Simple retrieval or projection                                           | Complex joins, aggregation, or specialized query shape                                      |
| Performance requirements | Satisfied by the existing persistence implementation                     | Requires specialized SQL or measured optimization                                           |
| Data shape               | Closely aligned with the application's existing data access              | Specialized projection or reporting result                                                  |
| Maintenance cost         | Simpler to maintain in the existing query implementation                 | Additional separation is justified by concrete benefits                                     |
| Data consistency         | Reads the authoritative persisted state through existing access patterns | May read the same tables or a separately maintained projection, depending on implementation |
| Write behavior           | Does not modify state when implemented as a query                        | Read-only; must not perform business-state mutations                                        |

These criteria are guidelines rather than rigid thresholds. Query complexity alone does not automatically justify a separate implementation if the existing approach remains clear and performant.

## Rationale

Selective CQRS provides a balance between architectural clarity and implementation simplicity.

The decision offers several benefits:

* **Avoids unnecessary abstraction:** Simple queries do not require additional read-side infrastructure.
* **Preserves write-side integrity:** Commands remain responsible for state changes and business invariants.
* **Enables query optimization:** Complex or performance-sensitive queries can use direct SQL and specialized result mapping.
* **Supports different data shapes:** Read projections can be designed around consumer requirements without forcing the Domain model to match every query.
* **Limits maintenance overhead:** The team introduces a dedicated ReadModel only when its benefits justify the additional implementation.
* **Fits the modular monolith:** Specialized read access does not require splitting the application into independent services or databases.

The decision treats CQRS as a design technique applied where useful, not as a requirement to duplicate the entire application's data model.

## Consequences

### Positive Consequences

* The application can optimize demanding queries independently of domain-model design.
* Straightforward queries remain easy to implement and maintain.
* Write-side business rules remain separate from read-side data retrieval.
* Direct SQL and ADO.NET are available where explicit control over query execution is beneficial.
* The architecture avoids introducing a separate database or projection pipeline without a demonstrated need.

### Trade-offs and Constraints

* Developers must determine which queries justify a dedicated implementation.
* Supporting both ordinary queries and a dedicated ReadModel introduces more than one read-side implementation approach.
* Direct SQL requires explicit attention to parameterization, result mapping, null handling, query maintenance, and database schema changes.
* Specialized queries can become tightly coupled to database schema if not maintained carefully.
* Read-side performance still depends on indexing, execution plans, connection management, database capacity, and measurement.
* If separately maintained projections are introduced, their consistency and failure-recovery behavior must be designed explicitly.

## Alternatives Considered

### 1. No CQRS Separation

**Description:** Use the same application services and persistence patterns for both reads and writes without explicitly separating command and query responsibilities.

**Reason for Rejection:** Although simple, this approach does not establish a clear distinction between state-changing operations and read-only operations. The project benefits from explicit command and query responsibilities, even when both use the same database.

### 2. Full CQRS With Separate Read Models for Every Query

**Description:** Require all queries to use dedicated read-side models and implementations separate from the write-side persistence model.

**Reason for Rejection:** Many queries do not justify the additional mapping, abstractions, and maintenance overhead. Requiring a separate read model universally would add complexity without ensuring better performance or maintainability.

### 3. Separate Read Database

**Description:** Maintain a dedicated database for read workloads.

**Reason for Rejection:** The current architecture does not establish a requirement for independent read-database scaling, workload isolation, or asynchronous data replication. A separate database would introduce additional synchronization and operational responsibilities.

This option may be reconsidered if future workloads justify it.

## Conditions for Reconsideration

This decision should be revisited if the system develops requirements such as:

* Sustained read workloads that interfere with write-side performance.
* A demonstrated need to scale read and write workloads independently.
* Reporting workloads that require stronger isolation from transactional operations.
* Specialized read models that are more efficiently maintained separately.
* A need for independently managed projections with explicit consistency guarantees.

Any expansion of CQRS must include a clear rationale for the added data ownership, synchronization, operational, and recovery responsibilities.

## Related Decisions

* [ADR-0001: Modular Monolith Architecture](0001-modular-monolith.md)
* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0008: ReadModel ADO.NET](0008-readmodel-ado-net.md)

## Summary

The E-Commerce system separates command and query responsibilities while introducing dedicated read-side implementations only where query requirements justify them.

**The governing principle is to keep commands responsible for valid state changes, keep queries read-only, and introduce specialized read models when they provide a concrete architectural or performance benefit.**
