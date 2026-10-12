# Authorization Feature — Requirements Technical Document

**Project:** E-Commerce Modular Monolith
**Version:** 1.5
**Status:** Final
**Scope:** Roles, Permissions, Policies, Permission/Operation Access Checks, Startup Data Seeding, and Future Resource-Based Authorization

---

## 1. Introduction

This document defines the technical requirements for the Authorization feature of the E-Commerce modular monolith.

Authorization determines what an authenticated user is permitted to do. It is distinct from authentication, which establishes the user's identity.

The feature provides five primary capability areas:

1. **Roles** — role definitions and user-role assignment.
2. **Permissions** — fine-grained operations that can be granted to roles.
3. **Policies** — dynamic authorization policies that map permission names to access requirements.
4. **Permission/Operation Access Checks** — enforcement of permission requirements at API and application layers.
5. **Startup Data Seeding** — reconciliation of authorization declarations in code with persisted database state.

A future, optional capability for resource-based authorization is documented in Section 3.6. It is not currently part of the implementation scope.

## 2. Scope

### 2.1 In Scope

* Role management using ASP.NET Core Identity's built-in role system.
* Custom permission entities and persistence.
* Many-to-many role-permission mapping.
* A dynamic `IAuthorizationPolicyProvider` that converts permission policy names into authorization policies.
* A `PermissionAuthorizationHandler` that evaluates the current user's permissions.
* An application-level `IPermissionService` abstraction for permission checks.
* Administrative commands and queries for managing roles, permissions, and assignments, if the optional management module is implemented.
* Integration with MediatR for centralized application-level authorization checks.
* API-level `[Authorize(Policy = "Permission:...")]` enforcement.
* Reflection-based discovery of authorization declarations.
* Startup reconciliation of declared permissions, roles, and role-permission mappings.
* Fail-fast validation of authorization declarations before any database writes.

### 2.2 Out of Scope

* Authentication and login, covered by the Authentication feature.
* External authentication provider management.
* User profile management, covered by the Identity/People features.
* Resource-based or row-level data authorization, except for the future approach documented in Section 3.6.
* Multi-tenant isolation policies.
* Hierarchical role inheritance.
* Custom authorization based on dynamic resource attributes.
* Automatic deletion or renaming of database entries that no longer appear in code. Such changes require explicit data migrations or administrative operations.

---

## 3. Functional Requirements

### 3.1 Roles

| ID   | Requirement                                                                                                                                                                                           |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R-01 | The system shall use ASP.NET Core Identity's built-in role persistence, based on `IdentityRole<Guid>`. A separate custom `Role` entity is unnecessary unless additional role-domain data is required. |
| R-02 | The system shall support administrative creation, updating, and deletion of roles when role management is implemented.                                                                                |
| R-03 | The system shall provide a query to list roles.                                                                                                                                                       |
| R-04 | The system shall provide a query to retrieve a role by ID.                                                                                                                                            |
| R-05 | The system shall support assigning a role to a user.                                                                                                                                                  |
| R-06 | The system shall support removing a role from a user.                                                                                                                                                 |
| R-07 | The system shall provide a query to list the roles assigned to a user.                                                                                                                                |
| R-08 | Role persistence and user-role assignments shall use the built-in Identity role store and APIs.                                                                                                       |
| R-09 | System roles shall be declared in code, with one declaration class per role implementing `IRole`.                                                                                                     |
| R-10 | Each role declaration shall specify its name through a constant defined in `SystemRoles` and declare the permissions granted to that role.                                                            |
| R-11 | Role declarations shall be discoverable without a manually maintained registration list.                                                                                                              |

**Implementation notes:**

* Use `RoleManager<IdentityRole<Guid>>` for role management.
* Use `UserManager<User>.AddToRoleAsync` and `RemoveFromRoleAsync` for user-role assignments.
* The role declaration contract is an application-level description of a system role. It is not itself the persisted Identity role entity.
* Role discovery must be registered and implemented explicitly, for example through Scrutor scanning for concrete `IRole` implementations.

### 3.2 Permissions

