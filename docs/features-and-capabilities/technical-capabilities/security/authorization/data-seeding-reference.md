\# Data Seeding — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Platform:\*\* .NET 8 / ASP.NET Core / EF Core / SQL Server

\*\*Scope:\*\* Startup Reconciliation of Authorization State

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Introduction



The Data Seeding subsystem reconciles persisted authorization state with declarations maintained in the codebase.



It is designed to execute once per application startup, after database migrations and before the application becomes ready to serve HTTP requests that depend on the seeded state.



The subsystem follows these principles:



\* \*\*Additive-only:\*\* Missing records are inserted; existing records are not automatically deleted.

\* \*\*Idempotent:\*\* Repeated successful runs do not create duplicate records.

\* \*\*Fail-fast:\*\* Invalid declarations and required configuration errors prevent the affected seeding operation from proceeding.

\* \*\*Transactional:\*\* Each seeder owns its transaction boundary.

\* \*\*Explicit execution:\*\* The startup pipeline invokes its four seeders in a defined order.

\* \*\*No HTTP surface:\*\* Seeding is an infrastructure concern, not an API feature.



The subsystem owns four reconciliation responsibilities:



1\. \*\*Permissions:\*\* Ensure every declared permission identity exists in the `Permissions` table.

2\. \*\*Roles:\*\* Ensure every declared system role exists in `AspNetRoles`.

3\. \*\*Role-permission mappings:\*\* Ensure each declared role-permission association exists in `RolePermissions`.

4\. \*\*Bootstrap administrator:\*\* Optionally create an administrator account and assign the Administrator role.



The subsystem is not a bounded context. Its execution and persistence logic reside in Infrastructure, while the permission and role declarations are maintained in Application.



\---



\## 2. Scope



\### 2.1 In Scope



\* Reflection-based discovery of permission declarations.

\* Scrutor-based discovery of permission sources and role declarations.

\* Reconciliation of permissions, roles, and role-permission mappings.

\* Optional bootstrap administrator creation.

\* Validation of declarations and configuration.

\* Idempotent, additive-only reconciliation.

\* Independent transactions for the four seeders.

\* Multi-instance startup behavior and failure handling.



\### 2.2 Out of Scope



\* Deleting or renaming authorization records that are no longer declared in code.

\* Continuous or background reconciliation.

\* Business data such as products, orders, and shipments.

\* HTTP endpoints or controllers.

\* Migration-time reference-data seeding.



When migration-managed reference data is introduced, it can use EF Core's `HasData` where appropriate. That mechanism is separate from runtime startup reconciliation.



\---



\## 3. Purpose and Responsibilities



The subsystem answers the following question:



\*\*Does the database contain the authorization entities and mappings declared by the code before the application is considered ready to serve requests?\*\*



\### 3.1 Responsibilities



The subsystem owns:



\* Startup reconciliation of permission identities.

\* Startup reconciliation of system roles.

\* Startup reconciliation of role-permission mappings.

\* Optional bootstrap administrator creation.

\* Discovery and validation of Application-layer authorization declarations.

\* Transaction management and operational logging for its seeders.



\### 3.2 Non-Responsibilities



The subsystem does not own:



\* Runtime authorization decisions.

\* Permission checks performed by MediatR behaviors or API authorization.

\* The business decision about which permissions a role should receive.

\* Data migrations or destructive cleanup.

\* Continuous synchronization of database state.

\* Reconciliation of an existing bootstrap administrator's role membership.



Runtime authorization reads persisted authorization state through the configured authorization services. It does not use the declaration classes as a substitute for runtime enforcement.



\---



\## 4. Folder Structure



\### 4.1 Application Layer



```text

E-Commerce.Application/

└── Shared/

&#x20;   └── Security/

&#x20;       └── Authorization/

&#x20;           ├── Contracts/

&#x20;           │   ├── IPermissionSource.cs

&#x20;           │   └── IRole.cs

&#x20;           │

&#x20;           ├── Extensions/

&#x20;           │   └── AuthorizationSourcesExtensions.cs

&#x20;           │

&#x20;           ├── Permissions/

&#x20;           │   ├── AccountPermissions.cs

&#x20;           │   ├── AuthorizationPermissions.cs

&#x20;           │   ├── CatalogPermissions.cs

&#x20;           │   ├── CustomerEngagementPermissions.cs

&#x20;           │   ├── FilePermissions.cs

&#x20;           │   ├── FinancePermissions.cs

&#x20;           │   ├── OrderingPermissions.cs

&#x20;           │   ├── PeoplePermissions.cs

&#x20;           │   └── ShippingPermissions.cs

&#x20;           │

&#x20;           └── Roles/

&#x20;               ├── SystemRoles.cs

&#x20;               ├── AdministratorRole.cs

&#x20;               ├── SupportRole.cs

&#x20;               ├── EmployeeRole.cs

&#x20;               ├── CustomerRole.cs

&#x20;               ├── CatalogManagerRole.cs

&#x20;               ├── DispatcherRole.cs

&#x20;               ├── DriverRole.cs

&#x20;               └── PackagingRole.cs

```



