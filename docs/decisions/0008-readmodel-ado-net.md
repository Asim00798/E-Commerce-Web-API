# ADR-0008: ADO.NET for Specialized Read-Model Queries

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Data Access / Query Architecture

## Context

The E-Commerce modular monolith uses Entity Framework Core (EF Core) as its primary ORM and persistence technology. EF Core supports domain persistence, transactional writes, repository implementations, and ordinary application queries.

However, some read workloads have requirements that are better served by explicitly written SQL and direct data access. Examples include complex joins across multiple tables, reporting queries, aggregation-heavy operations, and projections that require precise control over the SQL query shape.

The solution contains a dedicated `E-Commerce.ReadModel` project for specialized read-side workloads. This project is not intended to replace EF Core or require every query to use a separate read model.

ADO.NET provides direct access to SQL Server through explicit SQL commands and data readers, allowing these queries to avoid ORM materialization and change-tracking overhead where those costs are relevant.

The architectural question is how to use direct SQL selectively without duplicating the entire persistence architecture, weakening module boundaries, or introducing an unnecessary separate database.

## Decision

**Use ADO.NET within `E-Commerce.ReadModel` for specialized read queries when explicit SQL and direct data access provide a meaningful benefit. Retain EF Core as the default data access technology for ordinary application queries and domain persistence.**

The decision establishes a selective approach to query optimization rather than a universal preference for either EF Core or ADO.NET.

The `E-Commerce.ReadModel` project is responsible for specialized read-side implementations whose query shape, aggregation requirements, performance characteristics, or reporting needs justify direct SQL.

### 1. Selective Read-Model Usage

A query may use the ordinary Application and EF Core path when it is straightforward, maintainable, and sufficiently performant.

A dedicated read-model implementation is appropriate when one or more concrete requirements justify it, such as:

* Complex joins across multiple tables or modules.
* Aggregations, summaries, and reporting-oriented projections.
* Queries that return specialized DTOs rather than domain aggregates.
* Performance-sensitive queries for which execution plans, SQL shape, or data materialization require explicit control.
* Read workloads that are difficult to express clearly or efficiently through the existing EF Core query implementation.

The existence of a query alone does not justify creating a specialized read-model implementation.

### 2. Responsibilities of E-Commerce.ReadModel

The `E-Commerce.ReadModel` project provides specialized read-side data access.

Its responsibilities include:

* Executing explicitly designed SQL queries where appropriate.
* Mapping query results into read-only DTOs or projections.
* Supporting complex joins, aggregations, and reporting workloads.
* Keeping specialized query implementation details separate from domain behavior.
* Avoiding unnecessary entity tracking and domain-aggregate materialization for read operations that only require projections.

The project is not the authoritative owner of business state and must not bypass the established write-side rules to modify authoritative data.

A read model may represent data from several business modules for query purposes, but this does not transfer ownership of those modules' business concepts or invariants to the read-model project.

### 3. Responsibilities of EF Core

EF Core remains the default technology for:

* Domain aggregate persistence.
* Repository implementations.
* Unit of Work and transactional write operations.
* Ordinary application queries.
* LINQ queries that are sufficiently clear and efficient.
* Queries that benefit from entity materialization or relationships already modeled in the persistence layer.

ADO.NET is not introduced to replace EF Core's change tracking, repository implementations, or Unit of Work responsibilities.

The two technologies serve different purposes within the same application architecture.

### 4. Query Selection Criteria

The choice between EF Core and ADO.NET should be based on the query's requirements rather than a blanket technology preference.

| Criterion                     | EF Core                           | ADO.NET in ReadModel                                       |
| ----------------------------- | --------------------------------- | ---------------------------------------------------------- |
| Ordinary application queries  | Default choice                    | Usually unnecessary                                        |
| Domain aggregate persistence  | Primary choice                    | Not the default                                            |
| Unit of Work write operations | Primary choice                    | Outside the intended read-model responsibility             |
| Straightforward LINQ queries  | Preferred when suitable           | Usually unnecessary                                        |
| Complex SQL joins             | Suitable when clear and efficient | Appropriate when explicit SQL provides an advantage        |
| Reporting and aggregation     | Suitable for simpler workloads    | Appropriate for complex or specialized workloads           |
| Explicit SQL control          | Available when required           | A primary reason for selection                             |
| Entity tracking               | Available when needed             | Direct mapping can avoid ORM tracking                      |
| Specialized DTO projections   | Supported                         | Appropriate when direct SQL improves clarity or efficiency |

This table describes architectural defaults, not absolute technical limitations. EF Core can execute raw SQL, and ADO.NET can implement simple queries. The choice depends on the actual requirements and maintainability of the implementation.