| ID   | Requirement                                                                                                                                                                                                      |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| P-01 | The system shall persist permissions in the `security.Permissions` table.                                                                                                                                        |
| P-02 | Each permission shall have a unique name, such as `Accounts.Activate` or `Accounts.Read`.                                                                                                                        |
| P-03 | The system shall support administrative creation, updating, and deletion of permissions if permission management is implemented.                                                                                 |
| P-04 | The system shall provide a query to list permissions.                                                                                                                                                            |
| P-05 | The system shall provide queries to retrieve permissions by ID or name.                                                                                                                                          |
| P-06 | Permission identifiers shall be centralized in constants rather than repeated as magic strings in business logic.                                                                                                |
| P-07 | Adding a permission shall not require changes to the policy provider or authorization handler. The permission must be declared in code and reconciled into the database before it can be granted through a role. |
| P-08 | Permission declarations shall be grouped by bounded context, with one class per context implementing `IPermissionSource`.                                                                                        |
| P-09 | Each permission source shall declare permissions as `public const string` fields. Its permission set shall be derivable from those constants without a separate manually maintained list.                        |
| P-10 | Permission declarations shall be discoverable without a manually maintained registration list.                                                                                                                   |

**Implementation notes:**

* Permission constants describe the authorization vocabulary used by the application.
* The `Permission` entity represents persisted permission records.
* A permission declaration does not automatically grant access. The permission must exist in the database, be assigned to a role, and be available through the user's assigned roles.
* Permission names should be treated as stable identifiers. Renaming one requires an explicit migration or reconciliation strategy for existing mappings.

### 3.3 Dynamic Authorization Policies

| ID     | Requirement                                                                                                                                                          |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| POL-01 | The system shall support permission-based authorization using policy names in the format `Permission:{PermissionName}`.                                              |
| POL-02 | A custom `IAuthorizationPolicyProvider` shall dynamically create policies for permission policy names.                                                               |
| POL-03 | The system shall provide a `PermissionRequirement` containing the required permission name.                                                                          |
| POL-04 | A `PermissionAuthorizationHandler` shall evaluate whether the current authenticated user satisfies the permission requirement.                                       |
| POL-05 | Policy construction shall be deterministic and cacheable to avoid repeated construction. Policy-object caching shall remain distinct from permission-result caching. |
| POL-06 | API and application authorization shall use the same permission identifiers and consistent access semantics.                                                         |

**Implementation notes:**

* The custom policy provider should delegate non-permission policy names to the default provider.
* Policy objects can be cached because a policy is derived from a stable permission identifier.
* If permission evaluation results are cached, the implementation must define expiration or invalidation behavior so that role and permission changes do not leave stale authorization decisions indefinitely.
* API role authorization requires the authenticated principal to contain the appropriate role claims or use a correctly configured role-aware authorization mechanism. Identity role membership alone does not guarantee that a JWT contains role claims.
* An unknown permission name should not grant access. A permission policy must fail closed when the user lacks the required permission.

### 3.4 Permission and Operation Access Checks

| ID    | Requirement                                                                                                                                                                                                                   |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-01 | The Application layer shall expose an `IPermissionService` abstraction with a method equivalent to `HasPermissionAsync(Guid userId, string permission)`.                                                                      |
| AC-02 | Use cases requiring application-level permission checks shall enforce those checks before performing protected operations.                                                                                                    |
| AC-03 | When an application-level permission check fails, the use case shall return a consistent forbidden result, such as `Result.Failure("Forbidden.")`. The API layer shall translate that result into HTTP 403 where appropriate. |
| AC-04 | API endpoints shall support `[Authorize(Policy = "Permission:...")]` for permission-based access control.                                                                                                                     |
| AC-05 | Authorization shall be enforced at both the API and application layers where appropriate. Both layers shall apply consistent permission semantics.                                                                            |
| AC-06 | Sensitive authorization-management data shall not be exposed to unauthorized users.                                                                                                                                           |
| AC-07 | Failed permission checks may be logged at Debug level, subject to the application's logging and security policies.                                                                                                            |
| AC-08 | Permission evaluation shall be optimized, with suitable database indexes and query patterns.                                                                                                                                  |
| AC-09 | Administrative authorization-management operations shall require appropriate permissions, such as `Authorization.Manage`.                                                                                                     |
| AC-10 | Permissions shall be granted through roles. Direct user-permission assignments are excluded unless explicitly introduced as a separate requirement.                                                                           |

**Implementation notes:**

1. The Application layer shall depend on `IPermissionService`, not on EF Core or ASP.NET Core authorization framework types.
2. Infrastructure shall implement the permission service and database-backed evaluation.
3. API authorization protects the HTTP boundary. Application authorization protects the use case from being executed through another entry point that bypasses the API controller.
4. A MediatR authorization behavior may enforce permissions declared through application-level attributes, provided that the behavior is registered and its attribute-discovery rules are well defined.
5. Avoid unintentionally requiring the same check multiple times through attributes, pipeline behaviors, and manual checks. Each use case should have a clear enforcement strategy.
6. Authenticated users who lack permission should normally receive 403 Forbidden. Unauthenticated users should normally receive 401 Unauthorized. The API contract must apply this distinction consistently.
7. Resource ownership and other record-specific access rules are separate from general permission checks and are covered by Section 3.6.

