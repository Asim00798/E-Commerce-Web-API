\# E-Commerce.ReadModel — Technical Reference Document



\*\*E-Commerce Modular Monolith\*\*

\*\*Version 2.0 – Final\*\*



\---



\## 1. Overview



The `E-Commerce.ReadModel` project is a specialized, read-only, high-performance read subsystem within the modular monolith. It provides optimized query access for workloads whose query shape, aggregation, reporting requirements, or performance characteristics justify a dedicated read-side representation.



The ReadModel is not a mandatory CQRS layer for every query. Straightforward transactional reads remain on the normal write-side persistence model when the normalized transactional model can satisfy the requirement efficiently.



\*\*Core Principle:\*\* The write model is optimized for business behavior and transactional consistency. The ReadModel is optimized for efficient information retrieval.



\### 1.1 Why ReadModel Exists



ReadModel currently supports specialized live-query workloads and provides a foundation for future reporting workloads.



Two categories are considered:



\* \*\*Critical UI reads (e.g., customer and employee profiles).\*\* These are on the user's latency-sensitive path. The dedicated ReadModel access path provides a place to introduce separate connection configuration, replica routing, and other read-specific operational controls later, without changing Application query contracts.

\* \*\*Reporting queries (future).\*\* The primary long-term justification for the project: complex analytics that scan large volumes of data and benefit from explicit SQL, controlled execution characteristics, and a stable database-level contract for external consumers such as BI tools, finance exports, and administrative dashboards.



Both categories can use the same core ReadModel infrastructure — `IReadDbConnectionFactory`, DbUp migrations, ADO.NET query services, and SQL Server — while future reporting workloads may introduce additional operational controls.



The project exists because a subset of read workloads has characteristics that justify a specialized read-side implementation, not because every read should go through it.



\### 1.2 Current ReadModel Model



The current implementation uses \*\*live queries against the transactional SQL Server database\*\*.



It does \*\*not\*\* currently maintain:



\* A separate read database.

\* Persisted projection tables.

\* An event-driven projection pipeline.

\* A read replica.

\* An independent reporting database.



The current model is therefore:



```text

Application Query

&#x20;      ↓

Application-owned Query Contract

&#x20;      ↓

ReadModel Implementation

&#x20;      ↓

ADO.NET

&#x20;      ↓

SQL Server Primary

&#x20;      ↓

Result

```



A future persisted projection or read-replica architecture would be an explicit extension rather than an implicit property of the current ReadModel.



\### 1.3 When ReadModel Is Justified



Introduce ReadModel when at least one of the following is true:



\* \*\*The SQL is complex or SQL-first.\*\* CTEs, window functions, PIVOT, recursive queries, or large-scale correlated operations are significantly clearer, more maintainable, or more controllable as explicit SQL than through the write-side EF Core model.

\* \*\*A database-level contract must remain stable across write-side changes.\*\* External systems, BI tools, or reporting pipelines may consume a stored procedure whose definition is maintained as a versioned database artifact through DbUp migrations.

\* \*\*Reads may eventually require replica routing.\*\* The dedicated connection factory provides a defined access point where future read-database routing can be introduced. A replica is not part of the current implementation.

\* \*\*The read shape deliberately bypasses write-side conventions.\*\* Examples include audit/reporting queries that need soft-deleted records or deliberately denormalized cross-context result shapes.

\* \*\*Latency-sensitive reads require a dedicated access path.\*\* A specialized read connection abstraction provides a place to introduce separate connection configuration or connection-pool isolation if measured workload characteristics eventually justify it.



\*\*ReadModel is not justified by:\*\*



\* \*\*Cross-context data alone.\*\* Cross-context projections can often be expressed using EF Core projections.

\* \*\*"It might get complex later."\*\* Speculative complexity is not a reason to introduce a second data-access technology.

\* \*\*"Stored procedures are faster."\*\* For simple aggregates over small result sets, EF Core with `AsNoTracking()` can be comparable to direct ADO.NET. The database execution plan, indexing, and network round trip generally dominate the overhead of the data-access abstraction.