\### 4.2 Infrastructure Layer



```text

E-Commerce.Infrastructure/

└── Security/

&#x20;   └── Authorization/

&#x20;       └── DataSeeding/

&#x20;           ├── Configuration/

&#x20;           │   └── DataSeedingOptions.cs

&#x20;           │

&#x20;           ├── Execution/

&#x20;           │   └── StartupSeeder.cs

&#x20;           │

&#x20;           ├── Extensions/

&#x20;           │   └── DataSeedingExtensions.cs

&#x20;           │

&#x20;           └── Seeders/

&#x20;               ├── PermissionSeed.cs

&#x20;               ├── RoleSeed.cs

&#x20;               ├── RolePermissionSeed.cs

&#x20;               └── IdentitySeed.cs

```



\### 4.3 Removed Directories



The redesigned subsystem no longer uses these previous locations:



```text

Application/Shared/Security/Authorization/Seeding/

Infrastructure/Persistence/DataSeeding/

```



\### 4.4 Namespaces



Application namespaces:



```text

E\_Commerce.Application.Shared.Security.Authorization.Contracts

E\_Commerce.Application.Shared.Security.Authorization.Extensions

E\_Commerce.Application.Shared.Security.Authorization.Permissions

E\_Commerce.Application.Shared.Security.Authorization.Roles

```



Infrastructure namespaces:



```text

E\_Commerce.Infrastructure.Security.Authorization.DataSeeding.Configuration

E\_Commerce.Infrastructure.Security.Authorization.DataSeeding.Execution

E\_Commerce.Infrastructure.Security.Authorization.DataSeeding.Extensions

E\_Commerce.Infrastructure.Security.Authorization.DataSeeding.Seeders

```



The namespace declarations must match the actual project namespaces and assembly names.



\---



\## 5. Seeding Model



\### 5.1 Contracts



\#### `IPermissionSource`



\*\*Location:\*\* `Application/Shared/Security/Authorization/Contracts/IPermissionSource.cs`



Marks a class as a source of permission identities.



Implementations declare permissions through public `const string` fields. The default `Describe()` interface implementation discovers the supported constants through reflection.



This allows a permission to be introduced by adding a constant to its declaration class without maintaining a separate registration list.



\#### `IRole`



\*\*Location:\*\* `Application/Shared/Security/Authorization/Contracts/IRole.cs`



Declares a system role and its baseline permission set.



The contract exposes:



\* `string Name { get; }` — the persisted role name.

\* `IEnumerable<string> GetPermissions()` — the permissions assigned by the role declaration.

\* `const string AllPermissions = "\*"` — the wildcard identity, if supported by the implementation.



Each role name must correspond to a constant in `SystemRoles`.



\### 5.2 Permission Declarations



Permission classes are located under:



`Application/Shared/Security/Authorization/Permissions/`



Each class is a sealed implementation of `IPermissionSource`.



Example:



```csharp

public sealed class AccountPermissions : IPermissionSource

{

&#x20;   public const string Read = "Accounts.Read";

&#x20;   public const string Activate = "Accounts.Activate";



&#x20;   // Additional permission constants.

}

```



These classes serve two purposes:



1\. Provide compile-time constants for authorization attributes and application code.

2\. Provide discoverable permission declarations for startup reconciliation.



The inherited `Describe()` implementation should be the single discovery mechanism unless the contract is deliberately redesigned.



The reflection implementation must consistently define which fields qualify as permission declarations. Under this design, they are the implementing type's supported public constant string fields.



\### 5.3 Role Declarations



Role classes are located under:



`Application/Shared/Security/Authorization/Roles/`



Each class implements `IRole` and declares its own baseline permissions.



Example:



```csharp

public sealed class CatalogManagerRole : IRole

{

&#x20;   public string Name => SystemRoles.CatalogManager;



&#x20;   public IEnumerable<string> GetPermissions() =>

&#x20;   \[

&#x20;       CatalogPermissions.ViewProducts,

&#x20;       CatalogPermissions.ManageProducts,

&#x20;       CatalogPermissions.ManageBrands,

&#x20;       CatalogPermissions.ManageCategories

&#x20;   ];

}

```



Role classes reference permission constants directly. They must not depend on another role's permission set or implicitly inherit another role's authorization policy.



\### 5.4 Centralized Role Names



`SystemRoles` is the centralized constant repository for system role names.



It is used by:



\* Role declaration classes.

\* API authorization attributes such as `\[Authorize(Roles = ...)]`.

\* Other application code that needs a system role identifier.



The `IRole` implementations determine which role declarations the seeding pipeline discovers. A constant in `SystemRoles` alone does not cause a role to be created.



\### 5.5 Discovery Through Scrutor



`AddAuthorizationSources()` registers implementations of both contracts.



Illustrative registration:



```csharp

services.Scan(scan => scan

&#x20;   .FromAssemblies(Assembly.GetExecutingAssembly())

&#x20;   .AddClasses(classes => classes.AssignableTo<IPermissionSource>())

&#x20;   .AsImplementedInterfaces()

&#x20;   .WithSingletonLifetime());



services.Scan(scan => scan

&#x20;   .FromAssemblies(Assembly.GetExecutingAssembly())

&#x20;   .AddClasses(classes => classes.AssignableTo<IRole>())

&#x20;   .AsImplementedInterfaces()

&#x20;   .WithSingletonLifetime());

```



The scan must include the assemblies containing the declaration classes. `Assembly.GetExecutingAssembly()` identifies the assembly containing the registration code; it does not automatically mean every assembly in the application.



If declarations are distributed across multiple assemblies, explicitly include all relevant assemblies.



Singleton registration is appropriate because these classes are stateless declarations and should not depend on scoped services.



The seeders consume:



```csharp

IEnumerable<IPermissionSource>

IEnumerable<IRole>

```



\### 5.6 Reconciliation Algorithm



Each seeder follows the same general reconciliation model:



```text

1\. Read the declared state.

2\. Validate declarations relevant to this seeder.

3\. Read existing database state.

4\. Compute the missing records or mappings.

5\. Insert missing state within the seeder's transaction.

6\. Commit and log reconciliation results.

```



The exact implementation differs by seeder.



For permissions and roles, comparison is based on their stable names. For role-permission mappings, comparison uses the role and permission identifiers.



\*\*Orphaned records\*\* are persisted entries no longer represented by current declarations. They are counted and logged but are not automatically deleted.



\---



\## 6. Application Layer



\### 6.1 Declaration Inventory



| Folder         | Contents                                                        |

| -------------- | --------------------------------------------------------------- |

| `Contracts/`   | `IPermissionSource.cs`, `IRole.cs`                              |

| `Extensions/`  | `AuthorizationSourcesExtensions.cs`                             |

| `Permissions/` | Nine permission-source classes implementing `IPermissionSource` |

| `Roles/`       | `SystemRoles.cs` and eight role classes implementing `IRole`    |



The number of declaration classes should be updated when the codebase changes.



\### 6.2 Permission Discovery Requirements



The `Describe()` implementation must:



\* Discover the intended public constant string fields.

\* Avoid treating unrelated fields as permissions.

\* Return a consistent set of permission identities.

\* Support deterministic duplicate detection.

\* Fail clearly if the declaration shape violates the contract.



The seeder should validate the resulting permission identities before opening a transaction for database writes.



\### 6.3 Role Declaration Requirements



A role declaration must:



\* Return a non-empty, stable role name.

\* Reference a `SystemRoles` constant.

\* Return a deterministic permission set.

\* Reference only declared permissions, unless the wildcard is used.

\* Avoid duplicate permissions within its expanded set.



The seeding pipeline validates these conditions before writing the corresponding mappings.



\### 6.4 Wildcard Semantics



A role returning `IRole.AllPermissions` receives every permission currently declared by the discovered permission sources.



Expansion occurs on every successful reconciliation run.



For example:



```text

Current declarations:

&#x20;   Orders.Read

&#x20;   Orders.Cancel

&#x20;   Catalog.Read



Wildcard role:

&#x20;   \*



Expanded set:

&#x20;   Orders.Read

&#x20;   Orders.Cancel

&#x20;   Catalog.Read

```



If `Orders.Return` is declared later, it is included in the next expansion.



