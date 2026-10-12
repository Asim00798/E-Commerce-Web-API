\# Data Seeding — Integration Guide



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Platform:\*\* .NET 8 / ASP.NET Core / EF Core / SQL Server

\*\*Document Type:\*\* Cross-Cutting Capability — Integration Guide

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Purpose



This guide explains how bounded contexts integrate with the Data Seeding subsystem.



It covers:



\* How a context declares authorization permissions and roles.

\* How the startup pipeline discovers those declarations.

\* How the pipeline validates and reconciles authorization data.

\* How multi-instance startup races are handled.

\* Configuration and operational expectations.

\* Common integration scenarios and troubleshooting.



The intended audience includes developers adding a context, permission, or role, and operators responsible for understanding startup behavior.



The integration surface is deliberately narrow:



\*\*Contexts declare authorization state; the Data Seeding pipeline discovers, validates, and reconciles it.\*\*



Bounded contexts do not register individual seeders, directly modify authorization mapping tables, or invoke the startup pipeline.



\---



\## 2. Integration Overview



Data Seeding is a \*\*cross-cutting startup capability\*\*, not a bounded context.



Contexts integrate with it by implementing two contracts:



\* `IPermissionSource` — declares permission identities.

\* `IRole` — declares a system role and its permission set.



The pipeline handles discovery, validation, persistence, transactions, and logging.



```text

┌────────────────────────────────────────────────────────────┐

│                    Bounded Contexts                        │

│                                                            │

│  ┌────────────────────────┐  ┌──────────────────────────┐  │

│  │ ContextPermissions.cs  │  │ Role Declaration         │  │

│  │                        │  │                          │  │

│  │ IPermissionSource      │  │ IRole                    │  │

│  └────────────┬───────────┘  └────────────┬─────────────┘  │

└───────────────┼────────────────────────────┼────────────────┘

&#x20;               │                            │

&#x20;               └──────────────┬─────────────┘

&#x20;                              │

&#x20;                    Scrutor Discovery

&#x20;                   AddAuthorizationSources()

&#x20;                              │

&#x20;                              ▼

&#x20;             ┌─────────────────────────────────┐

&#x20;             │      Startup Seeding Pipeline   │

&#x20;             │                                 │

&#x20;             │  PermissionSeed                 │

&#x20;             │       ↓                         │

&#x20;             │  RoleSeed                       │

&#x20;             │       ↓                         │

&#x20;             │  RolePermissionSeed             │

&#x20;             │       ↓                         │

&#x20;             │  IdentitySeed                   │

&#x20;             └────────────────┬────────────────┘

&#x20;                              │

&#x20;                              ▼

&#x20;                        Database State

&#x20;                              │

&#x20;                              ▼

&#x20;                     Runtime Authorization

```



The context does not depend on the pipeline's implementation. The pipeline consumes the context's declarations through the contracts.



\### 2.1 Architectural Responsibilities



| Component               | Responsibility                                              |

| ----------------------- | ----------------------------------------------------------- |

| Bounded contexts        | Declare permission identities and role permission sets.     |

| Authorization contracts | Define the discovery surface.                               |

| Scrutor registration    | Discover and register declaration classes.                  |

| Data Seeding pipeline   | Validate declarations and reconcile persisted state.        |

| EF Core / Identity      | Persist authorization state.                                |

| Runtime authorization   | Enforce permissions and roles during application execution. |



\---



\## 3. Integration Points



\### 3.1 Contracts Implemented by Contexts



| Contract            | Location                                               | Purpose                                         |

| ------------------- | ------------------------------------------------------ | ----------------------------------------------- |

| `IPermissionSource` | `Application/Shared/Security/Authorization/Contracts/` | Identifies a source of permission declarations. |

| `IRole`             | `Application/Shared/Security/Authorization/Contracts/` | Declares a system role and its permission set.  |



\### 3.2 Dependencies Consumed by the Pipeline



| Dependency                        | Registration Source                                    |