### 3.5 Startup Data Seeding and Reconciliation

The startup seeding pipeline reconciles the authorization declarations in code with the persisted authorization data before the application accepts HTTP requests.

It is an Infrastructure responsibility. It does not expose HTTP endpoints and does not require background jobs.

| ID    | Requirement                                                                                                                                                                                                                                                                                                                                                                                                             |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| DS-01 | The system shall reconcile authorization data at application startup, after the required database migrations have completed and before the host accepts HTTP requests.                                                                                                                                                                                                                                                  |
| DS-02 | Reconciliation shall be additive-only. Seeders shall not delete or rename permissions, roles, or role-permission mappings.                                                                                                                                                                                                                                                                                              |
| DS-03 | Reconciliation shall be idempotent. Repeated execution against unchanged declarations and database state shall produce no additional inserts and shall preserve the same logical state.                                                                                                                                                                                                                                 |
| DS-04 | The pipeline shall validate declarations before opening any seeding transaction or performing database writes. Validation errors shall include duplicate permission identities across sources, duplicate role names, references to undeclared permissions, and duplicate permission entries within a role declaration.                                                                                                  |
| DS-05 | Each seeder—permissions, roles, role-permission mappings, and seed administrator—shall own its transaction where transactional persistence is applicable. A failure in one seeder shall not roll back a previously completed seeder.                                                                                                                                                                                    |
| DS-06 | Any unhandled seeding exception shall prevent successful host startup. The application shall not begin serving requests with incomplete authorization data.                                                                                                                                                                                                                                                             |
| DS-07 | Each seeder shall log an Information-level outcome summary containing declared, added, and orphaned counts where those counts are meaningful. Orphaned records are database entries not represented by current code declarations.                                                                                                                                                                                       |
| DS-08 | A role that declares the wildcard permission constant `"*"` shall receive all permissions currently declared in code. The wildcard shall be expanded on every reconciliation run so newly declared permissions are included automatically.                                                                                                                                                                              |
| DS-09 | The system shall support optional seed administrator creation. It shall be disabled by default and enabled only in development or through a controlled, one-time environment bootstrap.                                                                                                                                                                                                                                 |
| DS-10 | The seed administrator password shall never be committed to source control or stored in `appsettings.json`. It shall be supplied through development user secrets or an appropriate environment variable or secret manager.                                                                                                                                                                                             |
| DS-11 | Seed administrator creation shall use `UserManager<User>` so Identity applies its normal password hashing, validation, normalization, and security-stamp behavior.                                                                                                                                                                                                                                                      |
| DS-12 | Seed administrator creation shall use create-if-absent semantics. The seeder shall not modify an existing administrator's password, profile, or other account properties.                                                                                                                                                                                                                                               |
| DS-13 | For role-permission mappings, a SQL Server unique-constraint violation (error 2601 or 2627) caused by a concurrent insert may be treated as a benign multi-instance startup race. The failed transaction shall be rolled back, and the seeder shall verify or safely assume the expected mapping exists only if that conclusion is supported by the database state. Other `DbUpdateException` failures shall propagate. |
| DS-14 | For permission, role, and seed administrator creation, unique-constraint violations shall not be silently swallowed. The losing instance shall fail startup and may recover on a subsequent restart after the winning instance has completed seeding.                                                                                                                                                                   |
| DS-15 | General reference data, such as countries, languages, and currencies, shall not be created by this authorization seeding pipeline. When required, such data shall use an appropriate separate mechanism, such as EF Core `HasData` in the relevant `IEntityTypeConfiguration<T>`, where that approach fits the data's lifecycle.                                                                                        |

#### 3.5.1 Validation and Execution Order

The startup pipeline shall follow this sequence:

1. Complete required database migrations.
2. Discover all permission-source and role declaration types.
3. Materialize permission and role declarations.
4. Validate duplicate identities and role-to-permission references.
5. Expand wildcard permissions against the current declared permission set.
6. Execute the permission seeder.
7. Execute the role seeder.
8. Execute the role-permission mapping seeder.
9. Execute the optional seed administrator seeder.
10. Log each seeder's outcome and complete startup initialization.

