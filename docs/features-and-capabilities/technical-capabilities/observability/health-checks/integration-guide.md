# Health Checks — Integration Guide

**Project:** E-Commerce Modular Monolith
**Purpose:** How to Integrate and Use Operational Health Checks
**Version:** 1.0 — Final

---

## 1. Purpose

This guide explains how to integrate and use the Health Checks capability within the E-Commerce modular monolith.

It covers:

* Registering health checks.
* Configuring thresholds and optional checks.
* Exposing the `/health` endpoint.
* Interpreting health statuses.
* Testing health checks locally.
* Applying basic security controls.

The guide is intended for developers and operators responsible for enabling and monitoring application health checks.

## 2. Prerequisites

Before integrating health checks, ensure that:

* The health check implementations exist under `Infrastructure/Observability/HealthChecks/`.
* The required NuGet packages are installed:

  * `AspNetCore.HealthChecks.SqlServer` if using its built-in SQL Server check.
  * `AspNetCore.HealthChecks.Redis` if using its built-in Redis check.
  * `StackExchange.Redis` for Redis connectivity where required by the implementation.
* The API project references the Infrastructure project.
* Configuration is available through `appsettings.json`, environment variables, or another registered configuration provider.

**Implementation note:** Install only the packages required by the actual implementation. If SQL Server or Redis checks are implemented directly using EF Core or `StackExchange.Redis`, their corresponding health-check packages may not be necessary.

## 3. Step-by-Step Integration

### 3.1 Register Health Checks

In `Program.cs`, register the health checks before building the application:

```csharp
builder.Services.AddApplicationHealthChecks(
    builder.Configuration);
```

This extension method registers the health checks and their configuration options.

### 3.2 Map the Health Endpoint

After building the application, map the endpoint:

```csharp
var app = builder.Build();

app.MapHealthChecks("/health");
```

The endpoint exposes the application's aggregate health status at `/health`.

**Important:** The default `MapHealthChecks` response reports the health status but does not automatically provide a detailed JSON response containing every check's results. If monitoring tools require structured details, configure a suitable `ResponseWriter`.

### 3.3 Configure Health Checks

Add the following section to `appsettings.json` if these options are supported by the implementation:

```json
{
  "HealthChecks": {
    "RedisEnabled": true,
    "OutboxWarningThreshold": 100,
    "OutboxErrorThreshold": 500
  }
}
```

Adjust the values according to operational requirements.

Ensure that:

```text
OutboxWarningThreshold >= 0
OutboxErrorThreshold > OutboxWarningThreshold
```

The configuration determines whether Redis is checked and which pending-message counts trigger degraded or unhealthy status.

### 3.4 Run and Verify

Start the application and request the health endpoint:

```bash
curl -i http://localhost:5000/health
```

Use the actual HTTP and HTTPS ports configured for the application.

A successful health check normally returns:

```http
HTTP/1.1 200 OK
```

An unhealthy aggregate result normally returns:

```http
HTTP/1.1 503 Service Unavailable
```

The status code depends on the configured health-check response mapping.

## 4. Health Status Interpretation

| Status      |       Default HTTP Status | Meaning                                                                              |
| ----------- | ------------------------: | ------------------------------------------------------------------------------------ |
| `Healthy`   |                  `200 OK` | All registered checks report healthy.                                                |
| `Degraded`  |                  `200 OK` | One or more checks report degraded status, indicating a potential operational issue. |
| `Unhealthy` | `503 Service Unavailable` | One or more checks report unhealthy status.                                          |

These HTTP mappings are the defaults for `MapHealthChecks`. A custom `ResultStatusCodes` configuration can change them.

Operators should investigate degraded results and promptly respond to unhealthy results according to the application's operational requirements.

**Important:** A degraded result does not necessarily mean a dependency is functioning normally. For example, an outbox backlog may indicate that message processing is falling behind even though the database remains available.

## 5. Configuration Details

### `RedisEnabled`

Controls whether the Redis health check is registered or otherwise enabled, depending on the implementation.

* `true`: Redis health is checked.
* `false`: Redis is excluded from health evaluation.

If Redis is an essential dependency, disabling its health check should be an intentional operational decision rather than a way to suppress failures.

### `OutboxWarningThreshold`

