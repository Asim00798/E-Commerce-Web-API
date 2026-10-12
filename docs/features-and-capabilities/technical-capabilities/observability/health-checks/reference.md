# Health Checks — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — Operational Health Checks
**Version:** 1.0 — Final

---

## 1. Overview

Health checks provide a standardized mechanism for determining whether critical infrastructure dependencies are operational. They answer the question:

**“Is the system sufficiently healthy to handle work?”**

This capability is implemented entirely in the **Infrastructure** layer using ASP.NET Core's built-in health-check infrastructure. It exposes a single `/health` endpoint that orchestration platforms and monitoring tools can query.

The Domain and Application layers remain completely unaware of health-check implementation details.

### Objectives

* Verify connectivity to critical infrastructure dependencies.
* Detect operational issues affecting SQL Server, Redis, and Hangfire storage.
* Verify that the Outbox message store is accessible.
* Monitor the number of pending Outbox messages.
* Provide a consistent aggregate health status for monitoring systems.
* Keep operational health monitoring separate from business logic.

## 2. Architecture

The health-check pipeline is straightforward:

```text
Monitoring Tool / Orchestrator
            |
            v
       GET /health
            |
            v
ASP.NET Core HealthCheckService
            |
            +-- SQL Server Health Check
            |
            +-- Redis Health Check (optional)
            |
            +-- Hangfire Health Check
            |
            +-- Outbox Health Check
            |
            +-- Outbox Backlog Health Check
            |
            v
    Aggregated Health Status
            |
            +-- Healthy   --> HTTP 200
            |
            +-- Degraded  --> HTTP 200
            |
            +-- Unhealthy --> HTTP 503
```

Each check evaluates a specific dependency or operational condition and returns one of three statuses:

| Status      | Meaning                                                                                                                       |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------- |
| `Healthy`   | The dependency or monitored condition is operating normally.                                                                  |
| `Degraded`  | The system remains operational, but a monitored condition indicates elevated risk or pressure, such as a high Outbox backlog. |
| `Unhealthy` | The dependency is unreachable, a required operation fails, or a monitored condition exceeds its configured error threshold.   |

By default, ASP.NET Core maps `Healthy` and `Degraded` to HTTP `200 OK`, and `Unhealthy` to HTTP `503 Service Unavailable`.

These mappings can be customized through health-check endpoint configuration.

## 3. Folder Structure

```text
Infrastructure/
└── Observability/
    └── HealthChecks/
        ├── SqlServerHealthCheck.cs
        ├── RedisHealthCheck.cs
        ├── HangfireHealthCheck.cs
        ├── OutboxHealthCheck.cs
        ├── OutboxBacklogHealthCheck.cs
        ├── HealthChecksOptions.cs
        ├── HealthChecksOptionsValidator.cs
        └── HealthChecksRegistrationExtensions.cs

API/
└── Program.cs
    └── Health-check registration and /health endpoint mapping
```

No Domain or Application files are required for this capability.

The Infrastructure layer owns the dependency probes and their implementation details, while the API layer registers the checks and exposes the endpoint.

## 4. Component Details

### 4.1 SqlServerHealthCheck

**Layer:** Infrastructure

**Purpose:** Verify connectivity to the primary SQL Server database.

**Implementation:** Uses EF Core's `Database.CanConnectAsync()`.

**Expected results:**

| Condition                                 | Health Status |
| ----------------------------------------- | ------------- |
| Database connection succeeds              | `Healthy`     |
| Database connection cannot be established | `Unhealthy`   |
| An exception occurs during the check      | `Unhealthy`   |

This check provides a lightweight indication of database connectivity. It does not verify that every database operation, query, or transaction will succeed.

### 4.2 RedisHealthCheck

**Layer:** Infrastructure

**Purpose:** Verify connectivity to Redis when Redis is an operational dependency.

**Implementation:** Sends a `PING` command through `IConnectionMultiplexer`.

**Expected results:**

| Condition                    | Health Status |
| ---------------------------- | ------------- |
| `PING` succeeds              | `Healthy`     |
| The connectivity probe fails | `Unhealthy`   |

**Registration:** Conditional. The check is registered only when `HealthChecksOptions.RedisEnabled` is `true`.

When Redis is disabled in configuration, the check is omitted from health evaluation.

### 4.3 HangfireHealthCheck

**Layer:** Infrastructure

