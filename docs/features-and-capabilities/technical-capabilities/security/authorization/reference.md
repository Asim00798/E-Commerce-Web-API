# Authorization Feature — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Scope:** Roles, Permissions, Policies, Permission/Operation Access Checks, and Startup Data Seeding
**Version:** 1.1
**Status:** Final

---

## 1. Overview

The Authorization subsystem determines what an authenticated user is allowed to do. It is separate from Authentication, which establishes the user's identity.

The feature provides:

* **Role management** using ASP.NET Core Identity's built-in roles (`IdentityRole<Guid>`).
* **Custom permissions** persisted in the database.
* **Role-permission mapping** for fine-grained access control.
* **Dynamic authorization policies** for API-level permission enforcement.
* **Application-level permission and role services** for use-case authorization.
* **MediatR pipeline behaviors** for centralized authorization on commands and queries.
* **Administrative management operations** for roles, permissions, and assignments.
* **Declaration-based discovery** of permissions and roles through `IPermissionSource` and `IRole`.
* **Startup reconciliation** that synchronizes declared authorization data with persisted records.

The feature spans the Application, Infrastructure, Persistence, and API layers while preserving the modular monolith's architectural boundaries. The Domain layer remains unchanged.

### Version Scope

This reference describes the declaration model based on `IPermissionSource` and `IRole`, Scrutor-based discovery through `AuthorizationSourcesExtensions`, per-role permission declarations, and additive-only startup seeders.

---

## 2. Architecture and Placement

### 2.1 Roles and Permissions

The authorization model separates roles from permissions.

* **Roles** are persisted by ASP.NET Core Identity using `IdentityRole<Guid>` and declared in code through `IRole` implementations.
* **Permissions** are custom application-level authorization records persisted in `security.Permissions` and declared through `IPermissionSource` implementations.

Users receive permissions indirectly through their assigned roles.

```text
User
  │
  ▼
AspNetUserRoles
  │
  ▼
AspNetRoles
  │
  ▼
RolePermissions
  │
  ▼
Permissions
```

There are no direct user-permission assignments.

### 2.2 Layer Responsibilities

| Layer              | Responsibilities                                                                                                                                                                               |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Application**    | Commands, queries, handlers, DTOs, abstractions, authorization attributes, MediatR pipeline behaviors, permission constants, role declarations, discovery contracts, and discovery extensions. |
| **Infrastructure** | ASP.NET Core Identity integration, EF Core persistence implementation, dynamic policy provider, permission authorization handler, concrete services, and startup seeders.                      |
| **Persistence**    | Authorization entity configurations, database constraints, repositories, and `AppDbContext` integration.                                                                                       |
| **API**            | HTTP endpoints, role-based controller authorization, and optional permission-policy enforcement through ASP.NET Core authorization.                                                            |
| **Domain**         | No changes. Authorization remains an application and infrastructure concern for the current implementation.                                                                                    |

### 2.3 Folder Structure

#### Application Layer

```text
Application/
├── Shared/
│   ├── Behaviors/
│   │   ├── PermissionAuthorizationBehavior.cs
│   │   └── RoleAuthorizationBehavior.cs
│   │
│   └── Security/
│       └── Authorization/
│           ├── Attributes/
│           │   ├── AuthorizePermissionAttribute.cs
│           │   └── AuthorizeRoleAttribute.cs
│           │
│           ├── Contracts/
│           │   ├── IPermissionSource.cs
│           │   └── IRole.cs
│           │
│           ├── Extensions/
│           │   └── AuthorizationSourcesExtensions.cs
│           │
│           ├── Permissions/
│           │   ├── AccountPermissions.cs
│           │   ├── AuthorizationPermissions.cs
│           │   ├── CatalogPermissions.cs
│           │   ├── CustomerEngagementPermissions.cs
│           │   ├── FilePermissions.cs
│           │   ├── FinancePermissions.cs
│           │   ├── OrderingPermissions.cs
│           │   ├── PeoplePermissions.cs
│           │   └── ShippingPermissions.cs
│           │
│           ├── Roles/
│           │   ├── AdministratorRole.cs
│           │   ├── CatalogManagerRole.cs
│           │   ├── CustomerRole.cs
│           │   ├── DispatcherRole.cs
│           │   ├── DriverRole.cs
│           │   ├── EmployeeRole.cs
│           │   ├── PackagingRole.cs
│           │   ├── SupportRole.cs
│           │   └── SystemRoles.cs
│           │
│           └── Services/
│               ├── IPermissionManagementService.cs
│               ├── IPermissionService.cs
│               ├── IRoleManagementService.cs
│               └── IUserRoleService.cs
│
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
        │   ├── GetPermissionById/
        │   ├── GetRoleById/
        │   ├── ListPermissions/
        │   ├── ListPermissionsForRole/
        │   └── ListRoles/
        │
        └── Dtos/
            ├── PermissionDto.cs
            ├── RoleDto.cs
            └── RolePermissionDto.cs
```