### 5. Database and Transaction Boundaries

The read-model implementation may query the existing SQL Server database. A dedicated read-model project does not imply a separate physical database.

When a read query uses the same database as the write model, it can observe committed database state according to the database's transaction isolation and consistency behavior.

If a query uses a separate database, asynchronous projection, or independently updated read store in the future, its consistency characteristics must be documented explicitly.

ADO.NET queries must use parameterized SQL rather than concatenating untrusted input into SQL statements. Connection management, cancellation, command timeouts, error handling, and resource disposal must follow the application's established infrastructure conventions.

If a specialized read query must participate in a particular transaction, its connection and transaction must be configured accordingly. Direct data access must not silently undermine a required transaction boundary.

### 6. Architectural Boundaries

The use of ADO.NET does not change the solution's Clean Architecture dependency direction.

The Domain layer remains independent of SQL Server, EF Core, and ADO.NET implementation details.

Application use cases may depend on appropriately defined query contracts where needed, while the concrete read-model implementation remains in the outer data-access portion of the architecture.

The exact project-reference arrangement must preserve the established dependency rules. The Domain layer must not reference `E-Commerce.ReadModel`, and domain behavior must not depend on direct SQL implementation details.

The read-model project must not become a general-purpose location for unrelated business logic. Query shaping and result mapping belong there; business invariants and state transitions remain with the modules that own them.

## Consequences

### Positive Consequences

* Specialized queries gain explicit control over SQL shape and result mapping.
* Read operations can avoid unnecessary entity tracking and aggregate materialization.
* Complex reporting and aggregation queries can remain focused on their actual data requirements.
* Ordinary application queries retain the productivity and maintainability of EF Core.
* Domain persistence and transactional writes remain within the established EF Core architecture.
* The system can optimize individual read workloads without introducing a mandatory CQRS implementation for every query.
* A dedicated project makes specialized read-side responsibilities easier to identify and maintain.

### Trade-Offs

* ADO.NET requires explicit SQL authoring and result mapping.
* Query implementations must manage connections, commands, readers, cancellation, and resource disposal correctly.
* SQL Server-specific syntax may reduce portability.
* Raw SQL requires deliberate parameterization and careful handling of query composition.
* Specialized SQL queries may require more explicit maintenance when database schemas change.
* Maintaining two data-access approaches introduces some additional implementation complexity.

These trade-offs are acceptable when a specialized query has a concrete requirement that justifies direct data access.

## Alternatives Considered

### 1. Use EF Core for Every Query

All reads would use EF Core and LINQ, including complex reporting and aggregation workloads.

**Not adopted as a universal rule:** EF Core remains the default, but forcing every query through the same abstraction can make some specialized SQL workloads less explicit or harder to optimize.

EF Core remains a valid option for complex queries when its generated SQL and materialization behavior meet the requirements.

### 2. Use ADO.NET for All Data Access

All reads and writes would use direct SQL and explicit data mapping.

**Rejected:** This would discard useful EF Core capabilities for aggregate persistence, change tracking, relationships, and Unit of Work integration without a sufficient architectural benefit.

### 3. Use a Separate Read Database

The application would maintain a separate read store, potentially populated through asynchronous projections.

**Not required by this decision:** A dedicated project and specialized SQL do not require separate physical storage. A separate read database would introduce additional synchronization, consistency, operational, and recovery considerations that need independent justification.

### 4. Use EF Core Raw SQL for Every Specialized Query

Specialized SQL queries could be implemented through EF Core's raw SQL facilities.

**Retained as a valid alternative:** EF Core raw SQL may be appropriate when its materialization and query integration are useful. ADO.NET is selected where direct command execution and explicit result mapping provide a clearer or more suitable implementation.

The decision does not prohibit raw SQL through EF Core.

## Reconsideration Criteria

Revisit this decision if:

* Specialized read queries become difficult to maintain or duplicate substantial amounts of SQL and mapping logic.
* Query profiling shows that the existing implementation no longer meets performance requirements.
* The application introduces a separate read database or asynchronous projection pipeline.
* A different database technology or access strategy provides a demonstrable advantage.
* A common read-model abstraction begins adding more complexity than it removes.

Any change should be supported by concrete query requirements, maintainability concerns, or measured performance evidence rather than by a general preference for one data-access technology.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md)
* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0003: Selective CQRS](0003-selective-cqrs.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)

## Architectural Principle

**EF Core is the default for application persistence; ADO.NET is a deliberate option for specialized read workloads.**

The `E-Commerce.ReadModel` project exists to solve specific query requirements, not to impose a separate read implementation on every use case or introduce a separate database without justification.
