# Authorization — Integration Guide

**Project:** E-Commerce Modular Monolith
**Scope:** Permission and Role Declarations, Enforcement, Resource-Based Authorization
**Version:** 2.2 — Final

---

## 1. Purpose

This guide explains how bounded contexts, application modules, and shared capabilities integrate with the Authorization feature.

It covers:

* Declaring permissions and roles.
* Applying authorization attributes to commands and queries.
* Enforcing authorization at the API boundary.
* Implementing resource-based authorization.
* Extending existing authorization declarations.
* Troubleshooting and testing authorization behavior.

The guide is intended for developers introducing authorization into a new bounded context, module, or shared capability, extending existing permissions, or introducing a new system role.

The integration pattern is consistent throughout the modular monolith. Once the conventions are understood, authorization can be extended without introducing per-feature registration or manually maintained permission mappings.

## 2. Who Can Integrate?

Authorization is a **cross-cutting capability**, not a business bounded context. Any part of the application that exposes protected operations can integrate with it.

### 2.1 Bounded Contexts

Bounded contexts are the primary consumers of authorization. Each context declares permissions representing the operations it exposes and applies them to the relevant commands and queries.

Examples include:

* Catalog
* Ordering
* Shipping
* Finance
* Customer Engagement
* People

For example, Ordering might declare permissions for reading orders, placing orders, cancelling orders, and managing deliveries.

### 2.2 Application Modules

Application modules can declare and consume permissions using the same conventions as bounded contexts.

Examples include:

* **Authorization:** Managing roles and permission assignments.
* **Authentication:** Protecting administrative authentication operations, where applicable.
* **Identity:** Managing account activation, deactivation, locking, and unlocking.
* **Notifications:** Managing notification administration or device registrations.
* **Scheduling:** Protecting administrative job-management operations, where applicable.

A module does not automatically require its own permissions. Declarations should be introduced only when the module exposes operations that require authorization.

### 2.3 Shared Capabilities

Shared capabilities under `Application/Shared/` can also declare permissions when their operations require access control.

Examples include:

* **File Storage:** Reading, uploading, deleting, or managing files.
* **ReadModel or Profiles:** Protecting queries or profile-management operations beyond ordinary authentication requirements.

### 2.4 Centralization Rule

All permission-source declarations reside in:

`Application/Shared/Security/Authorization/Permissions/`

All role declarations reside in:

`Application/Shared/Security/Authorization/Roles/`

Role-name constants reside in the centralized `SystemRoles` declaration.

This convention centralizes authorization identities while keeping ownership visible through class names and permission prefixes.

The physical location of a permission declaration does not change which bounded context or module owns the underlying business operation.

---

## 3. Integration Overview

Integrating with Authorization involves three activities.

### 3.1 Declaration

Declare permissions through classes implementing `IPermissionSource` and roles through classes implementing `IRole`.

The Authorization module discovers these declarations through its configured discovery and registration mechanism.

### 3.2 Application

Apply `[AuthorizePermission]` and, where appropriate, `[AuthorizeRole]` to commands and queries.

The corresponding MediatR pipeline behaviors enforce the declared requirements before the request reaches its handler.

### 3.3 Consumption

Use `IPermissionService` and `IUserRoleService` when application logic requires an explicit permission or role check.

This is particularly relevant to resource-based authorization, where a user may perform an operation on their own resource but require additional permission to access another user's resource.

### 3.4 Integration Flow

```text
Bounded Contexts       Application Modules       Shared Capabilities
       |                       |                         |
       +-----------------------+-------------------------+
                               |
                               v
                   Centralized Declarations
                               |
                 +-------------+-------------+
                 |                           |
                 v                           v
          IPermissionSource                 IRole
                 |                           |
                 +-------------+-------------+
                               |
                               v
                     Authorization Discovery
                               |
                               v
                     Permission/Role Setup
                               |
                               v
                     Runtime Authorization
                               |
               +---------------+---------------+
               |               |               |
               v               v               v
         MediatR Pipeline   API Policies   Resource Checks
         Behaviors          and Roles      in Handlers
```