The wildcard does not mean that the role automatically receives arbitrary permission records that exist only in the database. Its source of truth is the current declared permission set.



Because wildcard roles receive future permissions automatically, this mechanism must be restricted to roles whose security policy explicitly permits that behavior.



\---



\## 7. Infrastructure Layer



\### 7.1 `StartupSeeder`



\*\*Location:\*\* `Infrastructure/Security/Authorization/DataSeeding/Execution/StartupSeeder.cs`



`StartupSeeder` implements `IHostedService` and orchestrates the four seeders in a fixed order.



It creates one dependency-injection scope and invokes the seeders directly.



```text

StartupSeeder.StartAsync()

&#x20;   │

&#x20;   ├── PermissionSeed

&#x20;   │

&#x20;   ├── RoleSeed

&#x20;   │

&#x20;   ├── RolePermissionSeed

&#x20;   │

&#x20;   └── IdentitySeed

```



The orchestration layer does not need reflection or dynamic seeder discovery. The declaration classes are discovered through Scrutor, while the four infrastructure seeders are invoked explicitly.



All four seeders execute within the same dependency-injection scope and may share the same scoped `AppDbContext`. Each seeder manages its own transaction.



A failure in a seeder propagates to the startup orchestration. The application must not be considered ready if required seeding has failed.



\*\*Important:\*\* Registering an `IHostedService` alone does not guarantee that no HTTP request can arrive before its initialization finishes. The actual host lifecycle and readiness configuration must enforce that requirement.



\### 7.2 Seeder Responsibilities



| Seeder               | Declaration or configuration source           | Comparison key                   | Transaction | Unique-key race behavior                                 |

| -------------------- | --------------------------------------------- | -------------------------------- | ----------- | -------------------------------------------------------- |

| `PermissionSeed`     | `IEnumerable<IPermissionSource>`              | Permission name                  | Own         | Propagates the exception                                 |

| `RoleSeed`           | `IEnumerable<IRole>` using `Name`             | Role name                        | Own         | Propagates the exception                                 |

| `RolePermissionSeed` | `IEnumerable<IRole>` using `GetPermissions()` | `(RoleId, PermissionId)`         | Own         | Handles only a verified, expected duplicate-mapping race |

| `IdentitySeed`       | `DataSeedingOptions` and Identity services    | Bootstrap administrator identity | Own         | Propagates the exception                                 |



\### 7.3 `PermissionSeed`



Responsibilities:



\* Discover permission identities through `IPermissionSource`.

\* Detect duplicate permission names across declaration sources.

\* Read persisted permission names.

\* Insert missing permission records with new `Guid` identifiers.

\* Calculate and log declared, added, and orphaned counts.



A unique-constraint violation during insertion is not specially handled. It propagates and aborts startup.



\### 7.4 `RoleSeed`



Responsibilities:



\* Discover role declarations through `IRole`.

\* Detect duplicate role names.

\* Read existing roles.

\* Create missing roles using `RoleManager<IdentityRole<Guid>>`.

\* Log reconciliation results.



Using `RoleManager.CreateAsync()` allows the configured Identity store to apply its normal role-creation behavior, including normalized role names.



If the Identity store shares the scoped `AppDbContext`, the role creation participates in the active database transaction when the transaction and store are correctly configured.



The seeder does not need to call `SaveChangesAsync()` separately if `RoleManager` and the configured Identity store persist the changes as part of `CreateAsync()`.



A unique-constraint violation during insertion propagates and aborts startup.



\### 7.5 `RolePermissionSeed`



Responsibilities:



\* Validate role names.

\* Validate permission references.

\* Reject duplicate permissions within an expanded role set.

\* Expand wildcard declarations.

\* Read existing role-permission mappings.

\* Insert missing mappings.

\* Log reconciliation results.



The intended validation checks are:



1\. No duplicate role names exist across `IRole` implementations.

2\. Every non-wildcard permission resolves to a declared permission.

3\. No role's expanded permission set contains duplicates.



All validation must complete before the first mapping write.



\#### Multi-instance behavior



This is the only seeder designed to treat a narrowly identified unique-key conflict as a benign concurrent-insert race.



The implementation must:



1\. Identify the expected role-permission uniqueness constraint.

2\. Confirm that the conflict represents a duplicate mapping insertion.

3\. Roll back the failed transaction.

4\. Log the race at Information level.

5\. Continue only when the intended mapping is already present and the desired state is satisfied.



