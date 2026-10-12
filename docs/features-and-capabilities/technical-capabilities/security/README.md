# Security

## Overview

The Security capability organizes the application's authentication, authorization, and identity-related infrastructure.

It establishes how users authenticate, how access to application functionality is controlled, and how authorization state is initialized and reconciled with the application's declarations.

Security follows the modular monolith's Clean Architecture dependency rules: application-level contracts and orchestration remain in the Application layer, while ASP.NET Core Identity, authentication mechanisms, persistence, and infrastructure-specific implementations remain in Infrastructure.

## Scope

The Security capability is organized into the following areas:

| Area               | Responsibility                                                                                                                              | Documentation                                                                                                                                                                               |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Authentication** | Establishes user identity, validates authentication credentials and tokens, and manages authentication workflows.                           | [`authentication/`](authentication/)                                                                                                                                                        |
| **Authorization**  | Defines permission and role declarations, enforces access policies, and provides application-level authorization mechanisms.                | [`authorization/`](authorization/)                                                                                                                                                          |
| **Data Seeding**   | Reconciles authorization declarations with persisted permissions, roles, and role-permission mappings during application startup.           | [`authorization/data-seeding-reference.md`](authorization/data-seeding-reference.md) · [`authorization/data-seeding-integration-guide.md`](authorization/data-seeding-integration-guide.md) |
| **Identity**       | Documents the ASP.NET Core Identity implementation and its integration with the application's authentication and user-management workflows. | [`identity/`](identity/)                                                                                                                                                                    |

Data Seeding is documented under Authorization because its primary responsibility is initializing and reconciling authorization state. It is not a separate business feature or bounded context.

## Architectural Responsibilities

### Authentication

Authentication establishes who the current user is. It covers credential validation, token-based authentication, external authentication where supported, and related authentication workflows.

The authentication module coordinates application behavior without moving ASP.NET Core Identity implementation details into the Domain layer.

### Authorization

Authorization determines whether an authenticated user may perform a particular operation.

The application uses declared roles and permissions to enforce access policies. Where authorization depends on a particular business resource, the owning feature remains responsible for evaluating its business-specific access rules, using the appropriate authorization abstractions when necessary.

### Identity

Identity manages authentication-related user accounts through ASP.NET Core Identity.

`IdentityUser` and its persistence implementation remain in Infrastructure. Identity is not treated as a business bounded context with its own Domain-layer model.

Human and customer information belongs to the appropriate business model, such as People. An authenticated account does not automatically imply that the corresponding person or customer profile has been created or that the user is eligible to perform every business operation.

### Data Seeding

Data Seeding initializes and reconciles persisted authorization state from declarations maintained in code.

It runs as part of the controlled application startup sequence, after database migrations have completed and before the application accepts requests. This ordering must be explicitly enforced by startup orchestration; registering a hosted service alone does not establish migration ordering.

The seeders are designed to be idempotent and additive. Removing a permission declaration or role mapping from code does not, by itself, delete the corresponding database record. Revocations and destructive changes require an explicit migration or reconciliation policy.

## Architectural Boundaries

The following principles govern the Security capability:

* **Keep the Domain layer framework-independent.** Do not introduce ASP.NET Core Identity types or infrastructure-specific authentication dependencies into domain models.
* **Preserve dependency direction.** Application defines the contracts and orchestration it requires; Infrastructure supplies their implementations.
* **Separate authentication from authorization.** Identifying a user and deciding what that user may do are distinct responsibilities.
* **Centralize authorization declarations.** Roles and permissions should have consistent, discoverable declarations rather than scattered string literals.
* **Enforce authorization at the appropriate boundary.** API policies protect HTTP endpoints, application-level policies protect use cases, and resource-specific business rules remain with the feature that owns the resource.
* **Keep infrastructure mechanisms reusable.** Identity persistence, token handling, startup reconciliation, and related implementation details should not become business-domain responsibilities.
* **Treat persisted authorization state as operationally significant.** Changes to permission declarations, role mappings, and account provisioning must account for their existing database state and runtime authorization behavior.

## Documentation Index

### Authentication

* [`authentication/reference.md`](authentication/reference.md) — authentication implementation and technical design.
* [`authentication/requirements.md`](authentication/requirements.md) — authentication requirements.
* [`authentication/workflow.md`](authentication/workflow.md) — authentication workflows and lifecycle behavior.

### Authorization

* [`authorization/integration-guide.md`](authorization/integration-guide.md) — integrating roles, permissions, and authorization policies.
* [`authorization/reference.md`](authorization/reference.md) — authorization implementation and technical design.
* [`authorization/requirements.md`](authorization/requirements.md) — authorization requirements.
* [`authorization/data-seeding-integration-guide.md`](authorization/data-seeding-integration-guide.md) — integrating with the authorization data-seeding mechanism.
* [`authorization/data-seeding-reference.md`](authorization/data-seeding-reference.md) — data-seeding architecture, startup execution, reconciliation, and failure handling.

### Identity

* [`identity/reference.md`](identity/reference.md) — Identity implementation and technical design.
* [`identity/workflow.md`](identity/workflow.md) — Identity-related workflows and lifecycle behavior.

## Maintenance

Keep this documentation aligned with the actual solution structure and established architectural decisions.

When authentication, authorization, or Identity responsibilities change, update the relevant documentation and assess whether the change affects:

* Application and Infrastructure dependency boundaries.
* Permission and role declarations.
* Authentication and authorization policies.
* Startup migration and data-seeding order.
* Existing persisted authorization state.
* Integration with business features.
* Security-related failure handling and operational behavior.

Document significant architectural changes in the Architecture Decision Records when they affect established boundaries or introduce consequential design decisions.