The declaration-validation phase must finish before any seeder performs database writes.

The exact order of role creation and role-permission mapping is mandatory: roles must exist before their mappings are inserted. Permission records must exist before mappings reference them.

#### 3.5.2 Additive-Only Reconciliation

Reconciliation shall insert missing declared records and missing role-permission mappings. Existing records not declared in code shall be reported as orphaned, but they shall not be deleted or renamed.

This distinction is important because a permission or role may have been provisioned manually or may still be referenced by existing operational data.

#### 3.5.3 Multi-Instance Startup

The pipeline must account for multiple application instances starting concurrently.

* Unique database constraints remain the final protection against duplicate records.
* `RolePermissionSeed` may handle a known duplicate-insert race by rolling back the failed transaction and verifying the expected mapping exists.
* Other seeders shall propagate unique-constraint failures according to DS-14.
* Race handling shall be narrowly scoped to the intended SQL Server constraint violations. Unrelated database errors must not be treated as successful reconciliation.

A successful seeding run on one instance does not guarantee that another instance's transaction can safely continue after a unique-constraint violation. The losing transaction must be rolled back before subsequent database operations are attempted.

#### 3.5.4 Wildcard Permission Semantics

The wildcard `"*"` is a declaration-time convenience, not a persisted permission granting unrestricted access by itself.

When a role declares `"*"`, the seeder shall expand it into the current set of declared permission identifiers before generating role-permission mappings.

This means that adding a new permission declaration will grant it to wildcard roles on the next successful startup reconciliation. The behavior must be documented because it can broaden access automatically.

### 3.6 Future Capability: Simplified Resource-Based Authorization

Resource-based authorization determines whether the current user may access or modify a specific resource.

This capability is not currently implemented and is outside the current delivery scope.

#### Example

An order contains an `Order.UserId` property identifying its owner.

For a request to `GET /orders/123`, the application loads the order and compares its owner with the authenticated user's ID.

* If `Order.UserId` equals the current user's ID, the ownership condition is satisfied.
* If they differ, access is denied unless another explicit business rule permits access.

#### Relationship to Permission-Based Authorization

These mechanisms answer different questions:

* **Permission-based authorization:** May this user perform `Orders.Read`?
* **Resource-based authorization:** May this user access this particular order?

A use case may require both conditions:

1. The user is authenticated.
2. The user has the required general permission, where applicable.
3. The user owns the resource or satisfies another explicit resource-access rule.
4. The operation is allowed only when all required conditions pass.

Resource ownership must not be treated as a substitute for authentication or all permission checks.

#### Placement

When introduced, resource-based checks should live close to the use case in the Application layer or in an appropriate domain service when the rule is genuinely a domain invariant.

The implementation must load and validate the resource before making ownership decisions. It must also account for elevated access only when explicitly authorized by the applicable business rules.

---

## 4. Non-Functional Requirements

### 4.1 Performance

| ID     | Requirement                                                                                                                                     |
| ------ | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| NFR-01 | Permission checks shall avoid significant additional request latency. Database lookups shall use suitable indexes and efficient query patterns. |
| NFR-02 | The dynamic policy provider shall cache constructed policy objects where appropriate.                                                           |
| NFR-03 | Startup seeding shall run before request processing and shall not introduce seeding work into the normal request path.                          |

### 4.2 Scalability

| ID     | Requirement                                                                                                                                                     |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| NFR-04 | Authorization shall work correctly across multiple API instances using centrally persisted permission and role data.                                            |
| NFR-05 | Role and permission changes should take effect without an application restart, subject to the configured permission-result caching and claims-refresh strategy. |
| NFR-06 | Startup reconciliation shall handle concurrent instances according to DS-13 and DS-14.                                                                          |

### 4.3 Maintainability

| ID     | Requirement                                                                                      |
| ------ | ------------------------------------------------------------------------------------------------ |
| NFR-07 | Authorization logic shall remain separated from business logic.                                  |
| NFR-08 | Permission names shall be centralized as constants.                                              |
| NFR-09 | The Application layer shall not depend on EF Core or ASP.NET Core authorization framework types. |
| NFR-10 | Adding a permission or role shall not require updating a manually maintained discovery list.     |

### 4.4 Testability

| ID     | Requirement                                                                                                   |
| ------ | ------------------------------------------------------------------------------------------------------------- |
| NFR-11 | Permission services and authorization handlers shall be testable in isolation using appropriate test doubles. |
| NFR-12 | Policy creation, policy evaluation, and permission enforcement shall be covered by integration tests.         |
| NFR-13 | Declaration discovery and validation shall be testable without a database dependency.                         |