#### Infrastructure Authorization Layer

```text
Infrastructure/
└── Security/
    └── Authorization/
        ├── DataSeeding/
        │   ├── Configuration/
        │   │   └── DataSeedingOptions.cs
        │   │
        │   ├── Execution/
        │   │   └── StartupSeeder.cs
        │   │
        │   ├── Extensions/
        │   │   └── DataSeedingExtensions.cs
        │   │
        │   └── Seeders/
        │       ├── IdentitySeed.cs
        │       ├── PermissionSeed.cs
        │       ├── RolePermissionSeed.cs
        │       └── RoleSeed.cs
        │
        ├── Entities/
        │   ├── Permission.cs
        │   └── RolePermission.cs
        │
        ├── Policies/
        │   ├── AuthorizationPolicyProvider.cs
        │   ├── PermissionRequirement.cs
        │   └── PermissionAuthorizationHandler.cs
        │
        └── Services/
            ├── PermissionService.cs
            ├── UserRoleService.cs
            ├── RoleManagementService.cs
            └── PermissionManagementService.cs
```

#### Persistence Layer

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

#### Optional API Layer

```text
API/
└── Controllers/
    └── AuthorizationController.cs
```

The API controller is required only when authorization-management endpoints are exposed.

---

## 3. ASP.NET Core Identity Integration

`AppDbContext` inherits from:

```csharp
IdentityDbContext<User, IdentityRole<Guid>, Guid>
```

The model follows these rules:

* `User` is the application user entity and inherits from `IdentityUser<Guid>`.
* Roles use the built-in `IdentityRole<Guid>` type. No custom role entity is required.
* Identity manages role persistence and user-role relationships.
* The Authorization feature adds the custom `Permission` and `RolePermission` entities.
* `RolePermission.RoleId` references `AspNetRoles.Id`.
* `RolePermission.PermissionId` references `Permissions.Id`.
* The `RolePermission` entity does not require navigation properties to the Identity role entity.

### 3.1 Persistence Relationships

```text
AspNetUsers
    │
    └── AspNetUserRoles
              │
              └── AspNetRoles
                        │
                        └── RolePermissions
                                  │
                                  └── Permissions
```

Identity owns user-role assignment persistence. The Authorization feature owns permission persistence and role-permission mapping.

---

## 4. Application Abstractions

### 4.1 Runtime Services

#### `IPermissionService`

```csharp
Task<bool> HasPermissionAsync(
    Guid userId,
    string permission,
    CancellationToken ct = default);
```

This service evaluates whether a user has a specified permission through their assigned roles.

#### `IUserRoleService`

```csharp
Task<bool> HasRoleAsync(
    Guid userId,
    string role,
    CancellationToken ct = default);
```

This service evaluates whether a user belongs to the specified role.

Both interfaces reside in:

```text
Application/Shared/Security/Authorization/Services/
```

They are consumed by MediatR authorization behaviors and by application handlers that require explicit runtime checks.

The Application layer depends on these abstractions, not on EF Core or ASP.NET Core authorization framework implementations.

### 4.2 Management Services

The Application layer defines management abstractions for administrative use cases:

* `IPermissionManagementService` — permission creation, updating, deletion, querying, and role assignment operations.
* `IRoleManagementService` — role creation, updating, deletion, querying, and relevant assignment operations.

