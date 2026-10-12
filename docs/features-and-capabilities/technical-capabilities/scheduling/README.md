# Scheduling

**E-Commerce Modular Monolith — Technical Capability**

## Overview

The Scheduling capability provides a centralized infrastructure for executing background jobs within the E-Commerce modular monolith.

It acts as a **controlled execution gateway** between application-defined jobs and the underlying background-processing technology. It standardizes job registration, scheduling, execution, policy enforcement, failure handling, and observability while keeping application code independent of Hangfire.

The capability supports both business-process jobs and housekeeping jobs without allowing either category to bypass the shared execution pipeline.

## Objectives

The Scheduling capability is designed to:

* Provide a consistent execution pipeline for all background jobs.
* Keep application-layer contracts independent of Hangfire.
* Centralize job dispatching, execution policies, and failure handling.
* Support immediate, delayed, and recurring job execution.
* Integrate with the Outbox processing workflow.
* Preserve feature ownership of job payloads, handlers, and business logic.
* Prevent background-job infrastructure from leaking into business modules.
* Enable the underlying scheduling technology to be replaced without redesigning application-level job contracts.

## Architectural Principles

### 1. Centralized Execution Gateway

All executable job payloads pass through the shared job execution pipeline.

The pipeline coordinates job-handler resolution, execution context, configured policies, and exception propagation. This provides a consistent execution model across the application.

### 2. Application-Layer Abstractions

The Application layer defines the contracts required to declare and schedule jobs, including:

* `IJob`
* `IJobHandler<TJob>`
* `IJobContext`
* `IJobScheduler`
* `IRecurringJobTrigger`
* `IJobPolicy`

Application code uses these abstractions rather than referencing Hangfire APIs directly.

### 3. Infrastructure-Owned Scheduling Technology

Hangfire is an Infrastructure implementation detail. Its storage, server configuration, dispatching, recurring-job registration, and technology-specific execution context remain within Infrastructure.

### 4. Feature-Owned Job Logic

Individual features own their job payloads, handlers, and recurring-job triggers where applicable.

The Scheduling capability provides shared execution infrastructure; it does not take ownership of business logic belonging to Catalog, Ordering, Finance, Notifications, or other modules.

### 5. Consistent Failure Handling

Job execution follows the shared pipeline and its configured policies. Exceptions must propagate appropriately so Hangfire can apply its configured retry and failure-handling behavior.

The implementation must avoid overlapping retry mechanisms that could multiply execution attempts unexpectedly.

### 6. Durable Business Communication

The Scheduling capability supports background processing of Outbox messages. It does not replace the Outbox or the integration-events pipeline.

Business changes and their corresponding Outbox records must be persisted atomically when they represent one business transaction. Subsequent dispatch and processing occur through the durable asynchronous workflow.

## Job Categories

The application distinguishes between two practical job categories:

| Category              | Responsibility                                                            | Examples                                                                                     |
| --------------------- | ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| Business-process jobs | Perform or advance application workflows outside the originating request. | Expiring pending orders or processing business operations that require background execution. |
| Housekeeping jobs     | Maintain infrastructure, storage, or operational data.                    | File-storage cleanup and dead-letter monitoring.                                             |

These categories describe the purpose of jobs within this application; they are not universal DDD classifications.

See [`background-job-classification.md`](background-job-classification.md) for the detailed classification and design rules.

## Execution Model

The typical execution flow is:

1. A feature defines a job payload and its corresponding handler.
2. Application code requests execution through `IJobScheduler`.
3. The Infrastructure implementation submits the job to Hangfire.
4. Hangfire invokes the Infrastructure dispatcher.
5. The dispatcher resolves the registered job handler and establishes the execution context.
6. The shared job orchestrator executes the handler through the configured policy pipeline.
7. Successful execution completes normally.
8. Failures propagate to Hangfire, which applies the configured retry and failure-handling behavior.

Recurring jobs use a registration and trigger mechanism to enqueue the actual job payload. The recurring trigger is distinct from the payload handler: the trigger initiates execution, while the payload is processed through the shared execution pipeline.

## Relationship with Other Capabilities

Scheduling works alongside other technical capabilities without absorbing their responsibilities.

| Capability                  | Relationship                                                                                                    |
| --------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Integration Events Pipeline | Uses background jobs to process and dispatch durable integration events through the Outbox workflow.            |
| Outbox                      | Provides durable recording of integration events in the same transaction as the corresponding business changes. |
| Observability               | Provides logging, metrics, tracing, health checks, and operational visibility where configured.                 |
| File Storage                | May define housekeeping jobs for storage-related maintenance.                                                   |
| Business Features           | Define feature-owned job payloads and handlers when background execution is required.                           |

## Documentation Structure

This directory contains the following documents:

| Document                                                               | Description                                                                                                       |
| ---------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| [`integration-guide.md`](integration-guide.md)                         | Explains how application features integrate with the Scheduling capability.                                       |
| [`reference.md`](reference.md)                                         | Documents the architecture, contracts, infrastructure components, execution pipeline, and implementation details. |
| [`requirements.md`](requirements.md)                                   | Defines the capability's functional requirements, reliability expectations, and architectural constraints.        |
| [`workflow.md`](workflow.md)                                           | Describes the workflows for defining, scheduling, executing, and handling failures in background jobs.            |
| [`integration-events-pipeline.md`](integration-events-pipeline.md)     | Documents durable integration-event processing, the transactional Outbox, dispatching, and idempotent handling.   |
| [`background-job-classification.md`](background-job-classification.md) | Explains business-process jobs versus housekeeping jobs and their respective responsibilities.                    |

## Architectural Boundaries

The following rules govern the capability:

* Application code must not depend on Hangfire.
* Feature handlers must not directly manage Hangfire job registration or scheduling.
* Infrastructure must not absorb feature-specific business logic.
* Job payloads should contain the minimum information necessary for execution, preferably identifiers rather than sensitive or unnecessarily large data.
* Background jobs must not assume the existence of an HTTP request, `HttpContext`, or an authenticated request principal.
* Job handlers must account for retries and possible repeated execution.
* Business changes and corresponding Outbox records must share the appropriate transaction boundary.
* A job request that must be guaranteed by a business transaction requires durable coordination; a separate enqueue operation after commit alone does not guarantee that the job will be scheduled.
* Scheduling configuration, connection strings, and credentials must be managed through appropriate configuration and secret-management mechanisms rather than hardcoded values.

## Design Goal

The Scheduling capability provides a **shared, technology-independent application contract and a controlled infrastructure execution pipeline** for background work.

Its purpose is to standardize execution and failure handling while preserving feature ownership, transaction boundaries, and architectural separation throughout the E-Commerce modular monolith.