The diagram describes the conceptual integration flow. The actual discovery, permission persistence, role reconciliation, and runtime enforcement mechanisms are implemented by the Authorization module.

---

## 4. Integration Contracts

### 4.1 Contracts Implemented by Integrators

| Contract            | Location                                               | Purpose                                         |
| ------------------- | ------------------------------------------------------ | ----------------------------------------------- |
| `IPermissionSource` | `Application/Shared/Security/Authorization/Contracts/` | Identifies a source of permission declarations. |
| `IRole`             | `Application/Shared/Security/Authorization/Contracts/` | Declares a system role and its permission set.  |

### 4.2 Services Consumed by Integrators

| Service              | Layer       | Purpose                                     |
| -------------------- | ----------- | ------------------------------------------- |
| `IPermissionService` | Application | Checks whether a user has a permission.     |
| `IUserRoleService`   | Application | Checks a user's role membership.            |
| `ICurrentUser`       | Application | Resolves the authenticated user's identity. |

The exact method signatures must match the contracts implemented by the application. For example, permission checks may use `HasPermissionAsync(userId, permission)`.

### 4.3 Authorization Attributes

| Attribute or Mechanism                   | Applied To                   | Enforcement                                                  |
| ---------------------------------------- | ---------------------------- | ------------------------------------------------------------ |
| `[AuthorizePermission(...)]`             | MediatR commands and queries | `PermissionAuthorizationBehavior`                            |
| `[AuthorizeRole(...)]`                   | MediatR commands and queries | `RoleAuthorizationBehavior`                                  |
| `[Authorize(Roles = "...")]`             | API controllers or actions   | ASP.NET Core authorization                                   |
| `[Authorize(Policy = "Permission:...")]` | API controllers or actions   | Dynamic policy provider and permission authorization handler |

Application-layer and API-layer enforcement are complementary. API authorization can reject a request before the application pipeline runs, while application authorization protects the use case independently of a particular HTTP endpoint.

---

## 5. Declaring Permissions

### 5.1 Declaration Pattern

Create one permission-source class per declaring concern in:

`Application/Shared/Security/Authorization/Permissions/`

#### Bounded Context Example

```csharp
public sealed class OrderPermissions : IPermissionSource
{
    public const string Read = "Orders.Read";
    public const string Place = "Orders.Place";
    public const string Cancel = "Orders.Cancel";
    public const string Deliver = "Orders.Deliver";
}
```

#### Module Example

```csharp
public sealed class AuthorizationPermissions : IPermissionSource
{
    public const string ReadRoles = "Authorization.ReadRoles";
    public const string ManageRoles = "Authorization.ManageRoles";
    public const string ReadPermissions = "Authorization.ReadPermissions";
    public const string ManagePermissions = "Authorization.ManagePermissions";
}
```

#### Shared Capability Example

```csharp
public sealed class FilePermissions : IPermissionSource
{
    public const string Read = "Files.Read";
    public const string Upload = "Files.Upload";
    public const string Delete = "Files.Delete";
    public const string Manage = "Files.Manage";
}
```

### 5.2 Declaration Rules

* **Class naming:** Use `<Concern>Permissions`.
* **Class modifier:** Use a `sealed` class implementing `IPermissionSource`.
* **Permission fields:** Declare permission identities as `public const string` fields.
* **Naming convention:** Use a stable `<Concern>.<Action>` format, such as `Orders.Read` or `Files.Upload`.
* **Description implementation:** If `IPermissionSource` supplies a default `Describe()` implementation that reflects over the class's own constant string fields, do not override it unnecessarily.
* **Location:** Place permission sources in the centralized `Permissions/` directory.
* **Discovery:** Ensure the class is included in the assemblies and types scanned by the configured discovery mechanism.

The default `Describe()` behavior is an implementation-specific convention, not a feature guaranteed by C# interfaces. It must be verified against the actual contract.

### 5.3 Benefits

Following this convention provides:

* Compile-time references to permission identities in attributes and role declarations.
* Centralized permission discovery.
* Consistent permission naming.
* No separate manual registration for each permission, provided automatic discovery is correctly configured.

### 5.4 When to Add a Permission

Add a permission when an operation requires a distinct authorization boundary.

Consider the intended audience and business meaning of the operation. If multiple operations represent the same access capability, they may share a permission. If their access requirements differ, separate permissions provide more flexibility.

Permissions should be sufficiently granular to support the required authorization policies without creating unnecessary complexity.

**Important:** Permission identities are persisted and may be referenced by role assignments. Renaming a permission can therefore require a database migration or an explicit data-reconciliation procedure.

---

## 6. Declaring Roles

### 6.1 Centralized Role Names

Role-name constants reside in `SystemRoles`.

```csharp
public static class SystemRoles
{
    public const string Administrator = "Administrator";
    public const string Support = "Support";
    public const string Employee = "Employee";
    public const string Customer = "Customer";
    public const string CatalogManager = "CatalogManager";
    public const string Dispatcher = "Dispatcher";
    public const string Driver = "Driver";
    public const string Packaging = "Packaging";
}
```

Use these constants instead of repeating literal role names in attributes, role declarations, or application code.

Role names are system-wide identifiers. Their permission sets may span multiple bounded contexts.

### 6.2 Role Declaration Pattern

Create one class per role in:

`Application/Shared/Security/Authorization/Roles/`

Example:

```csharp
public sealed class CatalogManagerRole : IRole
{
    public string Name => SystemRoles.CatalogManager;

    public IEnumerable<string> GetPermissions() =>
    [
        CatalogPermissions.ViewProducts,
        CatalogPermissions.ManageProducts,
        CatalogPermissions.ManageBrands,
        CatalogPermissions.ManageCategories
    ];
}
```

This example assumes the permission constants exist and that the project uses a C# version supporting collection expressions.

### 6.3 Role Rules

* **Class naming:** Use `<RoleName>Role`.
* **Interface:** Implement `IRole`.
* **Role name:** Reference a `SystemRoles` constant.
* **Permission set:** Return the complete permission set granted to that role.
* **Role composition:** Roles do not inherit permissions from other roles under this convention.
* **Location:** Place role declarations in the centralized `Roles/` directory.
* **Discovery:** Ensure the role class is included in the configured discovery mechanism.

### 6.4 Wildcard Permissions

If `IRole.AllPermissions` represents a wildcard in the implemented Authorization module, a role may return it to request every currently declared permission.

Example:

```csharp
public sealed class AdministratorRole : IRole
{
    public string Name => SystemRoles.Administrator;

    public IEnumerable<string> GetPermissions() =>
        [IRole.AllPermissions];
}
```

Use this only when the role is intentionally allowed to receive every permission.

The Authorization module must explicitly implement wildcard expansion during permission reconciliation or seeding. The constant itself does not automatically grant access.

If wildcard expansion runs during every seed or reconciliation operation, newly discovered permissions may also be granted to the role. This behavior must be deliberate and tested.

### 6.5 When to Add a Role

Add a role when a distinct audience requires a distinct set of capabilities.

Avoid introducing a new role merely because one operation differs slightly from an existing role. Consider whether the difference should be represented by an additional permission assigned to an existing role.

Role definitions should reflect actual access-management requirements rather than mirror the organizational structure of every bounded context.

---

## 7. Applying Authorization to Use Cases

### 7.1 Permission-Based Authorization

Apply `[AuthorizePermission]` to a command or query that requires a particular permission.

```csharp
using E_Commerce.Application.Shared.Security.Authorization.Attributes;

[AuthorizePermission(OrderPermissions.Place)]
public sealed record PlaceOrderCommand(/* parameters */)
    : IRequest<Result<Guid>>;
```

The `PermissionAuthorizationBehavior` evaluates the declared requirement before invoking the handler.

The handler does not need to repeat the same operation-level permission check unless the use case has an additional, distinct authorization requirement.

### 7.2 Role-Based Authorization