SQL Server error numbers `2601` and `2627` indicate unique-key violations, but the error number alone does not prove that the conflict is harmless.



\*\*Do not swallow every `DbUpdateException` or every unique-key violation indiscriminately.\*\* If the implementation cannot distinguish the expected mapping race from an unrelated integrity error, it must fail rather than mask the problem.



\### 7.6 `IdentitySeed`



Responsibilities:



\* Read `DataSeedingOptions`.

\* Skip administrator creation when `CreateSeedAdmin` is `false`.

\* Validate the required bootstrap administrator configuration when enabled.

\* Create the user through `UserManager<User>.CreateAsync()`.

\* Assign the intended Administrator role through `UserManager<User>.AddToRoleAsync()`.



User creation and role assignment should be wrapped in one database transaction when the Identity store shares the scoped `AppDbContext`.



This ensures that a failure during role assignment can roll back the user creation, provided both operations participate in the same transaction.



\#### Create-if-absent behavior



The seeder does not modify an existing administrator account.



In particular, it does not:



\* Reset an existing password.

\* Update existing account details.

\* Automatically reconcile role membership for an existing user.



This is intentional. Account maintenance belongs to the relevant Identity and administrative workflows.



\#### Concurrent creation



Two instances attempting to create the same bootstrap administrator simultaneously may cause one instance to fail on a uniqueness conflict.



The current design does not specially handle this race. The failure propagates, and a subsequent restart may succeed after the winning instance has committed its changes.



\---



\## 8. Configuration and Dependency Injection



\### 8.1 Options Model



\*\*Location:\*\* `Infrastructure/Security/Authorization/DataSeeding/Configuration/DataSeedingOptions.cs`



Illustrative options model:



```csharp

public sealed class DataSeedingOptions

{

&#x20;   public const string SectionName = "DataSeeding";



&#x20;   public bool CreateSeedAdmin { get; set; }



&#x20;   \[EmailAddress]

&#x20;   public string AdminEmail { get; set; } = string.Empty;



&#x20;   public string AdminPassword { get; set; } = string.Empty;

}

```



The conditional configuration rule is:



> When `CreateSeedAdmin` is enabled, both `AdminEmail` and `AdminPassword` must be present and valid.



The current design enforces this condition inside `IdentitySeed` before that seeder performs database writes.



If the application also uses options validation, keep the conditional rule consistent between startup options validation and the seeder. Avoid unconditional password requirements when bootstrap creation is disabled.



\### 8.2 Configuration File



Example `appsettings.json`:



```json

{

&#x20; "DataSeeding": {

&#x20;   "CreateSeedAdmin": false

&#x20; }

}

```



`AdminEmail` and `AdminPassword` are deliberately omitted.



\### 8.3 Environment Variables



For an explicitly enabled bootstrap operation:



```text

DataSeeding\_\_CreateSeedAdmin=true

DataSeeding\_\_AdminEmail=admin@example.com

DataSeeding\_\_AdminPassword=<secret>

```



The double underscore (`\_\_`) represents a configuration hierarchy separator in the standard .NET environment-variable configuration provider.



The password must not be committed to source control or written to application logs.



\### 8.4 Secret Management



In production, provide sensitive configuration through an approved secret-management solution, such as Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, or Google Cloud Secret Manager.



The appropriate provider must be registered and configured in the application. Supporting .NET configuration does not, by itself, make a secret manager available.



Keep bootstrap administrator creation disabled during normal production operation. Prefer a controlled, auditable bootstrap procedure over leaving a permanent bootstrap credential configured.



\### 8.5 Dependency-Injection Registration



Infrastructure registration follows this general structure:



```text

InfrastructureServiceRegistration.AddInfrastructure(configuration)

&#x20;   │

&#x20;   └── DataSeedingExtensions.AddDataSeeding(configuration)

&#x20;       │

&#x20;       ├── Bind DataSeedingOptions

&#x20;       ├── Register PermissionSeed as scoped

&#x20;       ├── Register RoleSeed as scoped

&#x20;       ├── Register RolePermissionSeed as scoped

&#x20;       ├── Register IdentitySeed as scoped

&#x20;       └── Register StartupSeeder as IHostedService

```



`DataSeedingExtensions.AddDataSeeding()` resides in the Infrastructure layer.



`AuthorizationSourcesExtensions.AddAuthorizationSources()` resides in the Application layer and is called from `ApplicationServiceRegistration.AddApplication()`.



The final DI composition must include all required persistence, Identity, and authorization-source registrations.



\---



\## 9. Startup Sequence



