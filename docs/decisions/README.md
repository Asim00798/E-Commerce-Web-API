# Architecture Decision Records

This directory contains the Architecture Decision Records (ADRs) for the E-Commerce modular monolith.

ADRs document significant architectural decisions, including their context, the chosen approach, alternatives considered, consequences, and circumstances that may justify revisiting them.

They provide a durable record of the reasoning behind the system's architecture and help keep future implementation decisions consistent with established architectural principles.

## Purpose

The ADRs establish and communicate the architectural decisions governing the E-Commerce system, including:

* Application structure and module boundaries.
* Clean Architecture dependency direction.
* Command and query separation.
* Domain events and cross-module integration.
* Transactional consistency and event delivery.
* Idempotency and failure handling.
* Read-model implementation.
* Background-job scheduling and execution.
* Technical capability ownership.
* Deployment and operational boundaries.

These decisions complement the project's architecture documentation, technical references, and implementation-specific guidance. They do not replace those documents.

## Decision Index

| ADR                                                     | Title                                   | Status   |
| ------------------------------------------------------- | --------------------------------------- | -------- |
| [0001](0001-modular-monolith.md)                        | Modular Monolith                        | Accepted |
| [0002](0002-clean-architecture-dependency-direction.md) | Clean Architecture Dependency Direction | Accepted |
| [0003](0003-selective-cqrs.md)                          | Selective CQRS                          | Accepted |
| [0004](0004-domain-events-vs-integration-events.md)     | Domain Events vs. Integration Events    | Accepted |
| [0005](0005-transactional-outbox.md)                    | Transactional Outbox                    | Accepted |
| [0006](0006-two-phase-save-in-unit-of-work.md)          | Two-Phase Save in Unit of Work          | Accepted |
| [0007](0007-per-handler-idempotency.md)                 | Per-Handler Idempotency                 | Accepted |
| [0008](0008-readmodel-ado-net.md)                       | ReadModel with ADO.NET                  | Accepted |
| [0009](0009-scheduling-abstraction-over-hangfire.md)    | Scheduling Abstraction over Hangfire    | Accepted |
| [0010](0010-file-storage-not-a-bounded-context.md)      | File Storage Is Not a Bounded Context   | Accepted |
| [0011](0011-notification-categories.md)                 | Notification Categories                 | Accepted |
| [0012](0012-automatic-handler-and-trigger-discovery.md) | Automatic Handler and Trigger Discovery | Accepted |
| [0013](0013-business-process-vs-housekeeping-jobs.md)   | Business Process vs. Housekeeping Jobs  | Accepted |
| [0014](0014-single-deployable-unit.md)                  | Single Deployable Unit                  | Accepted |

## Architectural Principles

The ADRs collectively establish the following principles:

### Modularity Without Unnecessary Distribution

The application is organized as a modular monolith with explicit responsibilities and controlled module interactions. Separate services, brokers, and other distributed infrastructure are introduced only when justified by concrete requirements.

### Explicit Dependency Direction

The application follows Clean Architecture dependency rules. Domain and Application code remain independent of concrete infrastructure implementations, while Infrastructure provides the required technical mechanisms.

### Business Ownership and Technical Capabilities

Business features own their business concepts, rules, and workflows. Technical capabilities provide reusable implementation mechanisms without taking ownership of business behavior.

### Controlled Cross-Module Communication

Domain events represent internal domain occurrences, while integration events define cross-module communication contracts. Publishers own integration-event contracts, and subscribing modules own their handlers.

### Transactional Reliability

Where business changes and integration-event publication intent must be atomic, they are persisted within the same database transaction through the transactional outbox. Asynchronous delivery must tolerate retries and duplicate processing.

### Selective Architectural Complexity

CQRS, dedicated read models, background processing, and other specialized mechanisms are used where their responsibilities or measurable benefits justify them. They are not imposed uniformly without a concrete reason.

### Clear Technical Ownership

Cross-cutting capabilities such as Scheduling, File Storage, Notifications, and Observability centralize reusable technical concerns while preserving business-feature ownership.

### Deployment Simplicity

The application is deployed as a single application by default. Internal modularity does not require independently deployed services, and a future move toward distributed deployment requires an explicit architectural decision.

## ADR Structure

Each ADR follows a consistent structure appropriate to the decision being documented. A typical record includes:

* **Title and identifier** — a unique, sequential number and descriptive name.
* **Status** — the current state of the decision.
* **Date** — when the decision was recorded.
* **Context** — the problem, constraints, and relevant architectural circumstances.
* **Decision** — the selected approach and its governing rules.
* **Consequences** — expected benefits, trade-offs, and constraints.
* **Alternatives Considered** — other approaches evaluated and the reasons for rejecting or deferring them.
* **Reconsideration Criteria** — circumstances that may justify revisiting the decision.
* **Related Decisions** — links to ADRs that establish or depend on related architectural choices.

Not every decision requires identical section headings. The objective is to document the reasoning and implications clearly rather than enforce a rigid template.

## Creating or Updating ADRs

Create an ADR when a decision has meaningful, lasting consequences for the system's structure, dependencies, reliability, security, data consistency, deployment, or operational model.

When documenting a decision:

1. Assign the next available sequential identifier.
2. Describe the context and constraints that motivate the decision.
3. State the chosen approach and the rules that follow from it.
4. Document relevant trade-offs and alternatives.
5. Identify conditions that could justify revisiting the decision.
6. Link related ADRs and update this index.
7. Keep the record consistent with the implemented architecture and other authoritative documentation.

When an accepted decision changes, update the existing ADR's status or supersession information as appropriate and create a new ADR when a materially different decision needs its own historical record. Avoid silently rewriting history when the rationale or direction has changed.

## Status Conventions

Use the following statuses where applicable:

* **Proposed** — under consideration and not yet accepted.
* **Accepted** — approved and governing the intended architecture.
* **Deprecated** — no longer recommended, but retained for historical context.
* **Superseded** — replaced by a newer decision; link to the replacement ADR.

The status describes the decision's architectural standing, not necessarily the completion status of every implementation task associated with it.

## Maintenance

ADRs should evolve alongside the system's architecture.

When implementation changes affect an established decision, verify whether the change remains consistent with the ADR. If it does not, either revise the implementation or document an explicit architectural change.

Keep cross-references valid, ensure the index reflects the available records, and avoid duplicating detailed implementation guidance that belongs in the relevant architecture documents or technical-capability references.

The goal is to preserve a clear, navigable history of **what was decided, why it was decided, and when the decision should be reconsidered**.
