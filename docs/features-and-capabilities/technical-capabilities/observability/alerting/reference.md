\# Alerting — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Capability:\*\* Operational Alerting Based on an External Observability Platform

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Overview



Alerting is an operational capability that detects abnormal system conditions and notifies the appropriate teams or systems.



In this modular monolith architecture, alerting is \*\*externalized\*\*. The application does not implement alert rules, manage incident channels, or directly deliver operational alerts. Instead, it exposes telemetry signals—including logs, metrics, traces, and health checks—that an external observability platform consumes to evaluate alert rules and trigger notifications.



This document defines recommended operational alerts, their source signals, example thresholds, severity levels, and configuration guidance for external observability platforms.



It serves as a reference for operations teams and DevOps engineers responsible for monitoring and operational readiness.



\---



\## 2. Scope



\### 2.1 In Scope



\* Recommended operational alerts based on application telemetry.

\* Mapping alert rules to metrics, logs, and health checks.

\* Severity classification: Informational, Warning, and Critical.

\* Suggested thresholds and evaluation durations.

\* Guidance on external platforms that can consume the signals.



\### 2.2 Out of Scope



\* Application code for sending operational alerts.

\* Business and customer notifications, such as order confirmations and payment failure notifications.

\* In-application alert delivery.

\* Observability platform deployment and infrastructure configuration.

\* Dashboard design and visualization.

\* Incident management workflows.



\---



\## 3. Architecture



The alerting architecture separates telemetry production from alert evaluation and notification.



```text

Application

&#x20;   |

&#x20;   +-- Logs

&#x20;   |

&#x20;   +-- Metrics

&#x20;   |

&#x20;   +-- Traces

&#x20;   |

&#x20;   +-- Health Checks

&#x20;           |

&#x20;           v

External Observability Platform

&#x20;   |

&#x20;   +-- Collect and ingest telemetry

&#x20;   |

&#x20;   +-- Evaluate alert rules

&#x20;   |

&#x20;   +-- Detect abnormal conditions

&#x20;   |

&#x20;   +-- Apply severity levels

&#x20;   |

&#x20;   +-- Trigger notifications

&#x20;           |

&#x20;           +-- PagerDuty

&#x20;           +-- Slack

&#x20;           +-- Email

&#x20;           +-- Webhooks

```



The application is responsible for producing meaningful, reliable telemetry. The external platform owns alert-rule evaluation and operational notification delivery.



The application may expose health endpoints and instrumentation, but those signals do not constitute a complete alerting system until an external platform collects and evaluates them.



\---



\## 4. Alert Sources and Signals



The following telemetry sources can support operational alerting, provided the corresponding instrumentation and integrations are configured.



| Signal        | Source                                               | Examples                                                                         |

| ------------- | ---------------------------------------------------- | -------------------------------------------------------------------------------- |

| Metrics       | OpenTelemetry Meter and ASP.NET Core instrumentation | HTTP request counts, request duration, application operation duration            |

| Logs          | Serilog                                              | Exceptions, background job failures, Outbox processing errors, database failures |

| Traces        | OpenTelemetry tracing                                | Span duration, error status, dependency calls, request execution paths           |

| Health checks | ASP.NET Core health checks                           | SQL Server, Redis, Hangfire, Outbox processing, Outbox backlog                   |



\### Implementation Considerations



\* Metric names and dimensions depend on the instrumentation libraries, exporter, and configuration. Alert rules must use the actual exported names.

\* Logs must include appropriate severity levels and useful contextual properties to support reliable filtering and aggregation.

\* Traces are useful for investigating slow or failed operations, but trace-based alerting requires a platform that supports the necessary queries or derived signals.

\* Health checks must be registered and exposed before an external platform can evaluate them.

\* An Outbox backlog health check is useful only if it is implemented and configured with meaningful thresholds.



A signal described in this document should not be assumed to exist merely because it is listed here.



\---



\## 5. Recommended Operational Alerts



The thresholds below are starting points, not universal production limits. They should be adjusted according to normal traffic, service-level objectives, dependency behavior, and observed baseline performance.



\### 5.1 High HTTP Error Rate



\*\*Severity:\*\* Critical



\*\*Signal:\*\* HTTP request metrics, including the HTTP response status code.



\*\*Example threshold:\*\* HTTP 5xx error rate exceeds 5% over a five-minute evaluation window.



\*\*Purpose:\*\* Detect service degradation, unexpected application failures, and dependency-related errors.



\*\*Recommended action:\*\*



\* Investigate logs and traces for recurring exceptions.

\* Identify affected endpoints and dependencies.

\* Check recent deployments and configuration changes.

\* Determine whether the issue affects the entire application or only specific operations.



\*\*Evaluation considerations:\*\* Avoid triggering an error-rate alert when request volume is too low for the percentage to be meaningful. Consider adding a minimum request-volume condition.



\### 5.2 High Request Latency



\*\*Severity:\*\* Warning



\*\*Signal:\*\* HTTP request-duration histogram.



\*\*Example threshold:\*\* The 95th percentile of request duration exceeds one second for five minutes.