### 4.5 Flexibility

| ID     | Requirement                                                                                                                                                          |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| NFR-14 | Adding a permission shall not require modifying the policy provider or authorization handler. It shall require a declaration and successful database reconciliation. |
| NFR-15 | Adding a system role shall require a new `IRole` implementation and a corresponding constant in `SystemRoles`, without a manually maintained registration list.      |

---

## 5. Security Requirements

### 5.1 Access Control

| ID     | Requirement                                                                                                                               |
| ------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| SEC-01 | Administrative authorization-management operations shall require appropriate permissions, such as `Authorization.Manage`.                 |
| SEC-02 | Permissions shall be granted through roles unless direct user-permission assignments are explicitly introduced as a separate requirement. |
| SEC-03 | Authorization shall be enforced at the API and application layers where appropriate.                                                      |

### 5.2 Data Protection

| ID     | Requirement                                                                                                            |
| ------ | ---------------------------------------------------------------------------------------------------------------------- |
| SEC-04 | Permission and role data shall be protected against unauthorized modification.                                         |
| SEC-05 | Role-permission mappings shall be unique at the database level.                                                        |
| SEC-06 | The seed administrator password shall never be committed to source control, logged, or included in exception messages. |

### 5.3 Principle of Least Privilege

| ID     | Requirement                                                                                                                                                                                                           |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SEC-07 | Users shall receive only the permissions required for their responsibilities.                                                                                                                                         |
| SEC-08 | Administrative roles shall be distinct from normal user roles.                                                                                                                                                        |
| SEC-09 | Seed administrator creation shall be disabled by default in production. The initial production administrator shall be provisioned out-of-band or through a controlled, one-time bootstrap that is disabled after use. |

### 5.4 Audit and Monitoring

| ID     | Requirement                                                                                                                        |
| ------ | ---------------------------------------------------------------------------------------------------------------------------------- |
| SEC-10 | Role and permission changes should be audited when audit logging is implemented.                                                   |
| SEC-11 | Failed permission checks may be logged at Debug level, subject to the application's logging and security policies.                 |
| SEC-12 | Each startup seeding run shall log an Information-level summary that provides operational visibility into reconciliation outcomes. |

---

## 6. Integration Requirements

### 6.1 ASP.NET Core Identity

* Use `RoleManager<IdentityRole<Guid>>` for role management.
* Use `UserManager<User>.AddToRoleAsync` and `RemoveFromRoleAsync` for user-role assignments.
* The application user entity shall be `User : IdentityUser<Guid>`.
* Identity persistence and framework-specific user management shall remain in Infrastructure.

### 6.2 ASP.NET Core Authorization

* Use the built-in authorization pipeline with a custom `IAuthorizationPolicyProvider`.
* Use a custom `AuthorizationHandler<PermissionRequirement>` for permission evaluation.
* Register the policy provider and handler through dependency injection.
* Ensure role-based authorization has access to the necessary role claims or a correctly configured role-aware authorization mechanism.

### 6.3 EF Core

* Map `Permission` to `security.Permissions`.
* Map `RolePermission` to `security.RolePermissions`.
* Enforce uniqueness of permission names and role-permission pairs through database constraints or unique indexes.
* Ensure the EF model includes all authorization entities and configurations.
* Use explicit transactions where required by the seeding strategy.

### 6.4 MediatR

* Application use cases shall use the application-level permission service when explicit permission checks are required.
* An optional authorization pipeline behavior may enforce permission requirements declared through application-level attributes.
* If multiple authorization behaviors are registered, their ordering and combination semantics shall be explicit.
* Authorization behavior shall fail closed when required permission evaluation fails.

### 6.5 Application Abstractions

The Application layer shall define:

* `IPermissionService`
* `IPermissionSource`
* `IRole`
* Permission constants grouped by bounded context
* Role declarations grouped according to the application's authorization organization
* Optional management-service interfaces, such as `IPermissionManagementService`, `IRoleManagementService`, and `IUserRoleService`

These abstractions shall not depend on EF Core or ASP.NET Core authorization framework types.

### 6.6 Startup Seeding

* The startup seeding pipeline shall run through an `IHostedService` or an equivalent hosted initialization mechanism.
* Required database migrations shall complete before seeding starts.
* The pipeline shall run before the application begins accepting requests.
* The seeding services shall be registered through `InfrastructureServiceRegistration.AddInfrastructure(configuration)`.
* Permission-source and role-declaration types shall be discoverable through application assembly scanning or equivalent registration configured by `ApplicationServiceRegistration.AddApplication()`.
* Dependency injection must ensure the seeding service can create the necessary scopes for scoped database and Identity services.

