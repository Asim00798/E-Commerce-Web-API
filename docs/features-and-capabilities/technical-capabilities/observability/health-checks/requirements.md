# Health Checks — Technical Requirements Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — Operational Health Checks
**Version:** 1.0 — Final

---

## 1. Introduction

Health checks provide a standardized mechanism for determining whether the application and its operational dependencies are functioning sufficiently to process work.

They answer the question:

**Is the system operational enough to serve requests?**

The capability uses ASP.NET Core's built-in health-check infrastructure. It focuses on critical infrastructure dependencies—SQL Server, Redis, Hangfire storage, and the Outbox—and does not attempt to model application business health.

Health checks are implemented in the **Infrastructure** layer and exposed through the API. The Domain and Application layers remain unaware of the health-check implementation.

## 2. Business Goals

The Health Checks capability aims to:

* Detect when a required operational dependency is unavailable or degraded.
* Provide a simple endpoint that load balancers, orchestration platforms, and operational tools can query.
* Distinguish between healthy, degraded, and unhealthy operational states.
* Monitor the Outbox backlog to help identify processing delays.
* Keep health-check implementation independent of business logic and Domain models.

## 3. Scope

### 3.1 In Scope

The capability includes:

* ASP.NET Core health-check infrastructure.
* SQL Server connectivity checks.
* Redis connectivity checks when Redis is configured as an operational dependency.
* Hangfire storage connectivity checks.
* Outbox message-store accessibility checks.
* Outbox backlog threshold monitoring.
* Centralized registration through a single extension method.
* A `/health` endpoint.
* Health statuses: `Healthy`, `Degraded`, and `Unhealthy`.
* Configuration-driven Redis registration and Outbox backlog thresholds.

### 3.2 Out of Scope

The capability does not include:

* Custom Application-layer health abstractions.
* Separate readiness and liveness probes.
* Alerting based on health status, which belongs to the observability and monitoring platform.
* Health checks for internal business components or Domain logic.
* Health checks for external providers such as Paymob and email/SMS services at this stage.
* UI dashboards for health status.

## 4. Functional Requirements

### 4.1 SQL Server Health Check

The system must verify connectivity to the application's primary database.

Requirements:

* The check must return `Healthy` when a lightweight database operation, such as `SELECT 1` or an equivalent connectivity probe, succeeds within the configured timeout.
* The check must return `Unhealthy` when the database is unreachable or the operation fails.
* The check must not execute expensive queries or modify business data.

### 4.2 Redis Health Check

The system must verify Redis connectivity when Redis is configured as an operational dependency, such as for caching or rate limiting.

Requirements:

* The check must return `Healthy` when a basic command, such as `PING`, succeeds.
* The check must return `Unhealthy` when Redis is required but unreachable or the connectivity probe fails.
* Registration must be configurable so that Redis can be omitted when it is not used by the deployment.
* Redis configuration and connection details must come from application configuration rather than hardcoded values.

If Redis is not required, the check should normally be omitted rather than registered as a permanently healthy check. This keeps the aggregate health status representative of the dependencies actually used by the deployment.

### 4.3 Hangfire Health Check

The system must verify **Hangfire storage availability and connectivity**, not background-job processing status.

Requirements:

* The check must verify that the configured Hangfire storage backend is reachable.
* For SQL Server storage, it should confirm that the necessary storage tables can be accessed, where appropriate.
* The check must return `Healthy` when storage connectivity is confirmed.
* The check must return `Unhealthy` when storage is unavailable or the required storage operations fail.
* The check must not claim that jobs are processing successfully merely because storage is reachable.

The check should use the existing configured database connection string when Hangfire is intentionally configured to use that database. If Hangfire uses a separate storage database, the check must use the actual Hangfire storage configuration.

### 4.4 Outbox Health Check

The system must verify that the Outbox message store is accessible through the application's data-access mechanism.

Requirements:

* The check must execute a lightweight query against the Outbox message store, such as an existence query or a pending-message count.
* The check must return `Healthy` when the Outbox repository or message store can be queried successfully.
* The check must return `Unhealthy` when the required Outbox query fails.
* The check must avoid expensive queries or unnecessary data retrieval.

**Clarification:** SQL Server connectivity and Outbox accessibility represent different operational concerns. A successful database connectivity check does not guarantee that the Outbox table is accessible or correctly configured.

The Outbox health check should therefore focus on access to the Outbox message store rather than duplicate the general database connectivity probe.

This check does not replace the dedicated backlog health check, which evaluates pending-message volume.

### 4.5 Outbox Backlog Health Check

The system must monitor the number of pending Outbox messages and report an appropriate health status based on configurable thresholds.

Requirements:

* The check must retrieve or calculate the pending-message count efficiently.
* Thresholds must be configuration-driven and must not be hardcoded into the health-check implementation.
* Operators must be able to adjust thresholds through application configuration or environment variables without changing application code.
* The check must return `Unhealthy` if the backlog cannot be evaluated because the required query fails.

**Expected status behavior:**

| Condition                                          | Health Status |
| -------------------------------------------------- | ------------- |
| `PendingCount < WarningThreshold`                  | `Healthy`     |
| `WarningThreshold ≤ PendingCount < ErrorThreshold` | `Degraded`    |
| `PendingCount ≥ ErrorThreshold`                    | `Unhealthy`   |
| Backlog query fails                                | `Unhealthy`   |

