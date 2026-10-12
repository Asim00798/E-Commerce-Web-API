\# Database Migrations — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Subsystem:\*\* Startup Migration Subsystem — Single-Instance Application with Transient Failure Recovery

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Introduction



The Migration subsystem applies EF Core migrations to the write-side database during application startup. It runs after the host is built and before requests are served. In the current deployment shape—a single instance—the subsystem runs directly with retry on transient SQL Server failures. No distributed coordination is needed because only one process migrates.



The subsystem is contained entirely within the Infrastructure layer. It owns the \*\*timing and execution\*\* of migrations, not their content. The migrations themselves are standard EF Core migrations generated from `AppDbContext`.



Its responsibility is to bring the schema up to date before the application serves requests and to fail fast when migration cannot complete.



A kill switch (`Enabled = false`) allows migrations to move to a deployment pipeline later without any code change. This is the recommended path when multi-instance deployment is introduced.



\---



\## 2. Scope



\### 2.1. In Scope



\* Application startup migration dispatch.

\* Retry on transient SQL Server failures.

\* Connection-fresh retries to avoid stale pooled connections.

\* Structured logging for startup diagnostics.

\* Configuration validation at startup.

\* A kill switch for deployment-owned migrations.



\### 2.2. Out of Scope



\* Migration generation (`dotnet ef migrations add`).

\* Migration content, including schema changes, seed data, and stored procedure migrations.

\* ReadModel migrations, which are managed by DbUp as a separate subsystem.

\* Hangfire schema preparation, which is managed by Hangfire's `PrepareSchemaIfNecessary`.

\* Rollback or down-migrations. Only forward migration is supported.

\* Distributed coordination, which is reserved for the multi-instance path and documented separately as a future extension.

\* Data migration between environments.



\---



\## 3. Purpose and Responsibility



The subsystem answers the following question:



> Has the schema been brought up to date before requests are served, and if the database was briefly unavailable, was that recovered without operator intervention?



It owns:



\* The decision to migrate or not, based on configuration.

\* Retry on transient failure.

\* Fail-fast startup when migration cannot complete.

\* Fresh-session semantics between retries.



It does not own:



\* The migration list or content.

\* The `\_\_EFMigrationsHistory` table, which is owned by EF Core.

\* The database schema, which is defined by the model and its migrations.

\* Coordination across instances, which is deferred because the current deployment uses a single instance.



\---



\## 4. Architecture Overview



\### 4.1. Startup Flow



```text

Application startup

&#x20;   |

&#x20;   v

Program.cs

&#x20;   |

&#x20;   v

app.ApplyMigrationsAsync()

&#x20;   |

&#x20;   v

MigrationOptions bound from configuration

&#x20;   |

&#x20;   +-- Enabled = false

&#x20;   |       |

&#x20;   |       v

&#x20;   |   Return without migrating

&#x20;   |

&#x20;   +-- Enabled = true

&#x20;           |

&#x20;           v

&#x20;       Apply migrations with retry

&#x20;           |

&#x20;           +-- Success --> Continue startup

&#x20;           |

&#x20;           +-- Unrecoverable failure --> Abort startup

```



`Program.cs` calls one extension method. Its call site is environment-independent; the behavior is driven entirely by configuration.



\### 4.2. Deployment Shape



\*\*Current deployment: Single instance\*\*



```text

Clients

&#x20;  |

&#x20;  v

API instance

&#x20;  |

&#x20;  +----> SQL Server

&#x20;  |

&#x20;  +----> Redis

```



One process serves all traffic. One migrator runs at startup. No distributed coordination is needed under this deployment assumption.



\### 4.3. The Kill Switch



Setting `Enabled = false` turns the startup migration call into a no-op.



```text

Deployment pipeline

&#x20;       |

&#x20;       v

Apply migrations

(dotnet ef database update)

&#x20;       |

&#x20;       v

Roll out application instances

(Database:Migrations:Enabled = false)

&#x20;       |

&#x20;       v

Instances start without touching the schema

```



This is the recommended path when multi-instance deployment arrives. The extension already supports the switch; only the configuration value changes.



\---



\## 5. Components



\### 5.1. MigrationOptions



`MigrationOptions` is bound from the `Database:Migrations` configuration section.



```csharp

public sealed class MigrationOptions

{

&#x20;   public const string SectionName = "Database:Migrations";



&#x20;   public bool Enabled { get; set; } = true;



&#x20;   public int RetryCount { get; set; } = 3;



&#x20;   public int RetryDelaySeconds { get; set; } = 5;

}

```