Use `[AuthorizeRole]` when the role itself is the intended access requirement.

```csharp
[AuthorizeRole(SystemRoles.Administrator)]
public sealed record RecalculateOrderTotalCommand(/* parameters */)
    : IRequest<Result>;
```

The `RoleAuthorizationBehavior` enforces the role requirement.

Prefer permission-based authorization for most business operations because it separates capabilities from role membership. Use direct role requirements when the operation is genuinely restricted to a specific role.

### 7.3 Combining Role and Permission Requirements

```csharp
[AuthorizeRole(SystemRoles.Support)]
[AuthorizePermission(OrderPermissions.Cancel)]
public sealed record CancelOrderOnBehalfCommand(/* parameters */)
    : IRequest<Result>;
```

Under the described independent pipeline behaviors, both requirements must succeed:

* The user must have the required role.
* The user must have the required permission.

This is **AND** behavior, not OR behavior.

The exact behavior depends on the implementation of the attributes and pipeline behaviors. It should be verified through application tests.

### 7.4 Multiple Permissions

```csharp
[AuthorizePermission(OrderPermissions.Read)]
[AuthorizePermission(FinancePermissions.Read)]
public sealed record GetOrderWithPaymentQuery(/* parameters */)
    : IRequest<Result<OrderWithPaymentDto>>;
```

If the pipeline evaluates each attribute independently and rejects any failed requirement, multiple permission attributes combine using **AND** semantics.

If a use case requires either permission, do not assume multiple attributes provide OR behavior.

Instead, use an explicit authorization mechanism that evaluates the alternatives, or define a common permission if that accurately represents the intended access capability.

### 7.5 Handler Responsibilities

For attribute-declared operation-level authorization, the handler normally does not need to:

* Repeat the permission check performed by the pipeline.
* Repeat the role check performed by the pipeline.
* Resolve `ICurrentUser` solely to repeat those checks.

However, the handler may still need `ICurrentUser` for resource ownership, tenant boundaries, or other use-case-specific authorization rules.

Authorization attributes protect the request only if the relevant MediatR pipeline behaviors are registered and executed for that request.

---

## 8. Applying Authorization at the API Layer

API-level authorization is an additional enforcement boundary. It is useful for rejecting unauthorized HTTP requests before the application use case is invoked.

Application-layer authorization remains necessary for protected commands and queries because application use cases may be invoked through different entry points.

### 8.1 Role-Based API Authorization

```csharp
[Authorize(Roles = SystemRoles.Administrator)]
public sealed class CatalogAdminController : BaseApiController
{
}
```

Use this when the HTTP endpoint itself requires membership in a specific role.

### 8.2 Permission-Based API Authorization

```csharp
[Authorize(Policy = "Permission:Orders.Read")]
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetOrder(
    Guid id,
    CancellationToken cancellationToken)
{
    // Dispatch the query.
}
```

The dynamic policy provider resolves policies using the configured `Permission:{Name}` convention.

The permission authorization handler evaluates the authenticated user's permission.

The policy provider, handler, and authentication services must all be registered correctly. The policy string must match the configured naming convention exactly.

### 8.3 When to Add API Gates

Add API-level authorization when:

* The HTTP endpoint should reject unauthorized callers before application execution.
* The API needs an explicit authorization boundary for an exposed endpoint.
* API-level telemetry should distinguish HTTP authorization failures from application-level authorization failures.

Do not rely exclusively on an API attribute to protect a use case that must remain authorized when invoked through other application entry points.

For ordinary protected commands and queries, apply the appropriate application-layer authorization attribute as well.

### 8.4 Authentication Is Still Required

Authorization determines whether an authenticated identity has sufficient access.

The API and application pipelines must correctly handle unauthenticated requests. A missing or invalid user identity must not be treated as an authorized identity merely because a permission check cannot resolve the user.

---

## 9. Resource-Based Authorization

Permission-based authorization answers:

> Can this user perform `Orders.Read`?

Resource-based authorization answers:

> Can this user read this particular order?

These checks are complementary.