**Operational constraint:** The host must not start accepting requests before the seeding pipeline completes successfully. Merely registering an `IHostedService` is insufficient if the chosen hosting and initialization arrangement does not enforce that startup dependency.

---

## 7. Configuration Requirements

### 7.1 Authorization Policy Caching

No additional mandatory configuration is required beyond the existing Identity and authorization setup.

An optional policy-cache setting may be introduced:

| Configuration Key             | Purpose                                                                                                | Example Default |
| ----------------------------- | ------------------------------------------------------------------------------------------------------ | --------------- |
| `Authorization:CacheDuration` | Controls the lifetime of cached policy objects if the implementation supports configurable expiration. | 5 minutes       |

This setting is optional. Policy-object caching must not be confused with caching a user's permission results.

### 7.2 Data Seeding

| Configuration Key             | Purpose                                   | Requirement                                           |
| ----------------------------- | ----------------------------------------- | ----------------------------------------------------- |
| `DataSeeding:CreateSeedAdmin` | Enables seed administrator creation.      | Optional; defaults to `false`.                        |
| `DataSeeding:AdminEmail`      | Email address for the seed administrator. | Required only when creation is enabled.               |
| `DataSeeding:AdminPassword`   | Password for the seed administrator.      | Required only when creation is enabled; secret value. |

The `appsettings.json` file may contain only the non-secret default:

```json
{
  "DataSeeding": {
    "CreateSeedAdmin": false
  }
}
```

`AdminEmail` and `AdminPassword` shall not be stored in `appsettings.json`.

Use development user secrets or suitable environment variables/secret-manager entries, such as:

```text
DataSeeding__AdminEmail
DataSeeding__AdminPassword
```

The application shall validate the required configuration when seed administrator creation is enabled. It shall not silently proceed with incomplete bootstrap configuration.

---

## 8. Authorization-Related Folder Structure

The following structure describes the intended organization. Optional management components should be included only if administrative authorization management is implemented.

### 8.1 Application Layer

```text
Application/
└── Shared/
    └── Security/
        └── Authorization/
            ├── Attributes/
            │   ├── AuthorizePermissionAttribute.cs
            │   └── AuthorizeRoleAttribute.cs
            │
            ├── Contracts/
            │   ├── IPermissionSource.cs
            │   └── IRole.cs
            │
            ├── Extensions/
            │   └── AuthorizationSourcesExtensions.cs
            │
            ├── Permissions/
            │   ├── AccountPermissions.cs
            │   ├── AuthorizationPermissions.cs
            │   ├── CatalogPermissions.cs
            │   ├── CustomerEngagementPermissions.cs
            │   ├── FilePermissions.cs
            │   ├── FinancePermissions.cs
            │   ├── OrderingPermissions.cs
            │   ├── PeoplePermissions.cs
            │   └── ShippingPermissions.cs
            │
            ├── Roles/
            │   ├── SystemRoles.cs
            │   ├── AdministratorRole.cs
            │   ├── SupportRole.cs
            │   ├── EmployeeRole.cs
            │   ├── CustomerRole.cs
            │   ├── CatalogManagerRole.cs
            │   ├── DispatcherRole.cs
            │   ├── DriverRole.cs
            │   └── PackagingRole.cs
            │
            └── Services/
                ├── IPermissionService.cs
                ├── IPermissionManagementService.cs
                ├── IRoleManagementService.cs
                └── IUserRoleService.cs
```

`AuthorizationSourcesExtensions.cs` may encapsulate scanning and registration for declaration types. If the implementation uses Scrutor directly in `ApplicationServiceRegistration`, a separate extension file is optional.

#### Optional Administrative Management Module

```text
Application/
└── Modules/
    └── Authorization/
        ├── Commands/
        │   ├── CreatePermission/
        │   ├── UpdatePermission/
        │   ├── DeletePermission/
        │   ├── CreateRole/
        │   ├── UpdateRole/
        │   ├── DeleteRole/
        │   ├── AssignPermissionToRole/
        │   ├── RemovePermissionFromRole/
        │   ├── AssignRoleToUser/
        │   └── RemoveRoleFromUser/
        │
        ├── Queries/
        │   ├── GetRoleById/
        │   ├── GetPermissionById/
        │   ├── ListRoles/
        │   ├── ListPermissions/
        │   └── ListPermissionsForRole/
        │
        └── Dtos/
            ├── RoleDto.cs
            ├── PermissionDto.cs
            └── RolePermissionDto.cs
```