\---



\## 2. Architectural Philosophy



The system separates write-oriented business modeling from specialized read-oriented data access.



```text

&#x20;                   E-COMMERCE MODULAR MONOLITH

&#x20;                              │

&#x20;                         APPLICATION

&#x20;                              │

&#x20;                ┌─────────────┴─────────────┐

&#x20;                │                           │

&#x20;            WRITE SIDE                  READ SIDE

&#x20;                │                           │

&#x20;         Domain + Application        Application Query

&#x20;                │                           │

&#x20;         Business behavior          Application-owned contract

&#x20;         Transactional state                 │

&#x20;                │                            ▼

&#x20;                │                    ReadModel implementation

&#x20;                │                            │

&#x20;                ▼                            ├── ADO.NET

&#x20;         Infrastructure                      ├── SQL / stored procedures

&#x20;                │                            └── Read-only access

&#x20;                │                            │

&#x20;                ▼                            ▼

&#x20;         SQL Server Primary

&#x20;         (shared database)

```



\*\*Contracts live in Application.\*\* Interfaces such as `ICustomerProfileQueryService` and `IEmployeeProfileQueryService`, together with result models such as `CustomerProfileReadModel` and `EmployeeProfileReadModel`, are Application-owned. ReadModel contains only their implementations and the infrastructure required to execute them.



\*\*ReadModel is read-only and ADO.NET-only\*\* — in deliberate contrast to the EF Core write side.



The ReadModel does not own business behavior or transactional authority.



\---



\## 3. Scope and Responsibilities



\### 3.1 In Scope



The ReadModel project is responsible for:



\* Query service implementations using ADO.NET and `Microsoft.Data.SqlClient`.

\* Stored procedure definitions used by query services.

\* Database migration management for ReadModel-owned stored procedures through DbUp.

\* Read-only database access through `IReadDbConnectionFactory`.

\* Read-side configuration and connection management.

\* Feature-local organization of SQL migrations.



\### 3.2 Out of Scope



The ReadModel project does not:



\* Contain domain business logic.

\* Modify transactional state.

\* Use EF Core, `DbContext`, `DbSet`, or `SaveChanges`.

\* Use Dapper or another ORM abstraction.

\* Define domain aggregates or entities.

\* Own persisted projection tables in the current architecture.

\* Replace the write model for transactional decisions.

\* Define contracts or DTOs consumed by the Application layer — those live in `E-Commerce.Application`.

\* Participate in write-side transactions.

\* Validate domain invariants or authorize business decisions.



\---



\## 4. Key Architectural Rules



1\. \*\*ReadModel is read-only by architectural contract.\*\* Query services execute read operations or approved read-only stored procedures. They do not perform `INSERT`, `UPDATE`, or `DELETE` operations against transactional state.

2\. \*\*ADO.NET is the only data-access technology.\*\* No EF Core, `DbContext`, `DbSet`, Dapper, or other ORM abstraction is used inside ReadModel.

3\. \*\*Stored procedures are managed through DbUp migrations.\*\* The migration system owns the lifecycle and version history of ReadModel-owned database objects.

4\. \*\*Same database, separate access path.\*\* The current implementation uses the same SQL Server primary database as the EF Core write side. ReadModel accesses it through its own connection factory rather than through the write-side `DbContext`.

5\. \*\*Application owns use cases and contracts.\*\* MediatR queries and handlers live in Application. Query service interfaces and result models also live in Application.

6\. \*\*ReadModel references Application.\*\* It implements Application-owned query contracts and returns Application-owned result models. This follows the dependency direction used by other Application abstractions such as `IFileService`, `ICache`, and `IStockService`.

7\. \*\*ReadModel is not a bounded context.\*\* It is a technical read-side subsystem. Its contracts belong to the relevant Application module or feature.

8\. \*\*Query execution never creates or modifies schema.\*\* ReadModel query services do not perform schema changes. DbUp migrations are a separate database-deployment operation that may be executed during deployment or, where configured, during application startup.