### 9.1 Implementation Pattern

Perform resource-specific checks in the handler, where the use case has access to the resource and its ownership information.

```csharp
public async Task<Result<OrderDto>> Handle(
    GetOrderByIdQuery query,
    CancellationToken cancellationToken)
{
    var order = await _orderRepository.GetByIdAsync(
        query.OrderId,
        cancellationToken);

    if (order is null)
    {
        return Result<OrderDto>.Failure("Order not found.");
    }

    var currentUserId = _currentUser.UserId;

    if (currentUserId is null)
    {
        return Result<OrderDto>.Failure("Forbidden.");
    }

    if (order.UserId != currentUserId.Value)
    {
        var canReadAnyOrder =
            await _permissionService.HasPermissionAsync(
                currentUserId.Value,
                OrderPermissions.Manage);

        if (!canReadAnyOrder)
        {
            return Result<OrderDto>.Failure("Forbidden.");
        }
    }

    return Result<OrderDto>.Success(order.ToDto());
}
```

This example assumes the relevant repository, user identifier types, permission constants, and `Result<T>` API match the actual application.

The `OrderPermissions.Manage` permission is illustrative. Use a permission that explicitly represents access to other users' orders rather than reusing a broad management permission without considering its meaning.

### 9.2 Resource Authorization Rules

* **Location:** Perform resource-specific checks in the handler or a dedicated application authorization service.
* **Ownership:** Compare the resource owner with the authenticated user.
* **Elevated access:** Use `IPermissionService` to check the relevant permission.
* **No hardcoded administrator shortcut:** Do not use an `IsAdmin` flag or compare role-name strings directly in business handlers.
* **Additional boundaries:** Enforce tenant, organization, or other ownership boundaries where applicable.
* **Minimal scope:** Introduce resource checks only where the operation requires them.

Resource ownership and business-specific access rules are application concerns. They should not be confused with a general permission declaration.

### 9.3 When Resource Checks Are Needed

Typical cases include:

* Reading, updating, or deleting a user's order.
* Editing a personal profile.
* Modifying a user's review or wishlist.
* Allowing support personnel to access another user's resource.
* Performing operations where a general permission is necessary but insufficient.

A general permission should not automatically imply access to every resource governed by that permission.

---

## 10. Integration Rules

### 10.1 Mandatory Rules

1. Declare permission identities as `public const string` fields on a sealed class implementing `IPermissionSource`.
2. Declare roles as classes implementing `IRole`.
3. Reference role names through `SystemRoles` constants.
4. Keep permission sources in `Application/Shared/Security/Authorization/Permissions/`.
5. Keep role declarations in `Application/Shared/Security/Authorization/Roles/`.
6. Use attribute-declared application authorization as the primary use-case enforcement mechanism.
7. Use API-level authorization as an additional HTTP boundary where appropriate.
8. Use `IPermissionService` for elevated resource access instead of hardcoded role checks.
9. Do not assign permissions directly to users if the system's authorization model grants permissions exclusively through roles.
10. Do not override the default `Describe()` implementation unnecessarily.
11. Ensure the configured discovery mechanism includes newly introduced permission sources and roles.
12. Test the runtime enforcement behavior rather than assuming that declarations alone enforce access.

### 10.2 Recommended Practices

* Prefer `[AuthorizePermission]` over `[AuthorizeRole]` unless the role itself is the intended security requirement.
* Keep persisted permission identifiers stable.
* Prefer meaningful, sufficiently granular permissions.
* Restrict wildcard roles to roles intentionally designed to receive all declared permissions.
* Log authorization failures at an appropriate level without exposing sensitive data or producing excessive log volume.
* Keep role declarations explicit and independently understandable.
* Document changes to role permissions that materially affect access.
* Review the consequences of adding a permission to a wildcard role.

---

## 11. Common Integration Scenarios

### 11.1 Adding a New Bounded Context