| --------------------------------- | ------------------------------------------------------ |

| `IEnumerable<IPermissionSource>`  | Scrutor discovery through `AddAuthorizationSources()`. |

| `IEnumerable<IRole>`              | The same discovery registration.                       |

| `AppDbContext`                    | Persistence registration.                              |

| `IUnitOfWork`                     | Persistence registration.                              |

| `RoleManager<IdentityRole<Guid>>` | ASP.NET Core Identity registration.                    |

| `UserManager<User>`               | ASP.NET Core Identity registration.                    |

| `DataSeedingOptions`              | Configuration section named `DataSeeding`.             |



Bounded contexts do not inject these dependencies to participate in seeding. The pipeline consumes them.



\### 3.3 What Contexts Must Not Do



A bounded context must not:



\* Register an individual seeder.

\* Invoke `StartupSeeder`.

\* Directly maintain role-permission mapping data as part of its feature implementation.

\* Reference `StartupSeeder`, `PermissionSeed`, `RoleSeed`, `RolePermissionSeed`, or `IdentitySeed`.

\* Depend on the `DataSeeding` configuration section.

\* Duplicate permission or role declarations maintained elsewhere.



The context declares its authorization identities. The cross-cutting pipeline manages their persistence.



\---



\## 4. Declaring Permissions



\### 4.1 Declaration Structure



Declare permissions in a sealed class implementing `IPermissionSource`.



\*\*Location:\*\*



`Application/Shared/Security/Authorization/Permissions/`



Example:



```csharp

using E\_Commerce.Application.Shared.Security.Authorization.Contracts;



namespace E\_Commerce.Application.Shared.Security.Authorization.Permissions;



public sealed class OrderPermissions : IPermissionSource

{

&#x20;   public const string Read = "Orders.Read";

&#x20;   public const string Place = "Orders.Place";

&#x20;   public const string Cancel = "Orders.Cancel";

&#x20;   public const string Deliver = "Orders.Deliver";

}

```



Each permission is represented by a stable string identity.



\### 4.2 Discovery



`AddAuthorizationSources()` registers declaration classes through Scrutor.



```csharp

services.Scan(scan => scan

&#x20;   .FromAssemblies(Assembly.GetExecutingAssembly())

&#x20;   .AddClasses(classes => classes.AssignableTo<IPermissionSource>())

&#x20;   .AsImplementedInterfaces()

&#x20;   .WithSingletonLifetime());

```



The scan must include the assembly or assemblies containing the declaration classes. If declarations are distributed across multiple assemblies, include all relevant assemblies explicitly.



The classes are registered as singleton implementations of `IPermissionSource`.



`PermissionSeed` consumes `IEnumerable<IPermissionSource>` and obtains permission identities through `Describe()`, which is supplied by the contract's default interface implementation.



Under the documented contract, `Describe()` discovers the declaration class's supported public constant string fields through reflection.



\*\*Adding a permission therefore requires no separate registration or manually maintained central list\*\*, provided the declaration class is discoverable by the configured assembly scan.



\### 4.3 Startup Processing



At startup, `PermissionSeed` performs the following operations:



1\. \*\*Validate declarations.\*\* Detect duplicate permission identities before performing database writes.

2\. \*\*Read existing permissions.\*\* Retrieve the persisted permission names.

3\. \*\*Compute missing permissions.\*\* Identify declared permissions that are not yet persisted.

4\. \*\*Insert missing permissions.\*\* Create records with new `Guid` identifiers.

5\. \*\*Report reconciliation results.\*\* Log declared, inserted, and orphaned permission counts.



A permission is considered orphaned when its database record no longer corresponds to a current code declaration.



Orphaned records are logged but not automatically deleted.



\### 4.4 Declaration Rules



\* \*\*Class name:\*\* `<Context>Permissions`.

\* \*\*Type:\*\* Sealed class implementing `IPermissionSource`.