\*\*Purpose:\*\* Detect performance degradation before it becomes a widespread availability issue.



\*\*Recommended action:\*\*



\* Analyze traces to identify slow components.

\* Investigate database queries and external dependencies.

\* Review CPU, memory, connection pools, and thread-pool pressure.

\* Compare current latency with normal traffic patterns.



\*\*Evaluation considerations:\*\* The one-second threshold is illustrative. Select latency limits according to endpoint requirements and service-level objectives. Percentile calculations also depend on the available metric type and aggregation method.



\### 5.3 SQL Server Unavailable



\*\*Severity:\*\* Critical



\*\*Signal:\*\* SQL Server health check, such as `SqlServerHealthCheck`, if registered.



\*\*Example threshold:\*\* The database health check reports `Unhealthy` for the configured evaluation period.



\*\*Purpose:\*\* Detect database connectivity or availability failures.



\*\*Recommended action:\*\*



\* Verify SQL Server availability.

\* Check database connectivity and network access.

\* Validate connection-string configuration and credentials.

\* Investigate database resource pressure and connection limits.



\*\*Evaluation considerations:\*\* A single failed probe may be transient. Use an appropriate evaluation duration or consecutive-failure condition to reduce false positives.



\### 5.4 Redis Unavailable



\*\*Severity:\*\* Critical when Redis is a critical dependency for the affected workload.



\*\*Signal:\*\* Redis health check, such as `RedisHealthCheck`, if registered and enabled.



\*\*Example threshold:\*\* The Redis health check reports `Unhealthy` for the configured evaluation period.



\*\*Purpose:\*\* Detect cache dependency failures.



\*\*Recommended action:\*\*



\* Verify Redis service availability.

\* Check network connectivity and authentication.

\* Inspect Redis resource usage and connection behavior.

\* Determine whether the affected application functionality can operate safely without the cache.



\*\*Evaluation considerations:\*\* Severity depends on Redis's role. If the application can continue operating correctly without Redis, a Warning may be more appropriate than a Critical alert.



\### 5.5 High Outbox Backlog



\*\*Severity:\*\* Warning for elevated backlog; Critical when the backlog indicates a serious processing failure.



\*\*Signal:\*\* Outbox backlog health check, such as `OutboxBacklogHealthCheck`, if implemented.



\*\*Example thresholds:\*\*



| Severity | Example condition                 |

| -------- | --------------------------------- |

| Warning  | Pending Outbox records exceed 100 |

| Critical | Pending Outbox records exceed 500 |



\*\*Purpose:\*\* Detect Outbox processing failures, sustained backlog growth, or delayed delivery of integration events.



\*\*Recommended action:\*\*



\* Check Outbox processing jobs and their execution history.

\* Investigate database performance and connection failures.

\* Review message-handler failures and retry behavior.

\* Check whether the backlog is growing faster than the processor can drain it.



\*\*Evaluation considerations:\*\* Pending-record counts alone may not indicate urgency. Consider record age, processing throughput, retry counts, and whether the oldest pending event exceeds an acceptable delivery delay.



\### 5.6 Repeated Background Job Failures



\*\*Severity:\*\* Warning



\*\*Signal:\*\* Background job metrics, where available, and structured logs.



\*\*Example threshold:\*\* Job failure rate exceeds 10% over ten minutes.



\*\*Purpose:\*\* Detect failures in scheduled and background processing, including Outbox dispatch and file cleanup.



\*\*Recommended action:\*\*



\* Inspect job execution logs and exception details.

\* Verify database and storage-provider availability.

\* Review retry history and recurring-job execution.

\* Identify whether failures affect a single job type or multiple jobs.



\*\*Evaluation considerations:\*\* The failure-rate calculation requires reliable execution and failure counts. If those metrics are unavailable, use structured logs or the job scheduler's monitoring data to derive an equivalent signal.



Do not assume that a `JobOrchestrator` metric exists unless the application actually emits it.



\### 5.7 High Application Exception Rate



\*\*Severity:\*\* Warning



\*\*Signal:\*\* Error-level logs or application operation metrics that record failed outcomes.



\*\*Example threshold:\*\* More than ten application errors per minute, sustained for ten minutes.



\*\*Purpose:\*\* Detect unexpected application faults that may not immediately produce a high HTTP 5xx rate.



\*\*Recommended action:\*\*



\* Group errors by exception type and operation.

\* Correlate logs with traces and request identifiers.

\* Identify recurring failure patterns.

\* Determine whether failures are expected business outcomes or unexpected application faults.



\*\*Evaluation considerations:\*\* Expected validation failures and business rejections should not automatically be treated as application exceptions. Filter known, intentionally handled conditions to reduce alert noise.



\### 5.8 Overall Health Check Unhealthy



\*\*Severity:\*\* Critical when the aggregate status indicates loss of essential functionality.



\*\*Signal:\*\* The aggregate health endpoint, such as `/health`.



\*\*Example threshold:\*\* The endpoint reports an aggregate `Unhealthy` status for the configured evaluation period.



\*\*Purpose:\*\* Detect critical dependency or application health failures.



\*\*Recommended action:\*\*



\* Inspect individual health-check results.