| Property            | Purpose                                                                                                    |

| ------------------- | ---------------------------------------------------------------------------------------------------------- |

| `Enabled`           | Master switch. `false` disables the startup migration call entirely.                                       |

| `RetryCount`        | Number of retry attempts after the initial attempt when a transient failure occurs. Zero disables retries. |

| `RetryDelaySeconds` | Fixed delay between retry attempts.                                                                        |



Configuration is validated at startup using `ValidateOnStart`. Invalid values abort host startup before migration runs.



\### 5.2. MigrationExtensions



The public surface exposes one method:



```csharp

public static async Task ApplyMigrationsAsync(this WebApplication app)

```



It is called once from `Program.cs`, after `app.Build()` and before `app.Run()`.



When migrations are disabled, it returns immediately. Otherwise, it runs migrations with retry. A failure that survives retries propagates and aborts startup.



\### 5.3. MigrationOptionsExtensions



Registers `MigrationOptions` with validation.



```csharp

public static IServiceCollection AddMigrationOptions(

&#x20;   this IServiceCollection services,

&#x20;   IConfiguration configuration)

```



It is called from `AddInfrastructure` and registers the configuration binding and validators.



\### 5.4. Transient Failure Classification



The retry classifier recognizes specific SQL Server error numbers known to be transient, including connection failures, timeouts, deadlocks, and service-busy conditions.



It walks the inner-exception chain because EF Core may wrap `SqlException` in higher-level exceptions.



The recognized transient errors are:



| SQL Server Error Number | Meaning                |

| ----------------------- | ---------------------- |

| `-2`                    | Timeout                |

| `1205`                  | Deadlock victim        |

| `4060`                  | Cannot open database   |

| `10928`                 | Resource limit reached |

| `10929`                 | Resource limit reached |

| `10053`                 | Connection aborted     |

| `10054`                 | Connection reset       |

| `10060`                 | Network timeout        |

| `40197`                 | Service error          |

| `40501`                 | Service busy           |

| `40613`                 | Database unavailable   |

| `49918`                 | Insufficient resources |

| `49919`                 | Too many operations    |

| `49920`                 | Too many operations    |



Non-transient errors—including schema conflicts, permission failures, invalid SQL in migrations, and model snapshot mismatches—are \*\*not\*\* retried. They propagate immediately, aborting startup rather than consuming unnecessary retry attempts.



\---



\## 6. Configuration



\### 6.1. Base Configuration — `appsettings.json`



```json

{

&#x20; "Database": {

&#x20;   "Migrations": {

&#x20;     "Enabled": true,

&#x20;     "RetryCount": 3,

&#x20;     "RetryDelaySeconds": 5

&#x20;   }

&#x20; }

}

```



\### 6.2. Development Configuration — `appsettings.Development.json`



The base configuration is suitable for single-instance development. No override is required unless the development environment needs different settings.



\### 6.3. Production Configuration — `appsettings.Production.json`



For a single-instance deployment where the application owns migration execution, the base settings can also be used in production.



When migrations are moved to a deployment pipeline, disable startup migration:



```json

{

&#x20; "Database": {

&#x20;   "Migrations": {

&#x20;     "Enabled": false

&#x20;   }

&#x20; }

}

```



\### 6.4. Environment Variable Overrides



```text

Database\_\_Migrations\_\_Enabled=false

Database\_\_Migrations\_\_RetryCount=5

Database\_\_Migrations\_\_RetryDelaySeconds=10

```



The standard .NET configuration hierarchy allows environment variables to override JSON configuration.



Switching migration modes per environment therefore requires no code change.



\---



\## 7. Workflows



\### 7.1. Disabled Mode



```text

app.ApplyMigrationsAsync()

&#x20;       |

&#x20;       v

Log: "EF Core migrations are disabled by configuration."

&#x20;       |

&#x20;       v

Return

```



No database connection is opened by the migration subsystem, and no migration is attempted. Application startup continues.



This mode is used when migrations are applied by the deployment pipeline.



\### 7.2. Direct Mode — Single Instance