The intended startup order is:



```text

Database migrations

&#x20;       ↓

Build application host

&#x20;       ↓

Hosted-service startup

&#x20;       ↓

StartupSeeder.StartAsync()

&#x20;       │

&#x20;       ├── PermissionSeed

&#x20;       │       ↓

&#x20;       │   Own transaction

&#x20;       │

&#x20;       ├── RoleSeed

&#x20;       │       ↓

&#x20;       │   Own transaction

&#x20;       │

&#x20;       ├── RolePermissionSeed

&#x20;       │       ↓

&#x20;       │   Own transaction

&#x20;       │

&#x20;       └── IdentitySeed

&#x20;               ↓

&#x20;           Own transaction

&#x20;       ↓

Application readiness

&#x20;       ↓

HTTP requests are served

```



\### 9.1 Migration Ordering



EF Core migrations must complete before the seeders access the schema they require.



In the intended application composition, migrations are invoked from `Program.cs` before `app.Run()`.



The startup implementation must verify that the migrations have completed successfully before the host begins the seeding lifecycle.



\### 9.2 Readiness Guarantee



The application must not serve authorization-dependent requests against incomplete seeded state.



Because hosted services participate in host startup, the application should verify its actual startup lifecycle rather than relying on an assumed ordering.



The implementation should ensure:



\* Migrations finish before seeding begins.

\* The four seeders execute in their intended order.

\* A required seeder failure prevents the application from becoming ready.

\* Health checks and deployment readiness signals reflect initialization failure.

\* Requests cannot bypass initialization gating if the application composition permits early request processing.



A readiness check alone is insufficient if the server can still serve requests that require the seeded state. Use startup orchestration or explicit request-processing gating where necessary.



\---



\## 10. Transaction Boundaries



Each seeder is an independent synchronization boundary.



| Seeder               | Transaction behavior                                                                                                       |

| -------------------- | -------------------------------------------------------------------------------------------------------------------------- |

| `PermissionSeed`     | Opens a transaction when permissions are missing; inserts the missing records and commits together. Rolls back on failure. |

| `RoleSeed`           | Uses its own transaction; `RoleManager` participates through the configured Identity store and shared `AppDbContext`.      |

| `RolePermissionSeed` | Opens a transaction when mappings are missing; persists the mappings and commits together. Rolls back on failure.          |

| `IdentitySeed`       | Wraps bootstrap user creation and role assignment in a transaction when both operations use the same database transaction. |



\### 10.1 Consequences



\* A failure in one seeder does not roll back earlier seeders that have already committed.

\* The failed seeder's writes should be rolled back as a unit.

\* A later startup can reconcile the remaining missing state.

\* A successful repeat run should insert no additional records when the database already contains the desired baseline state.

\* A transaction boundary does not automatically make concurrent inserts race-free; database constraints and the seeder's exception policy remain important.



\### 10.2 Validation Before Writes



Declaration validation should complete before the corresponding seeder opens a write transaction.



For example, `RolePermissionSeed` should validate all role names and expanded permission sets before inserting any mappings.



This prevents invalid declarations from causing partial writes within that seeder.



It does not mean that an error in a later seeder can roll back earlier committed seeders. Each seeder remains an independent transaction boundary.



\---



\## 11. Idempotency and Concurrency



\### 11.1 Idempotency



Each reconciliation compares the declared state with persisted state and inserts the missing difference.



Conceptually:



```text

Missing = Declared − Existing

```



The comparison keys are:



| Entity                  | Comparison key                                                                                       |

| ----------------------- | ---------------------------------------------------------------------------------------------------- |

| Permission              | `Name`                                                                                               |

| Role                    | `Name`                                                                                               |

| Role-permission mapping | `(RoleId, PermissionId)`                                                                             |

| Bootstrap administrator | Configured account identity, typically normalized email or the application's defined unique identity |



A successful second startup should produce no additional inserts when the database already contains the intended state.



This behavior assumes stable declaration identities, appropriate database uniqueness constraints, and no concurrent or external process changing the data during reconciliation.



\### 11.2 Additive-Only Semantics



No seeder automatically deletes persisted authorization data.



Consequently:



\* Removing a permission declaration does not remove its database row.

\* Removing a permission from a role declaration does not automatically revoke an existing mapping.

\* Removing a role declaration does not automatically delete the persisted role.

\* Existing role-permission mappings can remain even when they are absent from the current declaration.



An explicit migration or administrative operation is required for removals and revocations.