User-role assignment may be exposed separately through `IUserRoleService` or a dedicated management abstraction, depending on the chosen interface design.

The corresponding commands and queries reside under:

```text
Application/Modules/Authorization/
```

Infrastructure implements the interfaces.

### 4.3 Authorization Attributes

#### `AuthorizePermissionAttribute`

Declares the permission required to execute a request.

Example:

```csharp
[AuthorizePermission(AccountPermissions.Lock)]
public sealed record LockAccountCommand(Guid UserId)
    : IRequest<Result>;
```

Multiple required permissions use **AND semantics** in this design: every declared permission must be satisfied.

That behavior must be implemented explicitly by `PermissionAuthorizationBehavior`. Attribute declarations alone do not enforce authorization.

#### `AuthorizeRoleAttribute`

Declares a required role for a request.

Example:

```csharp
[AuthorizeRole(SystemRoles.Administrator)]
public sealed record ReconcileAuthorizationCommand
    : IRequest<Result>;
```

The attribute supports a single required role in the documented design.

Both attributes are defined in:

```text
Application/Shared/Security/Authorization/Attributes/
```

They are Application-layer constructs consumed by the MediatR behaviors. They are distinct from ASP.NET Core's API-level `[Authorize]` attribute.

### 4.4 Permission Declarations — `IPermissionSource`

Permission names are centralized in classes implementing `IPermissionSource`.

```csharp
using System.Reflection;

public interface IPermissionSource
{
    IEnumerable<string> Describe() =>
        GetType()
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly)
            .Where(field =>
                field.IsLiteral &&
                !field.IsInitOnly &&
                field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();
}
```

The default interface method discovers public static string constants declared directly on the implementing class.

`BindingFlags.DeclaredOnly` makes the intended scope explicit: the source contributes its own declared constants rather than inherited fields.

A new permission can therefore be declared by adding a constant without maintaining a separate permission list.

#### Permission Sources

| Class                           | Owning Concern                 |
| ------------------------------- | ------------------------------ |
| `AccountPermissions`            | Identity and account lifecycle |
| `AuthorizationPermissions`      | Authorization management       |
| `CatalogPermissions`            | Catalog                        |
| `CustomerEngagementPermissions` | Customer engagement            |
| `FilePermissions`               | File storage                   |
| `FinancePermissions`            | Finance and payments           |
| `OrderingPermissions`           | Ordering                       |
| `PeoplePermissions`             | People                         |
| `ShippingPermissions`           | Shipping                       |

Example:

```csharp
public sealed class FinancePermissions : IPermissionSource
{
    public const string Read = "Payments.Read";
    public const string Refund = "Payments.Refund";
    public const string Manage = "Payments.Manage";
}
```

The constant values are the authoritative permission identifiers. They must match the names persisted in `security.Permissions`.

They are used by:

* Application authorization attributes.
* Role declarations.
* Startup reconciliation.
* Runtime permission evaluation.

Permission identifiers should be treated as stable contracts. Renaming one requires a deliberate migration or compatibility strategy for persisted role-permission mappings and existing code references.

### 4.5 Role Declarations — `IRole`

Roles are declared through `IRole` implementations.

```csharp
public interface IRole
{
    const string AllPermissions = "*";

    string Name { get; }

    IEnumerable<string> GetPermissions();
}
```

Each implementation defines:

* `Name` — the role's canonical name, sourced from `SystemRoles`.
* `GetPermissions()` — the permissions granted to that role.

The returned permission set may contain concrete permission identifiers or the `AllPermissions` wildcard.

#### System Role Constants

```csharp
public static class SystemRoles
{
    public const string Administrator = "Administrator";
    public const string Support = "Support";
    public const string Employee = "Employee";
    public const string CatalogManager = "CatalogManager";
    public const string Dispatcher = "Dispatcher";
    public const string Driver = "Driver";
    public const string Packaging = "Packaging";
    public const string Customer = "Customer";
}
```

Role names used in attributes and other application code should reference these constants instead of repeating raw strings.