```text

Attempt 1..N

&#x20;   |

&#x20;   v

Log: "Applying EF Core migrations (attempt X/Y)"

&#x20;   |

&#x20;   v

dbContext.Database.MigrateAsync()

&#x20;   |

&#x20;   +-- Success

&#x20;   |       |

&#x20;   |       v

&#x20;   |   Log success and return

&#x20;   |

&#x20;   +-- Transient exception

&#x20;   |       |

&#x20;   |       v

&#x20;   |   Close connection

&#x20;   |       |

&#x20;   |       v

&#x20;   |   Delay and retry

&#x20;   |

&#x20;   +-- Non-transient exception

&#x20;           |

&#x20;           v

&#x20;       Propagate exception

&#x20;           |

&#x20;           v

&#x20;       Abort startup

```



\*\*Connection handling:\*\* The connection is explicitly closed between attempts. This is intended to ensure that the next attempt starts with a fresh SQL session rather than reusing a connection in an uncertain state after a network failure.



\*\*Migration check:\*\* EF Core checks `\_\_EFMigrationsHistory` before applying pending migrations. If no migrations are pending—for example, after a deployment pipeline has already applied them or during a subsequent startup—`MigrateAsync` returns without applying additional migrations.



The extension is therefore safe to invoke on every startup, provided the deployment assumptions are respected.



\*\*Retry exhaustion:\*\* If all attempts fail with transient errors, the last exception propagates and startup aborts. The failure is visible in the logs so an operator can investigate.



\### 7.3. Failure Recovery



| Failure                   | Behavior                                                     |

| ------------------------- | ------------------------------------------------------------ |

| Transient SQL error       | Close the connection, delay, and retry with a fresh session. |

| Non-transient SQL error   | Throw and abort startup.                                     |

| Retries exhausted         | Throw and abort startup.                                     |

| Connection close failure  | Log a warning and continue.                                  |

| Invalid migration content | Propagate the non-transient error and abort startup.         |

| No pending migrations     | `MigrateAsync` returns without applying changes.             |



\---



\## 8. Failure Modes and Guarantees



\### 8.1. Guarantees



| Property                                                       | Guarantee                                                                           |

| -------------------------------------------------------------- | ----------------------------------------------------------------------------------- |

| Migration runs before requests are served                      | The migration method is called before `app.Run()`.                                  |

| Migration failure aborts startup                               | Exceptions propagate; the application does not proceed to normal request serving.   |

| Transient failures are retried                                 | Failures are classified using SQL Server error numbers.                             |

| Non-transient failures fail fast                               | Non-transient exceptions are not retried and abort startup.                         |

| Configuration errors are caught at boot                        | `ValidateOnStart` validates configuration during host startup.                      |

| Connection is refreshed between retries                        | `CloseConnectionAsync` is called between attempts.                                  |

| Previously applied migrations are not reapplied                | EF Core uses `\_\_EFMigrationsHistory` to track migration history.                    |

| Kill switch is available                                       | `Enabled = false` disables startup migration.                                       |

| No concurrent migrators under the stated deployment assumption | The current design assumes a single application instance with one startup migrator. |



\### 8.2. Anti-Guarantees



\* \*\*Not a rollback mechanism.\*\* Only forward migrations are supported. A bad migration requires a corrective migration or an appropriate recovery procedure.

\* \*\*Not multi-instance safe.\*\* If two instances start simultaneously with `Enabled = true`, both can call `MigrateAsync` against the same database. EF Core migration history alone does not provide application-level distributed coordination. Deployments that scale horizontally must either set `Enabled = false` and migrate out of band or extend the subsystem with the distributed-lock path.

\* \*\*Not cross-database.\*\* The subsystem migrates the `AppDbContext` schema only. Other `DbContext` instances or separate databases require their own migration procedures.



\---



\## 9. Logging



\### 9.1. Events Logged



| Event                                                                   | Level       | Location                         |

| ----------------------------------------------------------------------- | ----------- | -------------------------------- |

| Migrations disabled                                                     | Information | `ApplyMigrationsAsync`           |

| Migration attempt started                                               | Information | Each attempt                     |

| Migration success                                                       | Information | Each successful attempt          |

| Retry after transient failure                                           | Warning     | Retry handler                    |

| Connection close failure                                                | Warning     | Retry handler                    |

| Migration failure, including non-transient failure or exhausted retries | Error       | Exception propagation to startup |



\### 9.2. Operational Value



\* \*\*Startup hang diagnosis:\*\* The `attempt X/Y` log entry helps identify where the process is during a slow migration.