\* \*\*Fields:\*\* Public `const string` declarations supported by `Describe()`.

\* \*\*Naming convention:\*\* `<Context>.<Action>`, such as `Orders.Read`.

\* \*\*No custom `Describe()` implementation:\*\* Use the default interface implementation unless the contract is deliberately changed.

\* \*\*Uniqueness:\*\* A permission identity must be unique across all discovered sources.



Avoid `static readonly` fields, instance fields, and unrelated members in declaration classes if the discovery contract expects only permission constants.



\### 4.5 When to Add a Permission



Add a permission when an operation requires a distinct authorization boundary.



Prefer permissions that express a meaningful capability, such as:



\* `Orders.Read`

\* `Orders.Cancel`

\* `Catalog.ManageProducts`



Permissions should be granular enough to support different roles without creating unnecessary duplication.



\*\*Permission names are persisted identifiers.\*\* Renaming one should be treated as a data migration, not merely a source-code refactoring.



\---



\## 5. Declaring Roles



\### 5.1 Centralized Role Names



Role names are defined in `SystemRoles`.



Example:



```csharp

public static class SystemRoles

{

&#x20;   public const string Administrator = "Administrator";

&#x20;   public const string Support = "Support";

&#x20;   public const string Employee = "Employee";

&#x20;   public const string Customer = "Customer";

&#x20;   public const string CatalogManager = "CatalogManager";

&#x20;   public const string Dispatcher = "Dispatcher";

&#x20;   public const string Driver = "Driver";

&#x20;   public const string Packaging = "Packaging";

}

```



Each system role should reference its corresponding constant rather than repeating a string literal.



\### 5.2 Role Declaration Structure



Declare each system role in a sealed class implementing `IRole`.



\*\*Location:\*\*



`Application/Shared/Security/Authorization/Roles/`



Example:



```csharp

using E\_Commerce.Application.Shared.Security.Authorization.Contracts;

using E\_Commerce.Application.Shared.Security.Authorization.Permissions;



namespace E\_Commerce.Application.Shared.Security.Authorization.Roles;



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



The role class declares the role's baseline permission set. It does not inherit another role's permission set or reference another role as a means of composing permissions.



\### 5.3 Discovery



The same authorization-source registration discovers role classes implementing `IRole`.



`RoleSeed` consumes `IEnumerable<IRole>` to reconcile role identities.



`RolePermissionSeed` consumes the role declarations to determine which permission mappings should exist.



\### 5.4 Role Reconciliation



`RoleSeed` performs the following operations:



1\. Validates role declarations for duplicate names.

2\. Reads existing role names from ASP.NET Core Identity.

3\. Creates missing roles through `RoleManager<IdentityRole<Guid>>`.

4\. Logs the reconciliation results.



Using `RoleManager` allows the Identity role store to apply its normal role-creation behavior, including role-name normalization.



\### 5.5 Role-Permission Reconciliation



`RolePermissionSeed` performs the following operations:



1\. Validates role declarations and their permission references.

2\. Rejects references to permission identities that are not declared by any discovered `IPermissionSource`.

3\. Rejects duplicate permission entries within a role's expanded permission set.

4\. Expands the wildcard declaration, if used.

5\. Reads existing role-permission pairs.

6\. Inserts missing mappings.

7\. Logs the reconciliation results.



Orphaned role records and role-permission mappings are reported but not automatically deleted.



The exact validation order should be verified in the implementation. In particular, \*\*all declaration validation intended to prevent partial writes should complete before the first database modification in that seeding operation\*\*.



\### 5.6 Role Declaration Rules



\* \*\*Class name:\*\* `<RoleName>Role`.

\* \*\*Role name:\*\* Reference a `SystemRoles` constant.

\* \*\*Permission set:\*\* Return the role's complete declared baseline permission set.

\* \*\*Role composition:\*\* Do not inherit or reference another role's permission set.

\* \*\*Wildcard:\*\* Use `IRole.AllPermissions` only when the role is intentionally meant to receive every declared permission.

\* \*\*Uniqueness:\*\* Every discovered role must have a unique name.



\### 5.7 Wildcard Behavior



If `IRole.AllPermissions` represents `"\*"`, the seeding pipeline expands that declaration against the current set of declared permissions.



When wildcard expansion is implemented as part of `RolePermissionSeed`, a newly declared permission is granted to wildcard roles on the next successful seeding run.



The wildcard applies to \*\*declared permissions\*\*, not arbitrary orphaned permission records already in the database.



Because wildcard roles automatically receive future permissions, restrict the wildcard to roles whose security responsibilities genuinely require this behavior. The Administrator role is the typical example.



\### 5.8 When to Add a Role



Add a role when a distinct audience requires a meaningfully different authorization profile.



Prefer adding or adjusting permissions when the difference between two roles is small, rather than introducing a new role for every minor permission variation.



Roles define broad access profiles; permissions define individual capabilities.



\---



\## 6. Startup Sequence



The intended startup lifecycle is:



```text