#### Role Implementations

| Class                | Name Source                  | Typical Permission Set                        |
| -------------------- | ---------------------------- | --------------------------------------------- |
| `AdministratorRole`  | `SystemRoles.Administrator`  | All declared permissions through the wildcard |
| `SupportRole`        | `SystemRoles.Support`        | Operational permissions                       |
| `EmployeeRole`       | `SystemRoles.Employee`       | Explicitly declared permissions               |
| `CatalogManagerRole` | `SystemRoles.CatalogManager` | Catalog permissions                           |
| `DispatcherRole`     | `SystemRoles.Dispatcher`     | Shipping dispatch permissions                 |
| `DriverRole`         | `SystemRoles.Driver`         | Delivery permissions                          |
| `PackagingRole`      | `SystemRoles.Packaging`      | Packaging operations                          |
| `CustomerRole`       | `SystemRoles.Customer`       | Customer self-service permissions             |

Example:

```csharp
public sealed class AdministratorRole : IRole
{
    public string Name => SystemRoles.Administrator;

    public IEnumerable<string> GetPermissions() =>
        [IRole.AllPermissions];
}
```

The set of declared system roles is determined by the discovered concrete `IRole` implementations.

Adding a role requires a new declaration class and a corresponding `SystemRoles` constant. Database reconciliation creates the missing Identity role and its declared permission mappings.

**Important:** Removing a role class does not delete the persisted Identity role. Reconciliation is additive-only; deletion requires a separate, explicit data-management operation.

### 4.6 Discovery — `AddAuthorizationSources`

Permission and role declarations are discovered through Scrutor scanning.

```csharp
public static IServiceCollection AddAuthorizationSources(
    this IServiceCollection services)
{
    var assembly = typeof(IPermissionSource).Assembly;

    services.Scan(scan => scan
        .FromAssemblies(assembly)
        .AddClasses(classes =>
            classes.AssignableTo<IPermissionSource>())
        .AsImplementedInterfaces()
        .WithSingletonLifetime());

    services.Scan(scan => scan
        .FromAssemblies(assembly)
        .AddClasses(classes =>
            classes.AssignableTo<IRole>())
        .AsImplementedInterfaces()
        .WithSingletonLifetime());

    return services;
}
```

The extension is called from `ApplicationServiceRegistration.AddApplication()`.

Both declaration categories are registered as singletons because they are stateless descriptors consumed by startup reconciliation.

**Discovery constraint:** The scan must include every assembly containing declarations. If permission or role declarations are moved into another assembly, that assembly must be included in the scan.

---

## 5. MediatR Authorization Pipeline Behaviors

Authorization is enforced through two behaviors.

### 5.1 `RoleAuthorizationBehavior`

Responsibilities:

* Inspect the request type for `AuthorizeRoleAttribute`.
* Resolve the current user's identity through the application's current-user abstraction.
* Check role membership through `IUserRoleService`.
* Reject requests that do not satisfy the role requirement.

A missing required role results in `ForbiddenAccessException`.

### 5.2 `PermissionAuthorizationBehavior`

Responsibilities:

* Inspect the request type for `AuthorizePermissionAttribute`.
* Determine the current user's identity.
* Check each required permission through `IPermissionService`.
* Reject unauthenticated requests with an authentication-related exception.
* Reject authenticated users who lack a required permission with `ForbiddenAccessException`.

The exception mapping must distinguish unauthenticated access from authenticated-but-forbidden access. In particular, `UnauthorizedAccessException` should not be assumed to mean HTTP 401 automatically; the API's exception-handling configuration must explicitly implement the intended mapping.

### 5.3 Pipeline Order

The intended ordering is:

```text
ValidationBehavior
    ↓
RoleAuthorizationBehavior
    ↓
PermissionAuthorizationBehavior
    ↓
TelemetryBehavior
    ↓
TracingBehavior
    ↓
CachingBehavior
    ↓
Request Handler
```

The actual order depends on MediatR's registration and pipeline composition behavior. Registration order alone should not be treated as proof that the runtime execution order matches this diagram; verify it against the configured MediatR version and registrations.

