# \# Features and Capabilities

# 

# This directory organizes the business features and technical capabilities of the E-Commerce modular monolith.

# 

# The system distinguishes between \*\*business features\*\*, which own business concepts and rules, and \*\*technical capabilities\*\*, which provide reusable technical functionality to support those features.

# 

# This classification establishes clear responsibility boundaries and complements the system's layered architecture and Domain-Driven Design principles.

# 

# \## Classification

# 

# \### Business Features

# 

# Business features implement business functionality and own the relevant business concepts, rules, and lifecycle. Where appropriate, these concepts are modeled in the Domain layer through aggregates, entities, value objects, domain services, and domain events.

# 

# Examples include:

# 

# \* Catalog

# \* Customer Engagement

# \* Finance

# \* Onboarding

# \* Ordering

# \* People

# \* Shipping

# 

# Identity is an application-level authentication and authorization module backed by ASP.NET Core Identity in Infrastructure. It is not classified as a business bounded context with its own Domain-layer model.

# 

# \### Technical Capabilities

# 

# Technical capabilities provide reusable technical functionality that supports multiple business features. They own technical abstractions, implementation mechanisms, and operational policies rather than business domain models in the Domain layer.

# 

# The existing technical capabilities are organized into the following areas:

# 

# \* \*\*Caching\*\* — application caching and HTTP caching.

# \* \*\*Communication\*\* — Notifications, Outbox, post-commit processing, and real-time communication.

# \* \*\*Database Startup and Migration\*\* — database initialization and migration guidance.

# \* \*\*File Storage\*\* — provider-independent file storage and file lifecycle management.

# \* \*\*Observability\*\* — alerting, health checks, logging, metrics, and tracing.

# \* \*\*Scheduling\*\* — background-job scheduling and execution infrastructure.

# \* \*\*Security\*\* — authentication, authorization, and Identity implementation guidance.

# 

# Technical capabilities may contain application contracts, infrastructure services, persistence models, and technical lifecycle management. These implementation details do not, by themselves, make a capability a business bounded context.

# 

# \## Architectural Principle

# 

# \*\*Business features own business meaning and rules; technical capabilities provide reusable implementation mechanisms.\*\*

# 

# A business feature may use one or more technical capabilities without transferring ownership of its business behavior to those capabilities.

# 

# For example, Ordering determines when an order-cancellation notification should be sent, while the Notifications capability provides the delivery mechanisms. Similarly, Ordering owns order-expiration rules, while Scheduling provides the infrastructure for executing the expiration job.

# 

# The distinction is based on each module's primary responsibility and ownership, not merely on whether it contains entities, persistence, or application services.

# 

# \## Classification References

# 

# This classification is a project-level architectural convention informed by Domain-Driven Design principles. It is not a universal DDD taxonomy: the terms \*business feature\* and \*technical capability\* describe how this project organizes and documents its modules.

# 

# \## Documentation Structure

# 

# \### Business Features

# 

# The `business-features/` directory contains documentation for individual business features.

# 

# Each feature has its own directory, typically containing:

# 

# \* `integration-guide.md` — integration contracts and guidance.

# \* `reference.md` — technical reference information.

# \* `requirements.md` — feature requirements.

# \* `workflow.md` — business workflows and lifecycle behavior.

# 

# The Onboarding feature documents the workflow spanning Identity and People.

# 

# \### Technical Capabilities

# 

# The `technical-capabilities/` directory contains documentation for reusable technical capabilities and cross-cutting concerns.

# 

# Its subdirectories organize related capabilities by responsibility. Depending on the capability, documentation may include architecture descriptions, integration guides, references, requirements, workflows, and edge-case guidance.

# 

# Each capability maintains documentation appropriate to its implementation and responsibilities; not every capability is required to use an identical set of documents.

# 

# \## Maintenance

# 

# Keep feature and capability documentation aligned with the actual solution structure and established architectural decisions.

# 

# When responsibilities change, update the affected documentation to preserve clear ownership boundaries. Business workflows should remain with the features that own them, while reusable technical mechanisms should remain within their appropriate capabilities.

# 

# Where a change affects an established architectural boundary or introduces a significant design decision, document the rationale in the Architecture Decision Records.