9\. \*\*ReadModel does not participate in transactional business decisions.\*\* A ReadModel result must not be treated as authoritative domain state when enforcing an invariant or changing transactional state.

10\. \*\*Current implementation uses live queries.\*\* There is no persisted projection store or asynchronous projection pipeline today.



\---



\## 5. Folder Structure



\### 5.1 Application Layer — Contracts and Query Handlers



```text

E-Commerce.Application/

└── Modules/

&#x20;   └── Profiles/

&#x20;       ├── CustomerProfile/

&#x20;       │   ├── Abstractions/

&#x20;       │   │   └── ICustomerProfileQueryService.cs

&#x20;       │   ├── Models/

&#x20;       │   │   └── CustomerProfileReadModel.cs

&#x20;       │   └── Queries/

&#x20;       │       └── GetCustomerProfile/

&#x20;       │           ├── GetCustomerProfileQuery.cs

&#x20;       │           └── GetCustomerProfileQueryHandler.cs

&#x20;       │

&#x20;       └── EmployeeProfile/

&#x20;           ├── Abstractions/

&#x20;           │   └── IEmployeeProfileQueryService.cs

&#x20;           ├── Models/

&#x20;           │   └── EmployeeProfileReadModel.cs

&#x20;           └── Queries/

&#x20;               └── GetEmployeeProfile/

&#x20;                   ├── GetEmployeeProfileQuery.cs

&#x20;                   └── GetEmployeeProfileQueryHandler.cs

```



\### 5.2 ReadModel Project



```text

E-Commerce.ReadModel/

│

├── Features/

│   ├── CustomerProfile/

│   │   ├── Queries/

│   │   │   └── CustomerProfileQueryService.cs

│   │   └── Sql/

│   │       └── Migrations/

│   │           └── 0001\_CustomerProfile\_CreateGetCustomerProfile.sql

│   │

│   └── EmployeeProfile/

│       ├── Queries/

│       │   └── EmployeeProfileQueryService.cs

│       └── Sql/

│           └── Migrations/

│               └── 0001\_EmployeeProfile\_CreateGetEmployeeProfile.sql

│

├── Infrastructure/

│   ├── Connections/

│   │   ├── IReadDbConnectionFactory.cs

│   │   └── SqlReadDbConnectionFactory.cs

│   ├── Configuration/

│   │   ├── ReadModelOptions.cs

│   │   └── ReadModelOptionsValidator.cs

│   ├── DependencyInjection/

│   │   ├── ReadModelConfigurationExtensions.cs

│   │   └── ReadModelServiceCollectionExtensions.cs

│   └── Migrations/

│       └── DbUpMigrationRunner.cs

│

└── E-Commerce.ReadModel.csproj

```



\*\*Note:\*\* Feature folders under `Features/` contain only implementation code and feature-local SQL migrations. Models and interfaces remain in Application.



\---



\## 6. Runtime Data Access — ADO.NET



\### 6.1 Connection Factory



`IReadDbConnectionFactory` provides open `SqlConnection` instances for ReadModel queries.



```csharp

public interface IReadDbConnectionFactory

{

&#x20;   Task<SqlConnection> CreateOpenConnectionAsync(

&#x20;       CancellationToken cancellationToken = default);

}

```



The current implementation resolves the runtime connection from `ConnectionStrings:DefaultConnection`, which points to the SQL Server primary.



The connection factory provides a defined boundary for future read-database routing. A future read replica may be introduced through configuration and/or connection-factory changes, but replica routing is not a current capability and should not be assumed to be a guaranteed connection-string-only change.



\### 6.2 Query Service Pattern



Each query service:



1\. Opens a read connection through `IReadDbConnectionFactory`.

2\. Creates a `SqlCommand` with `CommandType.StoredProcedure`.

3\. Adds parameters.

4\. Executes the command.

5\. Reads the result using `SqlDataReader`.

6\. Materializes the Application-owned read model POCO.

7\. Returns `null` when the requested row does not exist.



Connections are disposed after query execution.



ReadModel query execution does not enlist in an EF Core write-side transaction.