Database migrations

&#x20;       ↓

Build application host

&#x20;       ↓

Run startup seeding pipeline

&#x20;       ↓

PermissionSeed

&#x20;       ↓

RoleSeed

&#x20;       ↓

RolePermissionSeed

&#x20;       ↓

IdentitySeed

&#x20;       ↓

Application becomes ready to serve requests

```



Each seeder has its own transaction boundary:



| Order | Seeder               | Responsibility                                 |

| ----- | -------------------- | ---------------------------------------------- |

| 1     | `PermissionSeed`     | Reconcile permission identities.               |

| 2     | `RoleSeed`           | Reconcile system roles.                        |

| 3     | `RolePermissionSeed` | Reconcile role-permission mappings.            |

| 4     | `IdentitySeed`       | Optionally create the bootstrap administrator. |



The pipeline is intended to run after database migrations and before the application serves requests that depend on the seeded authorization state.



\### 6.1 Transaction Boundaries



Each seeder owns its own transaction. The seeders may share the same scoped `AppDbContext`, but their transactions are independent.



This means a later seeder's failure does not automatically roll back an earlier seeder's committed changes.



The overall startup process should fail if a required seeder fails. However, previously committed seeders may have completed successfully, so the database can contain partially reconciled state until the next successful startup.



\### 6.2 Startup Readiness Requirement



Do not assume that registering an `IHostedService` alone guarantees the web server cannot accept requests before seeding finishes.



The host's lifecycle and readiness behavior must be verified against the actual application composition.



If authorization data must exist before any relevant request is processed, ensure that migrations and seeding complete before the application begins accepting such requests, or gate readiness and request processing until initialization succeeds.



In this application, the migration and seeding order should be explicit in the startup composition and covered by an integration test.



\---



\## 7. Reconciliation Rules



| Rule                        | Behavior                                                                                       |

| --------------------------- | ---------------------------------------------------------------------------------------------- |

| \*\*Additive-only\*\*           | Seeders insert missing state but do not automatically delete or rename existing records.       |

| \*\*Idempotent\*\*              | Repeated successful runs converge on the same declared baseline state.                         |

| \*\*Comparison keys\*\*         | Permissions and roles are compared by name; mappings are compared by `(RoleId, PermissionId)`. |

| \*\*Fail-fast validation\*\*    | Invalid declarations should be rejected before writes within the relevant seeding operation.   |

| \*\*Wildcard expansion\*\*      | Wildcard roles expand against the current declared permission set on each run.                 |

| \*\*Per-seeder transactions\*\* | Each seeder commits or rolls back its own work independently.                                  |

| \*\*Startup failure\*\*         | Required seeding failures prevent the application from being considered ready.                 |

| \*\*Orphan reporting\*\*        | Records no longer declared in code are logged but not automatically deleted.                   |



\### 7.1 Additive-Only Does Not Mean Fully Declarative



The database is reconciled additively against code declarations.



Adding a permission or role can create the missing database record. Removing a declaration does not remove the corresponding record or its mappings.



Likewise, if runtime administration can add role-permission mappings, those additional mappings can remain in the database even when they are absent from a role's declared baseline.



Therefore, the database is not guaranteed to become an exact copy of the role declarations. Explicit migration or administrative cleanup is required for removals.



\---



\## 8. Multi-Instance Startup Behavior



Multiple application instances may start concurrently and attempt to insert the same missing records.



The documented implementation intentionally handles unique-constraint violations differently across the seeders.



| Seeder               | Behavior on SQL Server errors 2601 / 2627                                                                               |

| -------------------- | ----------------------------------------------------------------------------------------------------------------------- |

| `RolePermissionSeed` | Handles a verified duplicate-insert race by rolling back the affected transaction, logging the outcome, and continuing. |

| `PermissionSeed`     | Propagates the exception; startup fails.                                                                                |

| `RoleSeed`           | Propagates the exception; startup fails.                                                                                |

| `IdentitySeed`       | Propagates the exception; startup fails.                                                                                |



\### 8.1 Why the Behavior Differs



`RolePermissionSeed` validates its intended role-permission pairs before writing them. The implementation treats a duplicate-key error on the expected mapping constraint as a possible concurrent insertion race.



The other seeders use a stricter fail-fast policy. A unique-key violation could indicate a race, but it could also indicate an unexpected uniqueness conflict or a defect in the seeding logic.



The application therefore fails rather than assuming every unique-key violation is harmless.



\### 8.2 Conditions for Safely Handling a Race



A unique-constraint violation should be treated as a benign race only when the implementation can establish that:



\* The violated constraint is the expected one.

\* The conflicting record represents a mapping another instance has already inserted.

\* The current transaction is rolled back before execution continues.

\* The resulting database state satisfies the intended mapping.

\* The exception is not masking an unrelated integrity problem.



Do not swallow all SQL errors 2601 or 2627 indiscriminately. The constraint and operation context matter.



For the other three seeders, restart may succeed after a competing instance commits its changes. This is an operational recovery strategy, not a guarantee that every possible unique-key failure will be resolved by restarting.



\---



\## 9. Configuration



\### 9.1 Configuration Section



The `DataSeeding` section controls optional bootstrap administrator creation.



Example `appsettings.json`:



```json