The administrative module is optional because the core system can use code-declared permissions and roles without exposing general-purpose permission CRUD endpoints.

### 8.2 Infrastructure Authorization Layer

```text
Infrastructure/
└── Security/
    └── Authorization/
        ├── Entities/
        │   ├── Permission.cs
        │   └── RolePermission.cs
        │
        ├── Policies/
        │   ├── AuthorizationPolicyProvider.cs
        │   ├── PermissionRequirement.cs
        │   └── PermissionAuthorizationHandler.cs
        │
        ├── Services/
        │   ├── PermissionService.cs
        │   ├── PermissionManagementService.cs
        │   ├── RoleManagementService.cs
        │   └── UserRoleService.cs
        │
        └── DataSeeding/
            ├── Configuration/
            │   └── DataSeedingOptions.cs
            │
            ├── Execution/
            │   └── StartupSeeder.cs
            │
            ├── Extensions/
            │   └── DataSeedingExtensions.cs
            │
            └── Seeders/
                ├── PermissionSeed.cs
                ├── RoleSeed.cs
                ├── RolePermissionSeed.cs
                └── IdentitySeed.cs
```

Management-service implementations are optional when their corresponding management capabilities are not implemented.

### 8.3 Persistence Layer

```text
Infrastructure/
└── Persistence/
    ├── Context/
    │   └── AppDbContext.Security.cs
    │
    └── Modules/
        └── Security/
            └── Authorization/
                ├── Configurations/
                │   ├── PermissionConfiguration.cs
                │   └── RolePermissionConfiguration.cs
                │
                └── Repositories/
                    ├── PermissionRepository.cs
                    └── RolePermissionRepository.cs
```

`AppDbContext.Security.cs` registers the relevant `DbSet<Permission>` and `DbSet<RolePermission>` members. Entity configuration defines the database schema and unique constraints.

Repository implementations are needed only where they fit the persistence abstractions and access patterns. Identity roles and user-role assignments should continue to use the built-in Identity stores and managers.

### 8.4 Optional API Layer

```text
API/
└── Controllers/
    └── v1/
        └── Security/
            └── AuthorizationController.cs
```

The controller is needed only if administrative authorization-management endpoints are exposed.

### 8.5 Removed Folders

The following legacy locations are not part of the proposed structure:

```text
Application/Shared/Security/Authorization/Seeding/
Infrastructure/Persistence/DataSeeding/
```

Authorization declaration contracts and classes remain in Application. The startup reconciliation implementation remains in Infrastructure.

---

## 9. Assumptions and Constraints

### 9.1 Assumptions

| ID   | Assumption                                                                                                           |
| ---- | -------------------------------------------------------------------------------------------------------------------- |
| A-01 | Users authenticate through the Authentication feature before protected operations are executed.                      |
| A-02 | Roles and user-role assignments use ASP.NET Core Identity's built-in role system.                                    |
| A-03 | Permissions are custom application-level records persisted in the database.                                          |
| A-04 | Permission names are unique and treated as stable identifiers.                                                       |
| A-05 | Administrative authorization-management endpoints, if implemented, are restricted to appropriately authorized users. |
| A-06 | Required EF Core migrations complete before the startup seeding pipeline executes.                                   |

### 9.2 Constraints

| ID   | Constraint                                                                                                                                             |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| C-01 | Application must not reference EF Core or ASP.NET Core authorization framework types.                                                                  |
| C-02 | Permission enforcement shall use explicit application-level abstractions or the configured authorization pipeline rather than scattered magic strings. |
| C-03 | The Domain layer remains unchanged for the current authorization implementation.                                                                       |
| C-04 | Administrative commands shall enforce authorization before protected changes are executed.                                                             |
| C-05 | Seeding is additive-only and shall not delete or rename permission, role, or role-permission records.                                                  |
| C-06 | The seed administrator password shall never be committed to source control.                                                                            |
| C-07 | Seed administrator creation shall be disabled by default.                                                                                              |

---

## 10. Traceability Matrix