**Purpose:** Verify connectivity to the database used by Hangfire storage.

**Implementation:** Opens a SQL connection using the configured `DefaultConnection` connection string and executes a lightweight `SELECT 1` query.

**Expected results:**

| Condition                                 | Health Status |
| ----------------------------------------- | ------------- |
| Connection and query succeed              | `Healthy`     |
| The required connection string is missing | `Unhealthy`   |
| The connection or query fails             | `Unhealthy`   |

**Important:** This check verifies storage connectivity only. It does not determine whether background jobs are being processed successfully, whether queues are progressing, or whether recurring jobs are running on schedule.

The implementation uses `DefaultConnection`; therefore, the health check assumes Hangfire storage uses the database identified by that connection string. If Hangfire is configured to use a different database, the health check must use the actual Hangfire storage configuration.

### 4.4 OutboxHealthCheck

**Layer:** Infrastructure

**Purpose:** Verify that the Outbox message store is accessible and queryable.

**Implementation:** Executes a lightweight existence query, such as `AnyAsync()`, against the `OutboxMessage` table.

**Expected results:**

| Condition                              | Health Status |
| -------------------------------------- | ------------- |
| The query succeeds                     | `Healthy`     |
| The query fails or an exception occurs | `Unhealthy`   |

This check is distinct from the general SQL Server health check because it verifies access to the specific Outbox table.

It does **not** measure the number of pending messages or determine whether Outbox processing is keeping up with incoming work. Those concerns belong to `OutboxBacklogHealthCheck`.

### 4.5 OutboxBacklogHealthCheck

**Layer:** Infrastructure

**Purpose:** Monitor the number of pending Outbox messages and report degraded or unhealthy states according to configured thresholds.

**Implementation:** Retrieves pending messages up to the configured error threshold and compares the resulting count against the configured thresholds.

**Expected results:**

| Pending Messages                              | Health Status                                   |
| --------------------------------------------- | ----------------------------------------------- |
| `pending < WarningThreshold`                  | `Healthy`                                       |
| `WarningThreshold ≤ pending < ErrorThreshold` | `Degraded`                                      |
| `pending ≥ ErrorThreshold`                    | `Unhealthy`                                     |
| An exception occurs during the check          | `Unhealthy`, with a generic operational message |

The exact behavior at threshold boundaries depends on the comparison operators used in the implementation. The table above defines the intended behavior.

**Configuration:** Thresholds are provided through `HealthChecksOptions`.

**Operational consideration:** This check measures backlog size, not processing delay. A relatively small backlog can still indicate a serious issue if messages remain unprocessed for too long. Production monitoring may therefore benefit from additional metrics for the age of the oldest pending message and processing throughput.

### 4.6 HealthChecksOptions

**Purpose:** Centralize configuration for the Health Checks capability.

**Properties:**

| Property                 | Type   | Default | Description                                                          |
| ------------------------ | ------ | ------: | -------------------------------------------------------------------- |
| `RedisEnabled`           | `bool` | `false` | Determines whether the Redis health check is registered.             |
| `OutboxWarningThreshold` | `int`  |   `100` | Pending-message count at which the Outbox check reports `Degraded`.  |
| `OutboxErrorThreshold`   | `int`  |   `500` | Pending-message count at which the Outbox check reports `Unhealthy`. |

These defaults apply when the implementation initializes the options with the specified default values and no configuration source overrides them.

### 4.7 HealthChecksOptionsValidator

**Purpose:** Validate health-check configuration before it is used.

**Validation rules:**

* `OutboxWarningThreshold >= 0`
* `OutboxErrorThreshold >= 0`
* `OutboxErrorThreshold > OutboxWarningThreshold`

**Registration:** Registered as an implementation of `IValidateOptions<HealthChecksOptions>`.

The validator prevents invalid threshold configurations from silently changing the intended backlog-monitoring behavior.

### 4.8 HealthChecksRegistrationExtensions

**Purpose:** Provide a centralized registration entry point for the Health Checks capability.

**Behavior:**

1. Binds `HealthChecksOptions` from application configuration.
2. Registers the options validator.
3. Registers `SqlServerHealthCheck`.
4. Registers `HangfireHealthCheck`.
5. Registers `OutboxHealthCheck`.
6. Registers `OutboxBacklogHealthCheck`.
7. Registers `RedisHealthCheck` only when `RedisEnabled` is `true`.

**Usage:**