\* Identify the unhealthy dependency or subsystem.

\* Correlate the failure with logs and traces.

\* Determine the impact on essential application functionality.



\*\*Evaluation considerations:\*\* A health endpoint's aggregate status depends on which checks are registered and how they are grouped. Distinguish readiness checks from liveness checks, and avoid treating every optional dependency failure as a complete application outage.



\---



\## 6. Alert Severity Levels



| Severity      | Description                                                                                              | Expected response                                          |

| ------------- | -------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- |

| Informational | A noteworthy condition that may indicate a developing issue but does not require immediate intervention. | Review during normal operational monitoring.               |

| Warning       | Degraded performance, elevated resource pressure, or repeated failures that require investigation.       | Investigate within hours, according to operational policy. |

| Critical      | A service outage, essential dependency failure, or serious condition requiring urgent intervention.      | Investigate immediately.                                   |



These response expectations are recommendations. Production teams should define explicit response targets based on business impact, operating hours, and incident-response arrangements.



\---



\## 7. Recommended Tooling



The following platforms can consume application telemetry and support operational alerting.



| Tool                                   | Primary use                                                               |

| -------------------------------------- | ------------------------------------------------------------------------- |

| Prometheus and Alertmanager            | Metrics collection, rule evaluation, and alert routing                    |

| Grafana                                | Metrics and log visualization, alert rules, and notification integrations |

| Azure Monitor and Application Insights | Application telemetry, metrics, logs, tracing, and alerts                 |

| Datadog                                | Unified metrics, logs, traces, and alerting                               |

| Elasticsearch and Kibana               | Log analysis and supported alerting workflows                             |

| Seq                                    | Structured log search and supported alerting workflows                    |



The appropriate platform depends on the deployment environment, telemetry exporters, operational requirements, and existing infrastructure.



These options are alternatives rather than mandatory components. The architecture does not require adopting every listed tool.



\---



\## 8. Configuration Guidance



Alert rules should be defined in the external observability platform using thresholds appropriate to the application's actual telemetry and operational requirements.



\### 8.1 Example Prometheus Alert Rule



The following is an illustrative Prometheus rule for a high HTTP 5xx error rate:



```yaml

groups:

&#x20; - name: ecommerce-alerts

&#x20;   rules:

&#x20;     - alert: HighHTTPErrorRate

&#x20;       expr: |

&#x20;         (

&#x20;           sum(rate(http\_server\_request\_count{status\_code=\~"5.."}\[5m]))

&#x20;           /

&#x20;           sum(rate(http\_server\_request\_count\[5m]))

&#x20;         ) > 0.05

&#x20;       for: 5m

&#x20;       labels:

&#x20;         severity: critical

&#x20;       annotations:

&#x20;         summary: "High HTTP 5xx error rate detected"

&#x20;         description: "The HTTP 5xx error rate has exceeded 5% for five minutes."

```



\*\*Important:\*\* The metric name `http\_server\_request\_count` and label `status\_code` are illustrative. The actual names depend on the OpenTelemetry instrumentation, metric exporter, and Prometheus naming conventions in use. Replace them with the names and dimensions actually exposed by the application.



The expression also assumes that the selected request metric is a counter and that its labels allow successful aggregation across the relevant application instances.



For low-traffic applications, consider requiring a minimum request rate before evaluating the error percentage.



\### 8.2 Health Check Alerts



Health-check alerts can be based on:



\* The HTTP status code returned by the health endpoint.

\* A platform-specific health-check integration.

\* Individual health-check results, where the platform can access them.

\* Metrics derived from health-check execution.



A simple endpoint probe can detect that the endpoint is failing, but it may not reveal which dependency is unhealthy. Where possible, preserve individual check results in telemetry or an operationally accessible endpoint.



\### 8.3 Alert Rule Design Principles



When configuring alerts:



1\. Use thresholds based on operational baselines and business requirements.

2\. Include an evaluation duration to reduce noise from brief transient failures.

3\. Distinguish service-wide failures from isolated endpoint or dependency problems.

4\. Avoid duplicate alerts for the same underlying incident.

5\. Ensure that every alert has a clear purpose and an actionable response.

6\. Validate that the underlying telemetry is actually collected.

7\. Review alert frequency and false positives after deployment.

8\. Ensure the external notification destination is configured and monitored.



The application produces telemetry; the external platform evaluates the rules and routes notifications.



\---



\## 9. Summary



Operational alerting is externalized and relies on telemetry produced by the E-Commerce modular monolith: logs, metrics, traces, and health checks.



The application does not implement operational alert rules or manage incident notifications. Instead, an external observability platform consumes the available signals, evaluates configured conditions, assigns severity levels, and notifies the appropriate operational recipients.



This document provides recommended alert categories, example thresholds, severity guidance, and tooling considerations. The recommendations should be validated against the telemetry actually emitted by the application and adjusted to meet production service-level objectives.



\*\*Core principle:\*\* The application is responsible for producing reliable and useful telemetry; the external observability platform is responsible for evaluating alerts and delivering operational notifications.



\---



\*\*Document Version:\*\* 1.0 — Final