This means the database is reconciled toward the declared baseline but is not guaranteed to become an exact copy of all declarations.



\### 11.3 Multi-Instance Race Behavior



The four seeders have intentionally different exception policies.



\#### `RolePermissionSeed`



This is the only seeder that may treat an expected duplicate-mapping insert as a benign race.



It must identify the relevant uniqueness conflict, roll back the affected transaction, and confirm that the desired mapping already exists before considering the outcome successful.



If that cannot be established, startup must fail.



\#### `PermissionSeed` and `RoleSeed`



These seeders do not specially handle unique-key races.



If two instances attempt the same insert, the losing instance can fail startup. Restarting it after the winning instance commits may allow the next run to complete without further inserts.



This is a deliberate fail-loudly policy.



\#### `IdentitySeed`



Concurrent bootstrap administrator creation can result in one instance failing on a unique-key conflict. The current implementation does not treat this as a benign race.



The failure is visible and may be recoverable through a restart after the winning instance commits.



\### 11.4 Operational Implications



The deployment process should account for the documented race behavior.



If several application instances are deployed concurrently, operators should understand that a losing instance may require a restart.



Do not automatically retry every uniqueness violation without checking the violated constraint and resulting database state. Unique-key errors can indicate defects as well as expected races.



\---



\## 12. Failure Modes



| Scenario                                                          | Expected behavior                                                                                     |

| ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |

| Duplicate permission identity across sources                      | `PermissionSeed` fails before database writes.                                                        |

| Duplicate role name across role declarations                      | `RoleSeed` fails before database writes.                                                              |

| A role references an unknown permission                           | `RolePermissionSeed` fails before mapping writes.                                                     |

| An expanded role permission set contains duplicates               | `RolePermissionSeed` fails before mapping writes.                                                     |

| Database contains permissions or roles absent from code           | Records are retained; orphan counts are logged.                                                       |

| Expected duplicate mapping insertion during `RolePermissionSeed`  | The implementation may handle the verified race after rollback and confirmation of the desired state. |

| Unique-key violation during `PermissionSeed` or `RoleSeed`        | Exception propagates; the seeder transaction rolls back; startup fails.                               |

| Concurrent bootstrap administrator creation                       | Exception propagates; startup fails for the losing instance.                                          |

| An unrelated database error occurs during insertion               | Exception propagates; the seeder transaction rolls back.                                              |

| `CreateSeedAdmin` is enabled but required credentials are missing | `IdentitySeed` fails before database writes.                                                          |

| Identity rejects the bootstrap account or password                | Creation fails, and the transaction is rolled back where supported.                                   |

| A seeder fails after an earlier seeder commits                    | The failed seeder rolls back; the earlier commit remains.                                             |

| The host cannot complete required seeding                         | The application must not be considered ready to serve dependent requests.                             |



\---



\## 13. Testing



\### 13.1 Unit Tests



Test declaration and validation logic independently of the database where practical.



Recommended coverage:



\* `IPermissionSource.Describe()` returns the intended constant string fields.

\* `IRole` implementations return their exact expected permission sets.

\* Duplicate permission names are rejected.

\* Duplicate role names are rejected.

\* Unknown permission references are rejected.

\* Duplicate permissions within an expanded role set are rejected.

\* Wildcard expansion includes all currently declared permissions.

\* Configuration validation rejects missing bootstrap credentials when creation is enabled.

\* Identity failures are propagated with useful diagnostic information.

\* Unique-key exception handling is narrowly scoped to the expected mapping race.



\### 13.2 Integration Tests



Use the real relational database provider for behavior that depends on SQL Server transactions, constraints, or provider-specific exceptions.



Recommended coverage:



1\. \*\*Fresh database:\*\* Verify that the expected permissions, roles, and mappings are inserted.

2\. \*\*Repeated startup:\*\* Verify that a second successful run inserts no additional records.

3\. \*\*Permission addition:\*\* Verify that a new declaration is persisted on the next successful run.

4\. \*\*Role addition:\*\* Verify that the role and its declared mappings are persisted.

5\. \*\*Wildcard expansion:\*\* Verify that a newly declared permission is granted to wildcard roles.

6\. \*\*Transaction rollback:\*\* Inject a failure into a seeder and verify that its writes are rolled back.

7\. \*\*Independent transactions:\*\* Verify that earlier committed seeders remain committed when a later seeder fails.

8\. \*\*Concurrent mapping inserts:\*\* Start two hosts against a fresh database and verify the actual `RolePermissionSeed` race behavior.