The implementation must use comparison operators consistent with these boundaries.

The configured thresholds must satisfy:

```text
WarningThreshold >= 0
ErrorThreshold > WarningThreshold
```

The backlog check measures pending-message volume, not message age or processing throughput. Those can be added as separate operational signals if required.

### 4.6 Registration and Endpoint

The system must provide a centralized health-check registration method, such as:

```csharp
services.AddApplicationHealthChecks(configuration);
```

Requirements:

* A single extension method must register the required health checks with dependency injection.
* The registration method must bind and validate health-check configuration.
* Redis registration must be conditional on the configured Redis setting.
* Outbox backlog thresholds must be obtained from configuration.
* The API must expose a `/health` endpoint that returns the aggregate health status.
* The endpoint must be accessible to authorized operational probes according to the deployment's security requirements.

The default endpoint mapping should return:

| Aggregate Health Status |               HTTP Status |
| ----------------------- | ------------------------: |
| `Healthy`               |                  `200 OK` |
| `Degraded`              |                  `200 OK` |
| `Unhealthy`             | `503 Service Unavailable` |

These are the default ASP.NET Core mappings and may be changed through explicit endpoint configuration.

## 5. Non-Functional Requirements

### 5.1 Performance

* Health checks must be lightweight and return within a reasonable time under normal operating conditions.
* Each check must use an appropriate timeout to prevent slow dependency operations from blocking the endpoint indefinitely.
* Database checks must avoid expensive queries.
* Outbox backlog checks must avoid loading large collections of pending messages into memory.
* The aggregate health endpoint should return within a few seconds under normal conditions.

### 5.2 Reliability

* A health-check failure must not, by itself, terminate the application.
* An unavailable dependency must cause its corresponding check to report `Unhealthy`.
* The application should continue serving requests that do not require an unavailable dependency, where possible.
* Health checks must not modify business data or trigger business operations.
* Dependency failures should be handled as health-check results rather than escaping as unhandled exceptions that disrupt normal request processing.

### 5.3 Maintainability

* Health checks must be isolated within Infrastructure.
* Each check must have a clear responsibility and descriptive name.
* Configuration and thresholds must be centralized.
* Adding another check should require a focused implementation and registration change rather than modifications throughout the application.
* The implementation must use ASP.NET Core's existing health-check infrastructure rather than introducing unnecessary custom abstractions.

### 5.4 Configuration

* Outbox backlog thresholds must be configurable.
* Redis enablement and connection details must be configurable.
* Hangfire storage connectivity must use the actual configured Hangfire storage connection.
* Configuration values must be validated at startup or when the options are first evaluated, according to the application's options-validation strategy.
* Invalid threshold configurations must be rejected.

### 5.5 Security

* Health-check responses must not expose connection strings, credentials, personal information, stack traces, or raw internal exception details.
* Responses should contain only the status codes and minimal descriptive information required by monitoring tools.
* Detailed diagnostic information should remain in internal logs or authorized diagnostic systems.
* Access to the endpoint should be restricted where the deployment's security requirements demand it.

## 6. Architectural Constraints

The implementation must comply with the following constraints:

* Health-check implementations reside in Infrastructure.
* The Domain and Application layers must not depend on health-check implementation details.
* The capability must use ASP.NET Core's `IHealthCheck` and `HealthCheckService`.
* No custom Application-layer health abstraction should be introduced.
* Health checks must not replace logging, metrics, tracing, or alerting.
* Health checks must not introduce unnecessary distributed infrastructure or separate services.
* Health-check registration and endpoint mapping must remain explicit and centralized.

## 7. Acceptance Criteria

The capability is considered complete when the following criteria are satisfied:

1. A `/health` endpoint exists and returns `200 OK` when all registered checks report `Healthy`.
2. The SQL Server health check reports `Unhealthy` when the primary database is unreachable.
3. The Redis health check is optional and reports `Unhealthy` when Redis is required but unavailable.
4. The Hangfire health check verifies storage connectivity and reports `Unhealthy` when storage is unavailable.
5. The Outbox health check verifies access to the Outbox message store rather than relying solely on general database connectivity.
6. The Outbox backlog health check reports `Healthy`, `Degraded`, or `Unhealthy` according to the configured thresholds.
7. Outbox backlog thresholds are configurable and are not hardcoded in the check implementation.
8. Health checks are registered through a single extension method.
9. The health-check implementation does not depend on Domain or Application code.
10. Health checks do not interfere with normal business operations.
11. Dependency failures are reported through health-check results without exposing sensitive exception details in HTTP responses.
12. Health-check execution is bounded by appropriate timeouts.

## 8. Summary

The Health Checks capability provides operational visibility into critical infrastructure dependencies and Outbox backlog conditions.

It uses ASP.NET Core's built-in health-check infrastructure to monitor SQL Server, optional Redis connectivity, Hangfire storage, Outbox accessibility, and pending Outbox-message volume.

Configuration-driven thresholds and centralized registration keep the implementation maintainable, while the `/health` endpoint provides a standard interface for monitoring tools and orchestration platforms.

The capability remains isolated within Infrastructure and independent of business logic, preserving the modular monolith's architectural boundaries.