\* \*\*Retry visibility:\*\* A warning indicates that the database experienced a transient failure. Repeated warnings across deployments may indicate infrastructure instability, such as the database not being ready during restarts, network instability between the application and SQL Server, or connection-pool pressure.

\* \*\*Failure diagnosis:\*\* Non-transient failures surface the underlying exception and abort startup before normal request serving begins.

\* \*\*Healthy-startup signal:\*\* In a healthy startup, the logs show an attempt followed by success, without retry warnings.



\---



\## 10. Testing



\### 10.1. Unit Tests



\*\*MigrationOptions binding and validation\*\*



\* Negative `RetryCount` is rejected.

\* Negative `RetryDelaySeconds` is rejected.



\*\*Transient error classification\*\*



\* Each recognized SQL Server error number returns `true`.

\* Unrecognized error numbers return `false`.

\* A wrapped `SqlException` is detected through the inner-exception chain.



\### 10.2. Integration Tests



\* \*\*Clean database, direct mode:\*\* Migration runs, the schema is created, and startup continues.

\* \*\*Existing schema with no pending migrations:\*\* `MigrateAsync` performs no schema changes, and startup continues.

\* \*\*Transient failure:\*\* Simulate a recoverable connection failure and verify that retry succeeds using a fresh session.

\* \*\*Non-transient failure:\*\* Introduce a broken migration, such as invalid SQL, and verify that startup aborts with the error surfaced.

\* \*\*Retries exhausted:\*\* Force repeated transient failures and verify that startup aborts after the initial attempt plus `RetryCount` retries.

\* \*\*Disabled mode:\*\* Set `Enabled = false` and verify that `ApplyMigrationsAsync` returns without opening a database connection.



\### 10.3. Operational Tests



\* \*\*Cold start:\*\* Start the application while SQL Server is not yet ready. Verify that transient failures are retried and that startup succeeds if the database becomes available within the retry window.

\* \*\*Deployment-owned migration:\*\* Apply migrations through the deployment pipeline using `dotnet ef database update`, start instances with `Enabled = false`, and verify that startup logs show no migration activity.



\---



\## 11. Future Extension — Multi-Instance Support



When the deployment shape changes from a single instance to multiple instances, the subsystem must coordinate migration execution so that concurrent startup processes do not attempt uncoordinated schema changes.



The design for this is documented in the \*\*Migration Subsystem (Multi-Instance)\*\* technical reference. Its outline is:



\* Add `UseDistributedLock`, `LockResource`, and `LockTimeoutSeconds` to `MigrationOptions`.

\* Add a lock-coordinated execution path using SQL Server's `sp\_getapplock`.

\* Use a session-scoped lock so that the lock is released when its owning SQL session ends.

\* Ensure every retry reacquires the lock using a fresh connection, preserving the lock invariant across retry boundaries.



The current design does not foreclose this extension. `ApplyMigrationsAsync` already provides a single call site, so adding a configuration-driven execution branch can remain localized.



The kill switch (`Enabled = false`) remains the preferred mechanism when migrations can be handled reliably out of band. A lock-coordinated path is an alternative for environments where deployment-owned migration execution is not practical.



\---



\## 12. Summary



The Migration subsystem applies EF Core migrations to the write-side database during application startup, with retry on transient SQL Server failures. It is designed for single-instance deployment today, with a kill switch that enables deployment-owned migrations without code changes.



Its properties are:



\* \*\*Fail-fast:\*\* Non-transient failures and exhausted retries abort startup before normal requests are served.

\* \*\*Resilient:\*\* Transient failures are retried, with a connection close between attempts to reduce the risk of reusing a stale session.

\* \*\*Idempotent with respect to applied migration history:\*\* EF Core's `\_\_EFMigrationsHistory` prevents successfully recorded migrations from being reapplied.

\* \*\*Configurable:\*\* Migration mode, retry count, and retry delay are externalized.

\* \*\*Simple:\*\* No distributed coordination is required under the current single-instance deployment assumption.

\* \*\*Extensible:\*\* A multi-instance path can introduce a lock strategy without changing the public startup call site.



The subsystem is deliberately small. It has one responsibility: apply migrations before normal request serving begins. Its failure contract is clear: either migration completes successfully, or the process fails startup with diagnostic information.



\---



\*\*Document Version:\*\* 1.0 — Final

\*\*Feature:\*\* Database Migrations / Startup Migration Subsystem (Single-Instance)