{

&#x20; "DataSeeding": {

&#x20;   "CreateSeedAdmin": false

&#x20; }

}

```



`AdminEmail` and `AdminPassword` are deliberately omitted. They are environment-specific configuration values, and the password is a secret.



\### 9.2 Environment Variables



When bootstrap administrator creation is intentionally enabled:



```text

DataSeeding\_\_CreateSeedAdmin=true

DataSeeding\_\_AdminEmail=admin@example.com

DataSeeding\_\_AdminPassword=<secret>

```



The double underscore (`\_\_`) represents a configuration hierarchy separator in .NET.



Environment variables override corresponding JSON configuration values under the standard configuration provider ordering, assuming environment variables are registered after the JSON provider.



\### 9.3 Secret Managers



In production, supply sensitive values through an approved secret-management provider, such as Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, or Google Cloud Secret Manager, using the application's configured provider integration.



The logical configuration keys remain:



```text

DataSeeding:CreateSeedAdmin

DataSeeding:AdminEmail

DataSeeding:AdminPassword

```



Provider registration and configuration loading must be explicitly implemented; a secret manager is not automatically available merely because it supports .NET configuration.



\### 9.4 Configuration Behavior



| `CreateSeedAdmin` | `AdminEmail` / `AdminPassword` | Expected Result                                                                          |

| ----------------- | ------------------------------ | ---------------------------------------------------------------------------------------- |

| `false`           | Any values                     | Bootstrap administrator creation is skipped.                                             |

| `true`            | Both present and valid         | The administrator is created if absent, and the intended administrator role is assigned. |

| `true`            | Either missing or invalid      | `IdentitySeed` fails with a clear configuration or validation error.                     |



The precise validation rules should be implemented in `DataSeedingOptions` and/or `IdentitySeed`, including validation of non-empty values and any required email or password constraints.



\### 9.5 Production Guidance



\* Keep `CreateSeedAdmin` disabled during normal production operation.

\* Bootstrap the first administrator through a controlled operational procedure.

\* If the seed flag is temporarily enabled for deployment, disable it immediately afterward.

\* Never commit administrator passwords or other secrets to source control.

\* Do not log secret values.

\* Avoid placing secrets in deployment scripts, shell history, or diagnostic output.

\* Use a least-privilege deployment identity for migrations and seeding where operationally feasible.



\---



\## 10. Integration Rules



\### 10.1 Mandatory Rules



\* Declare permissions as public `const string` fields on sealed classes implementing `IPermissionSource`.

\* Declare roles as sealed classes implementing `IRole`.

\* Reference `SystemRoles` constants for role names.

\* Ensure permission identities are unique across all discovered sources.

\* Ensure role names are unique across all discovered role declarations.

\* Do not reference permission identities that are absent from the discovered declarations.

\* Do not implement a custom `Describe()` method unless the contract is intentionally being changed.

\* Do not use static permission-source classes when the discovery contract requires an instance implementing `IPermissionSource`.

\* Do not inherit or compose role declarations through other role classes.

\* Never place administrator passwords in `appsettings.json` or source control.

\* Do not let ordinary seeding silently delete persisted authorization records.

\* Ensure all declaration assemblies are included in discovery.

\* Ensure seeding finishes before the application is considered ready to process requests that depend on the seeded state.



\### 10.2 Recommended Practices



\* Follow the `<Context>.<Action>` permission naming convention.

\* Prefer meaningful, granular permissions over unnecessarily broad permissions.

\* Reserve wildcard permissions for explicitly approved roles, typically the Administrator role.

\* Keep persisted permission and role names stable.

\* Treat renames and removals as controlled data migrations.

\* Monitor orphan counts and investigate unexpected growth.

\* Test both clean-database initialization and repeated startup.

\* Verify the actual transaction and exception-handling behavior under concurrent startup.



\---



\## 11. Common Integration Scenarios



\### 11.1 A New Context Needs Permissions



1\. Create `<Context>Permissions.cs` under `Application/Shared/Security/Authorization/Permissions/`.

2\. Implement `IPermissionSource`.

3\. Declare the required permission constants.

4\. Verify that the class's assembly is included in `AddAuthorizationSources()`.

5\. Start the application after applying the required database migrations.

6\. Confirm the startup logs and persisted permission records.



No individual seeder registration or manual permission list is required when discovery is configured correctly.



\### 11.2 Adding a Permission to an Existing Context



1\. Add the new `const string` to the existing permission-source class.

2\. Add it to the appropriate role's `GetPermissions()` result if that role should receive it.

3\. Ensure the relevant authorization check uses the new permission identity.

4\. Start the application and verify that the permission and intended mappings are reconciled.



Declaring a permission does not automatically grant it to every role. It must be included in the applicable role declaration or granted through an explicitly supported runtime administration mechanism.



\### 11.3 Adding a Role



1\. Add a constant to `SystemRoles`.

2\. Create `<RoleName>Role.cs` implementing `IRole`.

3\. Reference the corresponding `SystemRoles` constant.

4\. Declare the role's intended baseline permissions.

5\. Verify discovery and startup logs.

6\. Confirm the role and mappings in the database.



\### 11.4 Granting a New Permission to a Wildcard Role



If a role uses `IRole.AllPermissions`, no additional role declaration change is needed for a newly declared permission, provided wildcard expansion is implemented as documented.



After a successful startup seeding run, the new permission should be included in the wildcard role's mappings.



\### 11.5 Removing a Permission



Removing a constant from code does not delete the persisted permission or its role mappings.



Use a controlled data migration or explicit administrative cleanup.



Recommended process:



1\. Identify all authorization checks that use the old permission.

2\. Remove or replace those checks in code.

3\. Remove the permission from relevant role declarations.

4\. Prepare a migration that deletes the associated `RolePermissions` mappings and then the `Permissions` record, respecting foreign-key constraints.

5\. Deploy the code and migration in a controlled order.

6\. Verify that no remaining code or persisted configuration depends on the old permission.



Until cleanup occurs, the orphaned permission may remain in the database. If runtime authorization checks continue to reference it, its persisted presence may still affect authorization behavior. Removing a constant alone is not a security revocation.



\### 11.6 Adding Another Seeder



Adding a new seeder should be uncommon. It is appropriate when a new category of startup state requires its own reconciliation responsibility.



For example, a future tenant-role mapping might require a dedicated seeder.



The integration steps are:



1\. Create the scoped seeder under `DataSeeding/Seeders/`.

2\. Define its transaction and failure-handling behavior.

3\. Add it to `StartupSeeder.StartAsync()` in the appropriate order.

4\. Register it through `DataSeedingExtensions.AddDataSeeding()`.

5\. Add unit and integration tests.

6\. Document its startup dependencies and multi-instance behavior.



Do not add a new seeder merely to avoid extending an existing seeder whose responsibility already includes the relevant state.



\### 11.7 Rotating the Bootstrap Administrator Password



The pipeline uses create-if-absent semantics. It does not reset an existing administrator's password on each startup.



To rotate the password:



1\. Use the appropriate authentication or Identity password-change/reset workflow, such as `UserManager` through the intended application service.

2\. Verify the successful password change and relevant audit records.

3\. Keep the bootstrap seed configuration disabled during normal operation.



Deleting an existing administrator simply to trigger reseeding is generally not recommended. It can disrupt references, audit history, or operational access. Use that approach only if the application's account lifecycle explicitly supports it and the operation is controlled.



\---



\## 12. Troubleshooting



| Symptom                                               | Likely Cause                                                                                    | Resolution                                                                                                             |

| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |

| Startup fails with duplicate permission identities    | Two discovered sources declare the same permission string.                                      | Locate the duplicate declarations and retain one canonical identity.                                                   |

| Startup fails with duplicate role identities          | Two `IRole` implementations return the same name.                                               | Ensure each role declaration references a unique `SystemRoles` constant.                                               |

| A role references an unknown permission               | `GetPermissions()` returns a permission not declared by any discovered source.                  | Add the missing declaration or remove the invalid reference.                                                           |

| Permission exists in code but not in the database     | Seeding did not run, discovery omitted the class, or the seeder failed.                         | Verify `AddAuthorizationSources()`, assembly scanning, migrations, and startup logs.                                   |

| Startup fails on a unique-key violation               | Possible multi-instance race or an unexpected uniqueness conflict.                              | Inspect the violated constraint and operation. Restart only when the failure is understood and retry is safe.          |

| Bootstrap administrator configuration is missing      | `CreateSeedAdmin` is enabled but required values are absent or invalid.                         | Supply valid `DataSeeding\_\_AdminEmail` and `DataSeeding\_\_AdminPassword` values through the configured secret provider. |

| Orphaned permissions appear in startup logs           | A permission was removed from code without removing its database record.                        | Expected under additive-only behavior. Use a controlled migration if removal is intended.                              |

| An old permission still appears in authorization data | Its database record or mapping was not removed.                                                 | Inspect persisted mappings and apply the appropriate migration or administrative cleanup.                              |

| A role has no expected permissions                    | The declaration is empty, mappings were not reconciled, or a referenced permission was invalid. | Verify `GetPermissions()`, declaration discovery, validation logs, and `RolePermissionSeed`.                           |

| A wildcard role lacks a new permission                | The latest seeding run did not complete, or wildcard expansion is not implemented as expected.  | Verify the pipeline and wildcard expansion implementation, then rerun seeding.                                         |

| Requests arrive before seeding completes              | Host readiness does not gate request processing on initialization.                              | Make startup initialization explicit or gate readiness and relevant requests until seeding succeeds.                   |

| A secret appears in logs                              | Configuration or exception logging exposes sensitive values.                                    | Remove secret logging, review log retention, and rotate the exposed credential if necessary.                           |



\---



\## 13. Testing



\### 13.1 Unit Tests



Test the declaration and reconciliation logic independently of the database where practical.



Recommended coverage:



\* `IPermissionSource.Describe()` returns the expected supported constants.

\* `IRole.GetPermissions()` returns the expected permission set.

\* Duplicate permission identities are rejected.

\* Duplicate role names are rejected.

\* Unknown role permission references are rejected.

\* Duplicate permissions within an expanded role set are rejected.

\* Wildcard expansion includes all currently declared permissions.

\* Missing records and mappings are identified correctly.

\* Existing records are not deleted by additive reconciliation.

\* SQL exceptions are handled only under the intended, narrowly defined conditions.



\### 13.2 Integration Tests



Use a real relational database provider for tests that depend on SQL Server constraints or transaction behavior.



Recommended coverage:



\* Fresh database: all expected permissions, roles, and mappings are created.

\* Repeated startup: a second run inserts no duplicate records.

\* Permission addition: the new permission appears after a successful startup.

\* Role addition: the role and its baseline mappings are persisted.

\* Wildcard expansion: a newly declared permission is granted to wildcard roles after reconciliation.

\* Failure behavior: a failed seeder rolls back its own transaction.

\* Startup readiness: the application does not become ready before required initialization completes.

\* Concurrent startup: two instances attempting the same inserts produce the documented results.

\* Orphan handling: removed declarations are logged but not deleted automatically.



Concurrency tests should verify the actual SQL Server constraint and transaction behavior rather than assuming all unique-key violations represent benign races.



\### 13.3 Authorization Tests



After seeding, verify that:



\* A user with the required role and permission mapping passes the corresponding authorization check.

\* A user without the required permission is rejected.

\* An unauthenticated request is handled as unauthenticated by the configured API authorization and exception-mapping pipeline.

\* An authenticated user who lacks permission receives a forbidden result.

\* Permission removal or mapping revocation takes effect according to the application's caching and authorization design.



\---



\## 14. Summary



Integrating a bounded context with Data Seeding requires declaring its authorization identities through `IPermissionSource` and, where applicable, `IRole`.



The pipeline discovers these declarations, validates them, and reconciles the corresponding database state.



Key properties:



\* \*\*Contract-based integration:\*\* Contexts declare; the pipeline reconciles.

\* \*\*Automatic discovery:\*\* Scrutor registers supported declaration classes from configured assemblies.

\* \*\*Centralized persistence:\*\* Contexts do not maintain authorization mapping tables directly.

\* \*\*Additive reconciliation:\*\* Missing records are created; removals require explicit cleanup.

\* \*\*Fail-fast validation:\*\* Invalid declarations prevent the affected seeding operation from proceeding.

\* \*\*Idempotency:\*\* Repeated successful runs converge on the same declared baseline state.

\* \*\*Independent transactions:\*\* Each seeder has its own transaction boundary.

\* \*\*Controlled concurrency:\*\* Multi-instance behavior is defined per seeder and must be verified against actual database constraints.

\* \*\*Secure configuration:\*\* Bootstrap administrator credentials are supplied through protected configuration providers.

\* \*\*Startup readiness:\*\* Required seeding must finish successfully before the application is considered ready to serve dependent requests.



\*\*The context declares. The pipeline validates and reconciles. The database stores the resulting authorization state. Runtime authorization enforces the applicable rules.\*\*



\---



\*\*Document Version:\*\* 1.0 — Final

\*\*Feature:\*\* Data Seeding / Cross-Cutting Integration Guide