Validation runs before authorization in the intended design. Telemetry and tracing are positioned inside the authorization behaviors so they do not necessarily execute for requests rejected by authorization.

If security auditing requires recording denied requests, ensure that logging or auditing is placed outside the authorization rejection path or is performed explicitly by the authorization behavior.

### 5.4 Enforcement Semantics

* A request with no authorization attribute passes through these authorization behaviors unless another mechanism protects it.
* A request with a role attribute must satisfy the required role.
* A request with multiple required permissions must satisfy all of them.
* Authorization must fail closed if permission evaluation fails or required authorization data cannot be obtained.
* Resource-specific ownership checks remain use-case-specific and are not replaced by these behaviors.

---

## 6. Dynamic API Authorization Policies

### 6.1 Policy Name Format

Permission policies use the following convention:

```text
Permission:{PermissionName}
```

Example:

```text
Permission:Accounts.Read
```

### 6.2 `AuthorizationPolicyProvider`

The custom policy provider:

* Recognizes permission policy names.
* Extracts the permission identifier.
* Builds a policy requiring an authenticated user and a `PermissionRequirement`.
* Caches constructed policies to avoid repeatedly rebuilding equivalent policy objects.
* Delegates policy names outside its own convention to the default policy provider.

Policy caching is separate from caching permission evaluation results.

### 6.3 `PermissionRequirement`

`PermissionRequirement` encapsulates the permission name that must be satisfied for the policy to succeed.

It should remain a small authorization requirement rather than contain persistence or business logic.

### 6.4 `PermissionAuthorizationHandler`

The handler:

1. Obtains the current authenticated user's ID through `ICurrentUser`.
2. Evaluates the required permission using `IPermissionService.HasPermissionAsync`.
3. Succeeds the authorization requirement only when the permission is present.

If the user is unauthenticated, has no usable identity, or lacks the permission, the policy must not grant access.

### 6.5 API Authorization Convention

The policy provider supports fine-grained permission authorization at the API boundary. However, the documented convention is:

* **Controllers:** primarily use role-based authorization.
* **Application commands and queries:** use `[AuthorizePermission]` and the MediatR pipeline for fine-grained permission enforcement.

Example API role gate:

```csharp
[Authorize(
    Roles = $"{SystemRoles.CatalogManager},{SystemRoles.Administrator}")]
public sealed class BrandsController : BaseApiController
{
    // Endpoints
}
```

Example permission policy:

```csharp
[Authorize(Policy = "Permission:Accounts.Read")]
```

These are separate enforcement mechanisms. The dynamic policy provider remains available where an API endpoint explicitly needs permission-based protection.

---

## 7. Database Model

### 7.1 Tables

| Table             | Schema     | Responsibility                                 |
| ----------------- | ---------- | ---------------------------------------------- |
| `AspNetUsers`     | `dbo`      | User accounts managed by ASP.NET Core Identity |
| `AspNetRoles`     | `dbo`      | Identity role records                          |
| `AspNetUserRoles` | `dbo`      | User-role assignments                          |
| `Permissions`     | `security` | Custom permission records                      |
| `RolePermissions` | `security` | Role-permission mappings                       |

### 7.2 Relationships and Constraints

The model defines the following relationships:

* `Permissions.Id` → `RolePermissions.PermissionId`.
* `AspNetRoles.Id` → `RolePermissions.RoleId`.

The intended delete behavior is cascade deletion from the referenced permission or role to its corresponding role-permission mappings.

`RolePermission` uses a composite primary key:

```text
(RoleId, PermissionId)
```

This enforces uniqueness for each role-permission pair.

The role relationship may be configured without a navigation property to the Identity role entity. A database foreign key can exist independently of an EF Core navigation property.

### 7.3 Indexes

* A unique index on `Permissions.Name`.
* A composite primary key on `RolePermissions(RoleId, PermissionId)`.

The composite key supports uniqueness and queries beginning with `RoleId`. If permission-centric queries need efficient reverse lookups, consider an additional index beginning with `PermissionId`, based on actual query patterns.

### 7.4 Persistence Ownership