The pending outbox-message count at which the backlog health check reports `Degraded`.

### `OutboxErrorThreshold`

The pending outbox-message count at which the backlog health check reports `Unhealthy`.

### Configuration Validation

The `HealthChecksOptionsValidator` validates the threshold configuration.

It should ensure that:

* Both thresholds are non-negative.
* `OutboxErrorThreshold` is greater than `OutboxWarningThreshold`.

The exact option names and validation behavior must match the implementation.

**Operational consideration:** A message-count threshold alone does not establish whether the outbox is unhealthy. Consider backlog age and processing throughput when defining production alerting thresholds.

## 6. Testing Scenarios

### 6.1 Database Available

**Condition:** SQL Server is reachable and the database health check succeeds.

**Expected result:**

* `SqlServerHealthCheck`: `Healthy`
* Overall status: `Healthy`, assuming all other registered checks are healthy.
* HTTP response: `200 OK`

### 6.2 Database Unavailable

**Condition:** SQL Server is unreachable or the database health check fails.

**Expected result:**

* `SqlServerHealthCheck`: `Unhealthy`
* Overall status: `Unhealthy`, assuming the check is registered and its failure maps to that status.
* HTTP response: `503 Service Unavailable`

### 6.3 Redis Unavailable

**Condition:** Redis is unreachable.

When `RedisEnabled = true`:

* `RedisHealthCheck` reports `Unhealthy` if the connectivity check fails.
* Overall status becomes `Unhealthy`, assuming no custom failure-status mapping overrides the result.
* HTTP response is normally `503 Service Unavailable`.

When `RedisEnabled = false`, Redis is excluded from health evaluation.

### 6.4 Outbox Backlog High

**Condition:** The pending outbox-message count exceeds `OutboxWarningThreshold` but has not exceeded `OutboxErrorThreshold`.

**Expected result:**

* `OutboxBacklogHealthCheck`: `Degraded`
* Overall status: `Degraded`, assuming no other check reports a more severe status.
* HTTP response: `200 OK` under the default mapping.

When the backlog reaches or exceeds the error threshold, the check should report `Unhealthy` if that is how the threshold comparison is implemented.

Verify whether the implementation uses `>` or `>=` at each threshold boundary.

### 6.5 Hangfire Storage Unavailable

**Condition:** Hangfire's configured storage database is unreachable or its storage health check cannot complete successfully.

**Expected result:**

* `HangfireHealthCheck`: `Unhealthy`
* Overall status: `Unhealthy`, assuming the check is registered and no custom mapping overrides it.
* HTTP response: normally `503 Service Unavailable`.

The Hangfire storage check should use the actual configured storage connection. Do not assume it uses `DefaultConnection` unless the implementation explicitly configures it that way.

### 6.6 Optional Redis Check Disabled

**Condition:** Redis is not used in the current deployment, and `RedisEnabled = false`.

**Expected result:**

* Redis is not evaluated.
* Redis availability does not affect the aggregate health status.
* Other registered checks continue to run normally.

## 7. Security Considerations

The `/health` endpoint provides operational information and should be exposed deliberately.

Recommended practices:

* Restrict access to monitoring systems or trusted network segments where appropriate.
* Avoid exposing detailed dependency information publicly unless there is a clear operational requirement.
* Do not return stack traces, credentials, connection strings, or other sensitive information.
* Use authentication, authorization, firewall rules, or network restrictions when required by the deployment environment.
* Consider separating **liveness** and **readiness** endpoints if the application will run under an orchestrator or platform that uses them for process restarts and traffic routing.

A single `/health` endpoint is sufficient for basic health reporting, but it does not automatically distinguish between a process that is alive and one that is ready to receive traffic.

## 8. Summary

Integrating the Health Checks capability involves four main steps:

1. Register health checks using `AddApplicationHealthChecks`.
2. Map the `/health` endpoint.
3. Configure optional checks and backlog thresholds.
4. Verify expected results and connect the endpoint to the monitoring system.

The capability remains isolated within Infrastructure and provides operational visibility without coupling business logic to health-check implementation details.

Health checks complement logs, metrics, traces, and alerting. They provide a view of application and dependency health, while monitoring and alerting systems determine how operational issues are detected and communicated.