```csharp
builder.Services.AddApplicationHealthChecks(
    builder.Configuration);
```

The extension method keeps health-check registration centralized and avoids scattering individual check registrations throughout the API startup configuration.

## 5. Data Flow

The health-check execution flow is as follows:

1. A monitoring tool or orchestrator sends `GET /health`.
2. ASP.NET Core's health-check infrastructure invokes the registered health checks.
3. Each check executes its corresponding dependency probe or operational query.
4. The health-check infrastructure aggregates the individual results.
5. The endpoint returns the aggregate status.

Under the default HTTP status mapping:

| Aggregate Health Status | HTTP Response             |
| ----------------------- | ------------------------- |
| `Healthy`               | `200 OK`                  |
| `Degraded`              | `200 OK`                  |
| `Unhealthy`             | `503 Service Unavailable` |

No custom `ResultStatusCodes` mapping is configured unless explicitly added.

**Important:** The default endpoint does not automatically return a detailed JSON report containing every check's result. If monitoring tools need structured health information, configure a suitable `ResponseWriter` and ensure that the response does not expose sensitive operational details.

## 6. Configuration

Example `appsettings.json`:

```json
{
  "HealthChecks": {
    "RedisEnabled": true,
    "OutboxWarningThreshold": 100,
    "OutboxErrorThreshold": 500
  }
}
```

### Configuration Behavior

* When `RedisEnabled` is `true`, the Redis health check is registered.
* When `RedisEnabled` is `false`, the Redis health check is not registered or evaluated.
* `OutboxWarningThreshold` controls when the Outbox backlog becomes degraded.
* `OutboxErrorThreshold` controls when the backlog becomes unhealthy.
* The options validator enforces valid threshold values.

Environment-specific configuration can override these values without changing the application code.

## 7. Security

Health-check endpoints should expose only the operational information needed by their intended consumers.

Recommended controls:

* Return generic operational messages rather than raw exception details.
* Keep exception details available to internal logging or diagnostics without exposing them through the HTTP response.
* Avoid returning connection strings, credentials, personal information, or other sensitive data.
* Restrict endpoint access through network policies, firewall rules, authentication, or other appropriate controls when required.
* Treat the Outbox backlog count as operational metadata and expose it only to authorized monitoring consumers when detailed responses are enabled.

The current design uses a single `/health` endpoint. If the deployment environment requires separate liveness and readiness checks, these can be introduced as distinct endpoints with appropriate check registrations and response behavior.

## 8. Testing

Verify the capability using manual checks and controlled failure scenarios.

### 8.1 Endpoint Availability

Request the health endpoint:

```bash
curl -i http://localhost:5000/health
```

Confirm that the endpoint responds and that its HTTP status corresponds to the aggregate health result.

### 8.2 Database Connectivity

* Run the application with SQL Server available.
* Verify that the database check reports `Healthy`.
* Simulate a database connectivity failure in a controlled environment.
* Verify that the database check reports `Unhealthy`.

### 8.3 Redis Connectivity

* Set `RedisEnabled` to `true` and verify that Redis is checked.
* Simulate a Redis connectivity failure and verify the resulting status.
* Set `RedisEnabled` to `false` and confirm that the Redis check is omitted.

### 8.4 Outbox Backlog Thresholds

Adjust the warning and error thresholds in a test environment.

Verify that:

* A pending count below the warning threshold reports `Healthy`.
* A pending count at or above the warning threshold but below the error threshold reports `Degraded`.
* A pending count at or above the error threshold reports `Unhealthy`.

Also verify the exact threshold-boundary behavior implemented by the comparison operators.

### 8.5 Hangfire Storage Connectivity

* Verify that the check reports `Healthy` when the configured storage database is accessible.
* Verify that it reports `Unhealthy` when the storage connection fails.
* Verify that a missing required connection string produces an unhealthy result.

These tests validate storage connectivity; they do not establish that Hangfire jobs are progressing normally.

## 9. Summary

The Health Checks capability provides a standardized way to monitor critical dependencies and Outbox backlog conditions using ASP.NET Core's built-in health-check infrastructure.

All checks reside in Infrastructure and are registered through a single extension method. Configuration is centralized, thresholds are validated, and the `/health` endpoint provides an operational interface for monitoring tools and orchestration platforms.

The capability remains isolated from Domain and Application concerns, complementing logs, metrics, traces, and alerting within the broader observability architecture.