* ASP.NET Core Identity owns user and role persistence.
* Authorization owns custom permission and role-permission persistence.
* The `RolePermission` entity references Identity role IDs without introducing a separate domain role entity.

---

## 8. Startup Data Seeding

Startup reconciliation discovers the permission and role declarations and adds missing records to the database.

### 8.1 Seeder Responsibilities

| Seeder               | Responsibility                                                                                                           |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `PermissionSeed`     | Discovers `IPermissionSource` declarations and inserts missing permission records.                                       |
| `RoleSeed`           | Discovers `IRole` declarations and inserts missing Identity roles.                                                       |
| `RolePermissionSeed` | Creates the mappings declared by each role and expands wildcard permissions against the current declared permission set. |
| `IdentitySeed`       | Optionally creates the seed administrator when `DataSeeding:CreateSeedAdmin` is enabled.                                 |

### 8.2 Reconciliation Model

The seeding pipeline follows these rules:

* **Additive-only:** Missing declared records are inserted; obsolete database records are not deleted.
* **Idempotent:** Repeated runs against unchanged declarations do not add duplicate records.
* **Fail-fast validation:** Declaration errors are detected before any seeder performs database writes.
* **Per-seeder transactions:** Each seeder owns its own transaction where transactional persistence is applicable.
* **Startup blocking:** The host does not accept requests until required reconciliation completes successfully.
* **Observable:** Each seeder reports its reconciliation outcome through structured logs.

Orphaned database records are logged but not deleted.

### 8.3 Required Execution Order

```text
Database migrations
        ↓
Discover declarations
        ↓
Validate declarations
        ↓
PermissionSeed
        ↓
RoleSeed
        ↓
RolePermissionSeed
        ↓
Optional IdentitySeed
        ↓
Startup completes
        ↓
HTTP requests accepted
```

Validation must check:

* Duplicate permission names across sources.
* Duplicate role names across role declarations.
* References to permissions that have not been declared.
* Duplicate permission entries within an individual role declaration.

Wildcard declarations must be expanded against the validated permission set before role-permission mappings are reconciled.

### 8.4 Multi-Instance Startup

The application may have multiple instances starting against the same database.

The intended concurrency policy is:

* `RolePermissionSeed` may handle SQL Server duplicate-key errors 2601 and 2627 when they represent a known concurrent insertion of the same mapping.
* The losing transaction must be rolled back before further operations are attempted.
* The implementation should verify that the expected mapping now exists before treating the race as successfully resolved.
* Unrelated database errors must propagate.
* `PermissionSeed`, `RoleSeed`, and `IdentitySeed` do not silently swallow unique-constraint failures. An affected startup may fail and recover on a subsequent restart.

Unique constraints remain the final database-level protection against duplicate records.

### 8.5 Wildcard Semantics

`IRole.AllPermissions` is a declaration-time wildcard, not a permission record.

When a role declares `"*"`, `RolePermissionSeed` expands it into the set of permissions currently declared in code.

For example, if `AdministratorRole` uses the wildcard and `CatalogPermissions.Publish` is introduced, the next successful reconciliation grants the new permission to the administrator role.

The wildcard does not include orphaned database permission records that are absent from current declarations.

Because wildcard roles automatically receive future permissions, their use must be limited to roles that are intentionally granted broad administrative access.

### 8.6 Seed Administrator

Seed administrator creation is optional and disabled by default.

When enabled:

* The seeder uses `UserManager<User>`.
* Required configuration is validated before attempting account creation.
* The password is provided through user secrets, environment variables, or a secret manager.
* The password is never committed to source control, logged, or included in exception messages.
* Existing administrator accounts are not modified by the seeder.

The bootstrap mechanism must be controlled carefully in production and disabled after initial provisioning where applicable.

---

## 9. Workflows

### 9.1 Adding a New Permission

1. Add a `public const string` field to the appropriate `*Permissions` class.
2. Restart the application or run the configured reconciliation process.
3. The declaration-discovery mechanism finds the new constant.
4. `PermissionSeed` inserts the missing permission record.
5. If a role should receive the permission, add it to that role's `GetPermissions()` result. A wildcard role receives it automatically.
6. `RolePermissionSeed` creates any missing mapping.
7. Use the permission constant in the relevant `[AuthorizePermission(...)]` declaration.