\### 6.3 Read-Only Enforcement



Read-only behavior is enforced at two levels.



\*\*Application/code-level rule\*\*



ReadModel query services are designed only to execute approved read operations.



\*\*Database permission boundary\*\*



In production, the runtime database identity should receive only the permissions required to execute the approved ReadModel procedures.



For the stored-procedure-only model, this normally means:



```text

ReadModel runtime identity

&#x20;       ↓

EXECUTE on approved read procedures

&#x20;       ↓

Read-only SQL operations

```



The runtime identity should not have schema-modification permissions.



The database permission model is the security boundary; the ADO.NET implementation alone does not make arbitrary SQL intrinsically read-only.



\---



\## 7. Stored Procedures and Migrations



\### 7.1 Procedure Definition



Stored procedures are schema-qualified (`dbo.`) and include `SET NOCOUNT ON`.



Example:



```sql

CREATE OR ALTER PROCEDURE dbo.GetCustomerProfile

&#x20;   @CustomerId UNIQUEIDENTIFIER

AS

BEGIN

&#x20;   SET NOCOUNT ON;



&#x20;   SELECT

&#x20;       p.Id AS CustomerId,

&#x20;       p.FullName,

&#x20;       p.Email,

&#x20;       p.PhoneNumber,

&#x20;       (

&#x20;           SELECT COUNT(\*)

&#x20;           FROM dbo.Orders o

&#x20;           WHERE o.CustomerId = p.Id

&#x20;       ) AS TotalOrders,

&#x20;       (

&#x20;           SELECT ISNULL(SUM(o.TotalAmount), 0)

&#x20;           FROM dbo.Orders o

&#x20;           WHERE o.CustomerId = p.Id

&#x20;       ) AS TotalSpent,

&#x20;       (

&#x20;           SELECT ISNULL(

&#x20;               AVG(CAST(r.StarRatingValue AS FLOAT)), 0.0)

&#x20;           FROM dbo.Ratings r

&#x20;           WHERE r.CustomerId = p.Id

&#x20;       ) AS AverageRating,

&#x20;       (

&#x20;           SELECT COUNT(\*)

&#x20;           FROM dbo.WishlistItems wi

&#x20;           INNER JOIN dbo.Wishlists w

&#x20;               ON wi.WishlistId = w.Id

&#x20;           WHERE w.CustomerId = p.Id

&#x20;       ) AS WishlistItemCount,

&#x20;       (

&#x20;           SELECT MAX(o.PlacedAtUtc)

&#x20;           FROM dbo.Orders o

&#x20;           WHERE o.CustomerId = p.Id

&#x20;       ) AS LastOrderDate

&#x20;   FROM dbo.People p

&#x20;   WHERE p.Id = @CustomerId;

END;

```



The example demonstrates a deliberate read-side SQL shape crossing multiple transactional areas.



Actual production procedures should be reviewed and indexed according to measured workload characteristics.



\### 7.2 Migration Authority



SQL migration files under each feature's `Sql/Migrations/` folder are the authoritative source files for the evolution of ReadModel-owned stored procedures.



Changes to an already-applied procedure create a \*\*new migration file\*\*. Previously applied migration files are never edited.



DbUp's journal records which migration scripts have been applied.



The migration history therefore provides controlled versioning of database-object changes.



\### 7.3 DbUp Runner



`DbUpMigrationRunner` executes embedded migration scripts using the migration connection string supplied by `ReadModelOptions`.



The migration connection represents a distinct \*\*credential/access path\*\* for schema modification. It does not necessarily represent a separate physical database.



For example:



```text

Runtime

&#x20;  ↓

app\_user

&#x20;  ↓

EXECUTE read procedures



Migration

&#x20;  ↓

migration\_user

&#x20;  ↓

Create/alter managed procedures

&#x20;  +

Maintain DbUp journal

```



Both identities may target the same SQL Server database.



The runner is invoked from the composition root rather than from `AddReadModel()`. This deliberately separates:



\* Dependency-injection registration.

\* Database deployment/migration execution.