1. Create `<Context>Permissions.cs` in the centralized `Permissions/` directory.
2. Declare the context's permission constants.
3. Apply `[AuthorizePermission(...)]` to the relevant commands and queries.
4. Add API-level authorization where appropriate.
5. Add the new permissions to the intended role declarations.
6. Verify discovery and permission persistence.
7. Test authorized and unauthorized execution.

### 11.2 Adding a New Application Module

Follow the same pattern used by bounded contexts.

For example, an Identity module may declare `AccountPermissions` for account-management operations.

The module's declarations remain centralized, while the module retains responsibility for the semantics of its operations.

### 11.3 Adding a Shared Capability's Permissions

For a shared capability such as File Storage:

1. Create `FilePermissions.cs` in the centralized `Permissions/` directory.
2. Declare the required permissions.
3. Apply the relevant authorization attributes to protected application requests.
4. Add permissions to appropriate roles.
5. Verify the runtime authorization path.

### 11.4 Adding a Permission to an Existing Source

1. Add a `public const string` field to the existing permission source.
2. Update role declarations that should grant the new permission.
3. Run or verify the configured permission discovery and reconciliation process.
4. Test the intended access behavior.

Adding a constant creates a compile-time permission identifier. It does not, by itself, guarantee that the permission has been persisted or granted to a user.

### 11.5 Adding a Role

1. Add a constant to `SystemRoles`.
2. Create `<RoleName>Role.cs` implementing `IRole`.
3. Reference the corresponding `SystemRoles` constant.
4. Return the role's complete permission set.
5. Verify role discovery and role seeding or reconciliation.
6. Test the resulting access rules.

### 11.6 Assigning a Role to a User

Use the Authorization module's supported application use case, such as:

```text
AssignRoleToUserCommand(userId, roleName)
```

The exact command and signature must match the implemented Application contract.

Where appropriate, Infrastructure may use ASP.NET Core Identity's `UserManager.AddToRoleAsync` behind the Authorization module's abstraction.

Do not bypass application authorization, validation, or role-assignment rules by calling Identity directly from unrelated handlers.

**Effective access changes depend on the application's permission-checking model.** If permissions are resolved from persisted role assignments on every check, changes may become effective immediately. If permissions are cached or materialized, invalidation or reconciliation may be required.

### 11.7 Adding a Resource Authorization Check

1. Load the resource.
2. Return the appropriate not-found result if it does not exist.
3. Compare its owner with `ICurrentUser.UserId`.
4. If the user is not the owner, call `IPermissionService` to check the relevant elevated permission.
5. Reject the operation if neither ownership nor elevated access authorizes it.
6. Test both owned-resource and foreign-resource cases.

Do not substitute an operation-level authorization attribute for a resource-specific ownership check.

### 11.8 Adding a Wildcard Role

If the implementation supports `IRole.AllPermissions`, declare the wildcard only for roles intended to receive every discovered permission.

The permission-seeding or reconciliation process must explicitly expand the wildcard, and tests must verify the effect of introducing new permissions.

### 11.9 Securing a New API Endpoint

```csharp
[Authorize(Policy = "Permission:Reports.View")]
[HttpGet("reports/{id:guid}")]
public async Task<IActionResult> GetReport(
    Guid id,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(
        new GetReportQuery(id),
        cancellationToken);

    // Map the application result to an HTTP response.
}
```

The application query should also carry its corresponding application-layer authorization requirement:

```csharp
[AuthorizePermission(ReportsPermissions.View)]
public sealed record GetReportQuery(Guid Id)
    : IRequest<Result<ReportDto>>;
```

This example assumes both the API policy mechanism and MediatR authorization behavior are configured and that their permission semantics are consistent.

---

## 12. Troubleshooting