| Requirement Group        | Description                                                             | Main Components                                                                                                      |
| ------------------------ | ----------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| R-01–R-11                | Roles and user-role management                                          | `RoleManager<IdentityRole<Guid>>`, `IRole`, role declaration classes, `IRoleManagementService`, `IUserRoleService`   |
| P-01–P-10                | Permission persistence and declarations                                 | `Permission`, `IPermissionSource`, `*Permissions` classes, `PermissionConfiguration`, `IPermissionManagementService` |
| POL-01–POL-06            | Dynamic permission policies                                             | `AuthorizationPolicyProvider`, `PermissionRequirement`, `PermissionAuthorizationHandler`                             |
| AC-01–AC-10              | Permission and operation access checks                                  | `IPermissionService`, `PermissionService`, `[Authorize]`, MediatR authorization behavior and handlers                |
| DS-01–DS-15              | Startup data seeding                                                    | `StartupSeeder`, `PermissionSeed`, `RoleSeed`, `RolePermissionSeed`, `IdentitySeed`, `DataSeedingOptions`            |
| Section 3.6              | Future resource-based authorization                                     | Application use cases or an appropriate domain service                                                               |
| NFR-01–NFR-15            | Performance, scalability, maintainability, testability, flexibility     | Permission queries, indexes, policy caching, declaration discovery and validation, seeding pipeline                  |
| SEC-01–SEC-12            | Security, least privilege, audit, and monitoring                        | Authorization enforcement, database constraints, secret handling, audit/logging infrastructure                       |
| Integration requirements | Identity, ASP.NET Core Authorization, EF Core, MediatR, startup seeding | `AppDbContext.Security.cs`, infrastructure registrations, authorization policies and seeding extensions              |

---

## 11. Acceptance Criteria

The Authorization feature is considered complete when the following criteria are satisfied.

### Roles and Permissions

1. Roles can be created, retrieved, updated, and deleted when role-management operations are implemented.
2. Permissions can be created, retrieved, updated, and deleted when permission-management operations are implemented.
3. Permissions can be assigned to and removed from roles.
4. Users can be assigned to and removed from roles.
5. The system can query roles, permissions, and relevant role-permission relationships.
6. Role and permission declarations are discovered without a manually maintained registration list.

### Policies and Enforcement

7. The dynamic policy provider constructs policies for valid `Permission:{PermissionName}` policy names.
8. The permission authorization handler correctly evaluates the authenticated user's effective permissions.
9. Protected application use cases enforce the required permissions.
10. API endpoints can use `[Authorize(Policy = "Permission:...")]`.
11. Unauthenticated and unauthorized requests receive responses consistent with the API's security contract, normally 401 and 403 respectively.
12. Unauthorized users cannot access protected authorization-management data or operations.

### Startup Reconciliation

13. Startup reconciliation runs after the required migrations and before the application accepts requests.
14. The pipeline reconciles permissions, roles, and role-permission mappings from code declarations.
15. Adding a permission declaration or role declaration makes the missing record eligible for creation on the next successful startup, without a hand-maintained registration list.
16. Duplicate permission identities, duplicate role names, duplicate permission entries within a role, or references to undeclared permissions fail validation before any seeder performs database writes.
17. Repeated reconciliation against unchanged declarations is idempotent.
18. Orphaned database entries are reported but not automatically deleted.
19. The role-permission seeder handles the explicitly permitted unique-constraint race, while other seeders propagate their specified concurrency failures.
20. Any unhandled startup-seeding failure prevents the application from serving requests.
21. The seed administrator feature is disabled by default. When enabled, credentials come from user secrets or an appropriate environment/secret-management mechanism, never from committed source-controlled configuration.
22. Existing administrator accounts are not modified by the seed administrator seeder.

### Documentation and Boundaries

23. The folder structure reflects the intended separation between Application declarations, Infrastructure authorization implementation, and Persistence configuration.
24. Optional management endpoints and services are not required when the corresponding management capability is not implemented.
25. Resource-based authorization is documented as a future capability and is not represented as an implemented feature.

---

## 12. Summary

This document defines the requirements for five primary authorization capabilities: **roles, permissions, dynamic policies, permission/operation access checks, and startup data seeding**.

The feature integrates with ASP.NET Core Identity, ASP.NET Core Authorization, EF Core, and MediatR while maintaining the modular monolith's Application and Infrastructure boundaries.

The startup reconciliation pipeline discovers authorization declarations, validates them before database writes, and performs additive-only reconciliation. Each seeder has a defined transaction boundary and concurrency policy. Startup fails when an unhandled seeding error prevents the authorization state from being established reliably.

The pipeline does not expose an HTTP surface or require background jobs. It establishes the authorization data needed by the application before requests are accepted.

Resource-based authorization remains a documented future capability to be introduced only where use-case-specific business rules require it.