The runner may be invoked during application startup for development or simple deployments. In a deployment pipeline, migrations can instead be executed as a separate deployment step and startup migration execution disabled.



\### 7.4 Migration Ordering



The current DbUp runner does not explicitly impose a cross-feature dependency graph.



Migration ordering must therefore be made deterministic by the configured DbUp script-discovery/order mechanism and the project's migration naming convention.



The current `0001\_`, `0002\_`, etc. convention should be maintained consistently across features.



If a future migration depends on a database object created by another feature's migration, that dependency must be reflected in the migration ordering strategy. If the current convention cannot guarantee the required ordering, the runner must be extended with explicit deterministic ordering before such a dependency is introduced.



Independent feature migrations should avoid unnecessary cross-feature ordering dependencies.



\---



\## 8. Configuration



\### 8.1 Connection Strings



\*\*Runtime\*\*



Uses:



```text

ConnectionStrings:DefaultConnection

```



This currently points to the same SQL Server primary used by the EF Core write side.



A future read replica or separate read database can be introduced through the ReadModel connection boundary. Such a change may require connection configuration, routing, consistency handling, or operational changes depending on the target architecture.



\*\*Migration\*\*



Uses:



```text

ReadModel:MigrationConnectionString

```



During development, this may be the same credential/connection string as the runtime database.



In production, a separate migration identity is preferred so schema-modification permissions are not granted to the application runtime identity.



\### 8.2 Options



```csharp

public sealed class ReadModelOptions

{

&#x20;   public string MigrationConnectionString { get; set; } = string.Empty;

}

```



`ReadModelOptionsValidator` validates that the migration connection string is not empty.



\### 8.3 Registration



Example configuration:



```yaml

ReadModel:

&#x20; MigrationConnectionString: "Server=...;Database=...;User Id=migration\_user;..."



ConnectionStrings:

&#x20; DefaultConnection: "Server=...;Database=...;User Id=app\_user;..."

```



Both are read from `appsettings.json` and environment-specific configuration.



Secrets are managed externally and are not committed to source control.



\---



\## 9. Dependency Direction



The compile-time dependency direction is:



```text

&#x20;                 E-Commerce.Domain

&#x20;                        ▲

&#x20;                        │

&#x20;               E-Commerce.Application

&#x20;                  ▲              ▲

&#x20;                  │              │

&#x20;                  │              │

&#x20;      E-Commerce.Infrastructure  E-Commerce.ReadModel

&#x20;                  ▲              ▲

&#x20;                  │              │

&#x20;                  └────── E-Commerce.Api

```



\*\*Arrows point toward the referenced dependency.\*\*



Therefore:



\* \*\*Application references Domain.\*\*

\* \*\*ReadModel references Application.\*\*

\* \*\*Infrastructure references Application and Domain.\*\*

\* \*\*API references Application, Infrastructure, and ReadModel.\*\*



The API references ReadModel because the composition root is responsible for registering the ReadModel implementation and invoking its migration/deployment infrastructure.



\*\*Infrastructure does not reference ReadModel.\*\*



This preserves the architectural boundary:



```text

API

&#x20;├── Application

&#x20;├── Infrastructure

&#x20;└── ReadModel



ReadModel → Application



Infrastructure → Application + Domain

```



ReadModel's direct project dependencies are:



\* `E-Commerce.Application`

\* `Microsoft.Data.SqlClient`

\* DbUp



The ReadModel implementation therefore follows the same general dependency direction as other infrastructure implementations: the inner Application layer owns the abstraction and the outer implementation project supplies the implementation.



\---



\## 10. Query Execution Flow