No policy-provider or authorization-handler modification is required.

### 9.2 Adding a New Role

1. Add a role-name constant to `SystemRoles`.
2. Create a sealed class implementing `IRole`.
3. Return the corresponding `SystemRoles` constant from `Name`.
4. Declare the required permission set in `GetPermissions()`.
5. Restart the application or run the configured reconciliation process.
6. Verify that the Identity role and its role-permission mappings have been created.

No manual discovery-list update is required, provided the new class is located in an assembly included by `AddAuthorizationSources()`.

### 9.3 Assigning a Permission to a Role at Runtime

An administrator initiates the `AssignPermissionToRoleCommand`.

The command handler calls the relevant management service, which:

1. Validates that the role and permission exist.
2. Checks whether the mapping already exists.
3. Creates the mapping if it is missing.
4. Persists the change through the configured persistence abstraction and unit of work.

The database's composite primary key prevents duplicate mappings.

**Important:** Runtime assignments are additive relative to code declarations. Since the seeders do not remove mappings, a manually granted mapping persists across restarts unless explicitly removed through an administrative operation or migration.

### 9.4 Enforcing Authorization on an Application Use Case

Example:

```csharp
[AuthorizePermission(AccountPermissions.Lock)]
public sealed record LockAccountCommand(Guid UserId)
    : IRequest<Result>;
```

The `PermissionAuthorizationBehavior` reads the required permission, checks the current user's effective permissions through `IPermissionService`, and rejects the request when authorization fails.

For role-based API enforcement:

```csharp
[Authorize(
    Roles = $"{SystemRoles.CatalogManager},{SystemRoles.Administrator}")]
public sealed class BrandsController : BaseApiController
{
    // Endpoints
}
```

The documented convention is to use roles at the API boundary and permissions on Application requests. Both layers must remain consistent with the intended access rules.

---

## 10. Security Considerations

### 10.1 Role-Based Grants

Permissions are granted through roles. There are no direct user-permission assignments.

### 10.2 Administrative Operations

Administrative authorization-management operations should require the intended combination of role and permission checks.

For example:

```csharp
[AuthorizeRole(SystemRoles.Administrator)]
[AuthorizePermission(AuthorizationPermissions.Manage)]
```

When both attributes are enforced by their respective pipeline behaviors, the request must satisfy both requirements.

This combination may be relaxed only when the intended security model explicitly permits permission-based administration without the administrator role.

### 10.3 Stable Permission Identifiers

Permission references should use centralized constants from the appropriate `*Permissions` class. Avoid raw permission strings at call sites.

### 10.4 Database Constraints

The database enforces uniqueness for:

* `Permissions.Name`.
* The `(RoleId, PermissionId)` pair in `RolePermissions`.

### 10.5 Failure Handling

Authorization failures must not result in protected operations being executed.

The API's exception-handling middleware must map authentication and authorization failures consistently. A missing permission for an authenticated user should normally produce HTTP 403 Forbidden.

### 10.6 Sensitive Data

Only appropriately authorized administrators may list or modify roles, permissions, and mappings.

The exact policy must be enforced by the administrative handlers and endpoints rather than relying only on UI restrictions.

### 10.7 Wildcard Privileges

`AllPermissions` grants every permission declared in code at reconciliation time. Adding a new permission therefore expands the administrator's effective permission set automatically.

This is intentional for a broad administrative role but must not be used for ordinary operational roles.

---

## 11. Testing and Verification

The feature should be verified at the unit, integration, and end-to-end levels.

### 11.1 Unit Tests

Test:

* `PermissionService` permission evaluation.
* `UserRoleService` role membership evaluation.
* Permission and role management service behavior.
* Missing-role and missing-permission behavior in MediatR pipeline behaviors.
* The AND semantics of multiple permission attributes.
* Wildcard expansion.
* Duplicate declaration detection and unknown-permission validation.

### 11.2 Integration Tests

Test:

* Permission and role persistence.
* Unique indexes and composite-key constraints.
* Foreign-key relationships and configured cascade behavior.
* Dynamic policy construction and caching.
* Permission authorization-handler evaluation.
* Application-level authorization enforcement.
* Startup reconciliation against an actual test database.

### 11.3 Discovery Tests

Verify that `AddAuthorizationSources()` registers every concrete `IPermissionSource` and `IRole` implementation from the intended assemblies.

Also verify that a newly added declaration is discovered without updating a separate list.

### 11.4 Seeder Tests

Verify that:

1. Missing permission records are inserted.
2. Missing role records are inserted.
3. Missing role-permission mappings are inserted.
4. Wildcard roles receive newly declared permissions.
5. Repeated reconciliation is idempotent.
6. Orphaned records are not deleted.
7. Invalid declarations fail before any database writes.
8. Concurrent mapping insertion is handled only for the expected duplicate-key condition.
9. Other database errors propagate.
10. Seed administrator creation is disabled by default and does not modify an existing account.

### 11.5 End-to-End Tests

Verify:

* Administrative endpoints reject unauthorized users.
* Users without the required role cannot access role-protected endpoints.
* Commands and queries reject users lacking required permissions.
* Authentication and authorization failures map to the intended HTTP responses.

---

## 12. Operational Expectations

Before deploying the feature, verify the following:

1. `AppDbContext` inherits from `IdentityDbContext<User, IdentityRole<Guid>, Guid>`.
2. `AppDbContext.Security.cs` exposes `DbSet<Permission>` and `DbSet<RolePermission>`.
3. EF Core migrations create `security.Permissions` and `security.RolePermissions` with the intended keys and constraints.
4. `AddAuthorizationSources()` is called from `ApplicationServiceRegistration.AddApplication()`.
5. All declaration assemblies are included in the Scrutor scan.
6. Startup seeders are registered and run after migrations and before the host accepts requests.
7. `PermissionSeed`, `RoleSeed`, and `RolePermissionSeed` run in the required dependency order.
8. `IdentitySeed` runs only when seed administrator creation is enabled.
9. The MediatR authorization behaviors are registered and their actual runtime order is verified.
10. `AuthorizationPolicyProvider` and `PermissionAuthorizationHandler` are registered in dependency injection.
11. Administrative endpoints use the intended role and/or permission requirements.
12. `DataSeedingOptions` validates the required configuration.
13. Secrets and passwords are provided through suitable secret-management mechanisms rather than committed configuration files.
14. Logging records seeding outcomes without exposing credentials or other sensitive information.

**Critical startup requirement:** The application must not accept HTTP requests until required authorization reconciliation has completed successfully. Registration of an `IHostedService` alone should not be assumed to guarantee every aspect of the intended startup order; verify the actual host initialization arrangement.

---

## 13. Summary

The Authorization feature provides role- and permission-based access control integrated with ASP.NET Core Identity, EF Core, and MediatR.

Its key design properties are:

* **Roles belong to Identity.** System roles are declared through `IRole` implementations and persisted using the built-in Identity role store.
* **Permissions are custom records.** Permission names are centralized in `IPermissionSource` implementations and persisted in the `security` schema.
* **Role-permission assignments define effective grants.** Users receive permissions through their assigned roles.
* **Discovery is convention-based.** Scrutor discovers declaration classes without a manually maintained registration list.
* **Reconciliation is additive-only.** Missing records and mappings are inserted, while obsolete records remain until explicitly migrated or removed.
* **Wildcard roles receive declared permissions.** The wildcard expands against the current permission declarations at each successful reconciliation.
* **API and Application enforcement have distinct responsibilities.** API endpoints primarily use role gates, while Application requests use permission attributes and MediatR pipeline behaviors.
* **The Domain layer remains unchanged.** Authorization is implemented through Application abstractions and Infrastructure services.
* **Startup establishes required authorization data.** Invalid declarations or unhandled seeding failures prevent successful application startup.

The result is a centralized authorization subsystem designed for the E-Commerce modular monolith without introducing a separate authorization service or unnecessary distributed infrastructure.