9\. \*\*Permission and role insert races:\*\* Verify that the losing host fails as designed and can succeed after a safe restart.

10\. \*\*Startup readiness:\*\* Verify that initialization failure prevents the application from becoming ready.

11\. \*\*Orphan behavior:\*\* Verify that removed declarations are logged but their persisted records are not automatically deleted.



Concurrency tests must verify actual database constraints and transaction behavior rather than assuming every SQL Server error `2601` or `2627` represents a harmless race.



\### 13.3 Authorization Integration Tests



After seeding, verify that:



\* A user assigned the Administrator role can access operations permitted by the configured Administrator permission set.

\* A user assigned a specific role can access operations permitted by that role's effective permissions.

\* A user without the required permission is denied access.

\* Unauthenticated requests are handled as unauthenticated by the API authorization and exception-mapping pipeline.

\* Revoked role-permission mappings stop granting access according to the application's cache invalidation and authorization design.



An Administrator's ability to pass every permission check is guaranteed only if the Administrator role declaration or supported wildcard expansion actually grants every relevant permission.



\---



\## 14. What This Subsystem Eliminates



The design eliminates several forms of manual synchronization:



\* \*\*Hand-maintained permission lists:\*\* Permission identities are discovered from `IPermissionSource` implementations.

\* \*\*Hand-maintained role discovery lists:\*\* Roles are discovered from `IRole` implementations.

\* \*\*Hardcoded database identifiers:\*\* Reconciliation uses stable names and relationship keys rather than fixed GUIDs.

\* \*\*Separate permission registration for seeding:\*\* A new declaration is discovered automatically when its assembly is included in the scan.

\* \*\*Silent declaration errors:\*\* Duplicate identities and unknown permission references fail validation.

\* \*\*Repeated insertion of existing records:\*\* Set-difference reconciliation and database constraints support idempotent execution.

\* \*\*Unbounded partial writes within a seeder:\*\* Each seeder's writes are grouped in its own transaction.



The subsystem does not eliminate the need for explicit migrations when authorization records or mappings must be removed or renamed.



\---



\## 15. What Is Not Included



The following capabilities are deliberately outside this subsystem:



\* \*\*Deletion or renaming:\*\* Handled through controlled data migrations or explicit administrative operations.

\* \*\*Migration-time reference data:\*\* Use EF Core `HasData` or another appropriate migration mechanism when reference data is introduced.

\* \*\*Continuous reconciliation:\*\* Seeding runs once per startup.

\* \*\*HTTP exposure:\*\* No controllers or endpoints are provided.

\* \*\*Existing administrator reconciliation:\*\* An existing bootstrap account's password and role membership are not automatically changed.

\* \*\*Uniform multi-instance race handling:\*\* Only the expected role-permission mapping race receives special handling.

\* \*\*Business data initialization:\*\* Products, orders, and other operational records are not seeded here.

\* \*\*Runtime authorization enforcement:\*\* This remains the responsibility of the authorization subsystem.



\---



\## 16. Summary



Data Seeding is a startup reconciliation pipeline that creates missing authorization state from declarations maintained in the Application layer.



Its key properties are:



\* \*\*Contract-based declarations:\*\* `IPermissionSource` and `IRole` define the integration surface.

\* \*\*Automatic discovery:\*\* Scrutor discovers supported declaration classes in configured assemblies.

\* \*\*Set-difference reconciliation:\*\* Missing records and mappings are inserted.

\* \*\*Additive-only behavior:\*\* Orphaned records are retained and logged.

\* \*\*Independent transactions:\*\* Each seeder owns its own transaction boundary.

\* \*\*Fail-fast validation:\*\* Invalid declarations prevent the relevant seeder from writing.

\* \*\*Explicit orchestration:\*\* `StartupSeeder` invokes the four seeders in dependency order.

\* \*\*Controlled concurrency:\*\* The expected role-permission race can be handled narrowly; other races fail loudly.

\* \*\*Protected configuration:\*\* Bootstrap administrator credentials are supplied through protected configuration providers.

\* \*\*Startup readiness:\*\* Required initialization must complete before the application is considered ready to serve dependent requests.



\*\*The Application layer declares authorization state. Infrastructure discovers and reconciles it. The database stores the resulting state. Runtime authorization enforces the applicable rules.\*\*



\---



\*\*Document Version:\*\* 1.0 — Final

\*\*Feature:\*\* Data Seeding / Startup Authorization Reconciliation