```text

HTTP Request

&#x20;    │

&#x20;    ▼

API Controller

(\[Authorize(Roles = ...)])

&#x20;    │

&#x20;    ▼

Application Query

(MediatR)

(\[AuthorizePermission(...)] optional)

&#x20;    │

&#x20;    ├── LoggingBehavior

&#x20;    ├── PerformanceBehavior

&#x20;    ├── ValidationBehavior

&#x20;    ├── RoleAuthorizationBehavior

&#x20;    ├── PermissionAuthorizationBehavior

&#x20;    ├── TelemetryBehavior

&#x20;    ├── TracingBehavior

&#x20;    └── CachingBehavior

&#x20;        (optional via ICacheableQuery)

&#x20;    │

&#x20;    ▼

Application Query Handler

&#x20;    │

&#x20;    ▼

I<Feature>QueryService

(Application-owned contract)

&#x20;    │

&#x20;    ▼

ReadModel Query Service

(ADO.NET + stored procedure)

&#x20;    │

&#x20;    ▼

IReadDbConnectionFactory

&#x20;    │

&#x20;    ▼

SQL Server Primary

&#x20;    │

&#x20;    ▼

Application-owned Read Model POCO

&#x20;    │

&#x20;    ▼

API Response DTO

(mapped in controller)

&#x20;    │

&#x20;    ▼

HTTP Response

```



The Application layer owns:



\* The use case.

\* The query contract.

\* The result shape.



ReadModel owns:



\* SQL execution.

\* Database command construction.

\* Result materialization.



The ReadModel implementation does not own the business meaning of the query.



\---



\## 11. Consistency and Authoritative Source



The current ReadModel queries execute directly against the live SQL Server primary.



Consequently, once a write transaction has committed, a subsequent ReadModel query against the same primary does not incur replica lag.



This should be understood as \*\*read-after-commit against the same primary\*\*, rather than as a guarantee of instantaneous visibility under every database/transaction scenario.



The write side remains authoritative for business decisions:



```text

Domain + Application

&#x20;       ↓

Business authority

```



ReadModel is an information-retrieval mechanism:



```text

ReadModel

&#x20;   ↓

Information retrieval

```



ReadModel must never be used as the source for:



\* Domain invariant validation.

\* Transactional authorization decisions.

\* State-transition decisions.

\* Business-critical concurrency decisions.



If a future read replica is introduced, replica lag must be treated as an explicit consistency characteristic. Queries whose consumers require primary-consistent data must not be silently routed to a lagging replica.



If persisted projections are introduced later, their asynchronous update process may also introduce eventual consistency.



\---



\## 12. Security



\### 12.1 Runtime Identity



The ReadModel runtime identity should receive only the database permissions required by the read workload.



For the current stored-procedure model:



```text

Application runtime identity

&#x20;       ↓

EXECUTE

&#x20;       ↓

Approved ReadModel procedures

```



It should not have permissions to:



\* Create procedures.

\* Alter procedures.

\* Modify tables.

\* Delete transactional data.

\* Perform schema migrations.



The runtime identity should therefore be distinct from the migration identity in production.



\### 12.2 Migration Identity



The migration identity should have only the schema permissions required by the ReadModel migration set.



This may include:



\* Permissions required to create/alter managed procedures.

\* Permissions required to maintain the DbUp journal.

\* Any additional schema permissions required by future ReadModel migration objects.



The migration identity must never be used by runtime query services.



\### 12.3 Sensitive Data



No sensitive data should be exposed through a ReadModel query unless the data is required for the use case and the caller is appropriately authorized.



The ReadModel does not bypass Application/API authorization simply because the SQL query itself is read-only.



\### 12.4 Cross-Context Coupling



Stored procedures may join tables belonging to multiple bounded contexts, such as People, Ordering, and CustomerEngagement.



This is \*\*schema-level coupling\*\*, not a transfer of domain ownership.



It is a deliberate trade-off for specialized read access.



The write-side domain boundaries remain intact, but schema changes in any participating context may require reviewing the affected ReadModel procedures.



A stable procedure contract therefore does not eliminate internal database coupling; it controls the interface presented by the read side.



\---



\## 13. Testing



\### 13.1 Query Service Tests



Query services should be tested against a real SQL Server environment, such as:



\* LocalDB.

\* A SQL Server container.



The purpose is to verify actual SQL execution and result mapping.



Mocking `SqlDataReader` provides limited value because it can reproduce implementation assumptions without validating the actual database behavior.