| Symptom                                             | Likely Cause                                                                                           | Recommended Fix                                                                                            |
| --------------------------------------------------- | ------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------- |
| Permission check always fails                       | Permission is not granted through the user's assigned roles, or the user identity cannot be resolved   | Verify role assignment, permission composition, user identity, and the permission-checking implementation. |
| API permission policy is not found                  | Incorrect policy name or missing dynamic policy provider                                               | Use the exact `Permission:{Name}` convention and verify service registration.                              |
| `ICurrentUser.UserId` is null                       | Request is unauthenticated, the identity claim is missing, or claim mapping is incorrect               | Verify authentication configuration, the principal, and the current-user implementation.                   |
| Role check fails unexpectedly                       | Role-name mismatch or missing role assignment                                                          | Compare the stored role name with the exact `SystemRoles` constant and inspect role assignment.            |
| Multiple permission attributes behave unexpectedly  | The pipeline may not implement the expected combination semantics                                      | Inspect `PermissionAuthorizationBehavior` and test the intended AND/OR behavior.                           |
| Resource access is incorrectly allowed              | Ownership or elevated-access checks are incomplete                                                     | Verify ownership comparison and require the appropriate permission for access to another user's resource.  |
| Permission source is not discovered                 | The class is not included in the configured scan or does not implement the required contract correctly | Verify its interface, assembly scanning, and registration process.                                         |
| Permission is declared but missing from persistence | Discovery or permission reconciliation has not run successfully                                        | Inspect startup or reconciliation logs and verify the persisted permission record.                         |
| Newly assigned role has no effect                   | Role assignment failed, permission data is stale, or caching is not invalidated                        | Verify persistence, role-permission composition, and cache invalidation where applicable.                  |
| New permission unexpectedly reaches Administrator   | Wildcard expansion grants all discovered permissions                                                   | Verify that the wildcard is intentional and review the administrator role's access policy.                 |

---

## 13. Testing Requirements

### 13.1 Declaration Tests

Verify that:

* Each `IPermissionSource` returns exactly its declared permission constants.
* Each role returns its expected permission set.
* Wildcard expansion follows the intended policy.
* Permission names and role names remain stable and unique.
* Discovery includes every intended declaration.

### 13.2 Application Tests

Verify that:

* A command or query decorated with `[AuthorizePermission]` rejects callers lacking the permission.
* A command or query decorated with `[AuthorizeRole]` rejects callers lacking the required role.
* Combined attributes enforce all required conditions.
* Unauthenticated requests cannot bypass authorization.
* Resource checks reject unauthorized access to another user's resource.
* Owners can access their own resources where the business rules allow it.
* Elevated access requires the intended permission.

### 13.3 API Tests

Verify that:

* API permission policies reject users lacking the required permission.
* Authorized users can access the endpoint.
* Role-protected endpoints enforce the configured role requirement.
* Application-layer authorization remains effective independently of API-level attributes.
* HTTP responses do not expose sensitive authorization internals.

The exact expected status codes should match the application's documented authentication and authorization error contract.

### 13.4 Persistence and Reconciliation Tests

Verify that:

* Discovered permissions are persisted as intended.
* Role declarations are reconciled without unintended permission grants.
* Role assignment updates effective access according to the configured consistency and caching model.
* Renamed or removed permission identifiers are handled through an explicit migration or reconciliation strategy.

---

## 14. Summary

Integrating with Authorization involves three activities.

**1. Declare**

Create an `IPermissionSource` class for each permission concern and an `IRole` class for each role. Use the centralized `Permissions/`, `Roles/`, and `SystemRoles` declarations.

**2. Apply**

Use `[AuthorizePermission]` and, when appropriate, `[AuthorizeRole]` on protected commands and queries. Add API-level authorization when it provides a useful additional boundary.

**3. Consume**

Use `IPermissionService`, `IUserRoleService`, and `ICurrentUser` when application logic requires explicit checks, especially for resource ownership and elevated access.

The conventions are consistent across bounded contexts, application modules, and shared capabilities. Ownership remains with the declaring feature, while declaration discovery and enforcement are provided by the shared Authorization capability.

Adding a permission declaration does not automatically grant it to users. Effective access depends on discovery, persistence or reconciliation, role assignment, and runtime enforcement.

Likewise, declaring an authorization attribute does not provide protection unless the relevant pipeline behavior is registered and executed.

**Document version:** 2.2 — Final
**Feature:** Authorization / Cross-Cutting Integration Guide