Assertions should cover:



\* Correct columns.

\* Correct SQL-to-CLR type mapping.

\* Expected result mapping.

\* Missing-row behavior.

\* Nullable-column behavior.

\* Parameter handling.

\* Stored procedure execution.



\### 13.2 Application Handler Tests



Application query handlers can mock the Application-owned query service interface.



Tests should verify:



\* Successful result mapping.

\* `Result<T>.Success` behavior.

\* Expected not-found behavior when the query service returns `null`.

\* The handler's defined error/exception behavior.



Unexpected infrastructure exceptions should follow the application's established exception-handling policy rather than being universally required to become `Result<T>.Failure`.



\### 13.3 API Tests



API tests should cover:



\* Response mapping.

\* 404/not-found behavior.

\* Self-service resolution through `ICurrentUser`.

\* Endpoint-level authorization behavior.



Authorization middleware should also be verified through API integration tests rather than relying solely on controller unit tests.



\### 13.4 Migration Tests



DbUp migrations should be validated in CI against a fresh SQL Server database.



Tests should verify that:



\* Migration scripts execute successfully.

\* Expected procedures are created.

\* Expected procedure signatures exist.

\* Migrations can be applied in the intended order.

\* The resulting database objects match the expected schema contract.



Migration tests should use the same migration mechanism that is used by the actual deployment path.



\---



\## 14. Final Architectural Rules



1\. \*\*Use the write model for business operations and straightforward reads.\*\*

2\. \*\*Introduce ReadModel only when a workload characteristic justifies it\*\* — such as complex SQL, a stable database-level contract, future replica routing, deliberate convention bypass, or a measured need for specialized read-access characteristics.

3\. \*\*Keep ReadModel read-only by architectural contract and ADO.NET-only.\*\*

4\. \*\*Manage ReadModel-owned stored procedures through DbUp migrations.\*\* Never edit an applied migration.

5\. \*\*Use one SQL Server primary for write and read initially.\*\* The current ReadModel connection factory provides the boundary for future read-database routing, but a replica is not part of the current architecture.

6\. \*\*Application owns contracts, use cases, and result shapes.\*\* ReadModel implements the Application-owned query service interfaces and returns the Application-owned models.

7\. \*\*ReadModel is not a bounded context.\*\* It is a technical read-side subsystem.

8\. \*\*Do not add unnecessary abstractions.\*\* No `IQueryable`, no ORM wrapper, and no common helper abstraction unless multiple features demonstrate a genuine shared need.

9\. \*\*Do not allow ReadModel queries to participate in write-side transactions or enforce domain invariants.\*\*

10\. \*\*Keep the current implementation live-query based.\*\* Do not introduce persisted projections, replica routing, or a separate reporting database without a concrete workload and consistency requirement.

11\. \*\*Defer paging, index tuning, and database-level optimization until workload requirements or measured evidence justify them.\*\* Application-level operation performance is already measured through `PerformanceBehavior`.

12\. \*\*Keep migration execution separate from query execution.\*\* DbUp is a deployment/database-management mechanism, not part of the runtime query path.

13\. \*\*Separate runtime and migration database credentials in production.\*\* Runtime access should be restricted to the operations required by the application; migration access should carry the additional schema permissions required by DbUp.



\---



\## Conclusion



This Technical Reference Document describes the current ReadModel architecture and its intended evolution boundaries.



It reflects:



\* The dependency direction.

\* Application-owned contract ownership.

\* The current folder structure.

\* ADO.NET-only read access.

\* Live queries against the SQL Server primary.

\* DbUp-managed stored procedures.

\* Separate runtime and migration access paths.

\* Selective CQRS rather than universal CQRS.

\* No persisted read-side projections.

\* No current read replica.

\* No ReadModel participation in business transactions.



It also provides the criteria for deciding whether a new read workload belongs in `E-Commerce.ReadModel` or remains on the normal composite write/read-side persistence model.



\*\*Document version:\*\* 2.0 – Final

\*\*Feature:\*\* ReadModel / Optimized read-only queries



