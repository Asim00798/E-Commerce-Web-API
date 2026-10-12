# ADR-0012: Automatic Handler and Trigger Discovery

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Architecture / Dependency Injection / Background Processing

## Context

The E-Commerce modular monolith uses application-owned contracts to define background jobs and recurring-job triggers. Business features and technical capabilities own their job payloads, handlers, and recurring-job declarations, while the centralized Scheduling capability provides the execution pipeline through Hangfire.

As the number of background jobs grows, manually registering every handler and recurring trigger becomes repetitive and creates a maintenance risk. A newly added handler might be omitted from dependency injection, or a recurring job might never be registered with Hangfire.

The application therefore needs a consistent mechanism for discovering and registering implementations of the established contracts.

However, automatic discovery must not transfer scheduling infrastructure into business modules, expose Hangfire to Application, or silently accept invalid or ambiguous registrations.

## Decision

**Use automatic discovery to register application-owned job handlers and recurring-job triggers, while keeping discovery, dependency-injection registration, and Hangfire recurring-job registration within Infrastructure.**

The discovery mechanism scans explicitly selected application assemblies for implementations of the relevant contracts and registers them through the application's dependency-injection composition root.

Infrastructure translates discovered recurring-job declarations into Hangfire registrations during startup.

Automatic discovery is a registration mechanism, not a substitute for explicit architectural boundaries or startup validation.

### 1. Job Handler Discovery

Infrastructure discovers implementations of:

`IJobHandler<TJob>`

Each valid handler is registered with dependency injection so the centralized dispatcher can resolve the appropriate implementation when a job executes.

The discovery process must:

* Scan explicitly selected assemblies.
* Identify concrete, non-abstract implementations of the supported handler contracts.
* Register handlers with appropriate dependency-injection lifetimes.
* Avoid registering the same implementation multiple times unintentionally.
* Detect ambiguous handler registrations where the architecture expects a single handler for a job type.
* Fail visibly when required handlers cannot be resolved.

The dispatcher remains responsible for resolving and invoking handlers through the centralized execution pipeline.

Job handlers must not be required to register themselves directly with Hangfire.

### 2. Recurring-Job Trigger Discovery

Recurring jobs are declared through application-owned trigger contracts and metadata.

Infrastructure discovers implementations of `IRecurringJobTrigger` and reads the associated recurring-job metadata, including:

* A stable recurring-job identifier.
* The configured cron expression.
* The associated job type or payload.
* Any additional scheduling configuration explicitly supported by the contract.

The exact metadata representation remains an implementation detail of the established scheduling contracts.

Infrastructure validates the discovered declarations and registers the recurring jobs with Hangfire through the centralized recurring-job bootstrapper.

Business modules must not invoke Hangfire's static `RecurringJob` API or register recurring jobs directly through `IRecurringJobManager`.

### 3. Explicit Assembly Boundaries

Discovery must operate on a known set of assemblies rather than scanning every assembly loaded into the process without restriction.

The selected assemblies should contain the application-owned job handlers and recurring-job declarations intended for registration.

This approach makes discovery predictable and avoids accidentally registering unrelated types, test implementations, or components from external libraries.

The discovery configuration must be updated when a new module or assembly introduces supported job implementations.

A type's presence in an assembly does not automatically make it a valid background job handler. It must implement the relevant contract and satisfy the registration rules.

### 4. Dependency-Injection Registration

Infrastructure owns the composition and registration of the Scheduling pipeline.

The registration process includes:

1. Register the scheduling abstractions and their infrastructure implementations.
2. Discover supported job handlers.
3. Register discovered handlers with dependency injection.
4. Discover recurring-job triggers.
5. Validate recurring-job metadata.
6. Register the recurring-job bootstrapper and other required scheduling infrastructure.
7. Register recurring jobs with Hangfire at the appropriate startup stage.

The exact order must ensure that dependencies are registered before they are resolved and that recurring-job registration occurs only after the required Hangfire storage and infrastructure have been configured.

Handler discovery and recurring-job registration are related but distinct responsibilities. A handler can be valid without being recurring, and a recurring trigger must ultimately refer to executable job behavior.

### 5. Stable Recurring-Job Identity

Every recurring job must have a stable identifier.

The identifier is used to update or locate the recurring registration across application restarts and deployments.

Registration must be deterministic: discovering the same recurring-job declaration repeatedly should update or preserve its intended registration rather than create a new logical recurring job each time.

Duplicate identifiers must be detected and treated as configuration errors unless the registration model explicitly defines a valid resolution rule.

Changing a recurring-job identifier can leave an old registration behind if the previous identifier is not removed. Renames must therefore include an explicit migration or cleanup plan.

The application must also account for multiple instances starting concurrently. Recurring-job registration should be coordinated through Hangfire's supported registration mechanisms and must not assume that only one application instance will ever start.

### 6. Startup Validation and Failure Handling

Automatic discovery must not silently conceal configuration errors.

The application should detect invalid registrations, including:

* Missing required handler implementations.
* Multiple handlers for a job type where only one is supported.
* Duplicate recurring-job identifiers.
* Invalid cron expressions.
* Unsupported or invalid job declarations.
* Unresolvable handler dependencies.
* Recurring triggers that reference job types that cannot be dispatched.

Validation should occur as early as practical, preferably during startup or registration, before the affected jobs are expected to execute.

Where a failure can only be detected when a dependency is resolved or a job executes, the system must report it through the established logging and operational mechanisms.

The application must not claim that every discovered job is operational merely because its type was registered successfully.

### 7. Discovery Does Not Own Business Behavior

Automatic discovery only identifies and registers implementations of established contracts.

It does not determine:

* When a business operation should occur.
* Whether a job is required by a business workflow.
* The business rules executed by a handler.
* Whether a job's side effects are idempotent.
* Whether scheduling requests are durably persisted with business changes.
* The retry policy for a business operation beyond the established execution pipeline.

Those responsibilities remain with the relevant business feature, technical capability, or Scheduling infrastructure, according to their established ownership boundaries.

For example, Ordering owns the expiration rules for pending orders. Discovery makes the corresponding handler available, while Scheduling and Hangfire provide the execution mechanism.

## Consequences

### Positive Consequences

* Adding a new handler generally requires implementing the established contract rather than editing a central registration list.
* Recurring-job registration follows a consistent startup process.
* Registration boilerplate is reduced as the number of jobs grows.
* Business modules remain independent of Hangfire registration APIs.
* Handler resolution and recurring-job configuration remain centralized.
* Startup validation can detect missing, ambiguous, or invalid registrations before they cause operational failures.
* The discovery mechanism supports the existing modular monolith without requiring a separate worker service or distributed messaging infrastructure.

### Trade-Offs

* Automatic registration makes some dependencies less visible than explicit per-type registration.
* Assembly scanning introduces configuration and startup complexity.
* Invalid metadata may prevent application startup if validation is deliberately strict.
* Discovery must be maintained when new assemblies or modules are introduced.
* Registration behavior must remain deterministic across deployments and application instances.
* Tests must verify the discovery and registration rules rather than assume that every matching type is registered correctly.

These trade-offs are acceptable because the application has a growing collection of background jobs and benefits from consistent registration without distributing Hangfire-specific code throughout the solution.

## Alternatives Considered

### 1. Manually Register Every Job Handler and Trigger

Every handler and recurring trigger would be registered explicitly in the composition root.

**Not adopted as the primary strategy:** Explicit registration is straightforward for a small number of jobs, but maintaining a central list becomes repetitive as the job catalog grows.

Manual registration remains appropriate for components that require special construction or exceptional registration behavior.

### 2. Let Each Business Module Register Its Own Hangfire Jobs

Business modules would directly register recurring jobs and configure Hangfire.

**Rejected:** This would leak infrastructure details into business modules and undermine the centralized Scheduling capability.

### 3. Scan Every Loaded Assembly Automatically

Infrastructure would scan all assemblies available in the process.

**Rejected as the default:** Unrestricted scanning can register unrelated implementations and make startup behavior difficult to predict. Explicit assembly selection provides a clearer registration boundary.

### 4. Generate Registrations at Compile Time

A source generator or another build-time mechanism would generate handler and trigger registration code.

**Not adopted:** Runtime discovery is sufficient for the current requirements. Compile-time generation may be reconsidered if startup scanning becomes problematic or stronger compile-time registration guarantees provide a concrete benefit.

## Reconsideration Criteria

Revisit this decision if:

* Assembly scanning materially increases startup time or operational complexity.
* The system requires stronger compile-time guarantees about handler and trigger registration.
* Dynamic module loading introduces new discovery requirements.
* The number of special-case registrations makes automatic discovery harder to maintain than explicit registration.
* Registration failures are difficult to detect or diagnose with the existing validation mechanisms.

Any alternative must preserve the separation between application-owned job contracts, Infrastructure-owned registration, and business-owned job behavior.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md)
* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0007: Per-Handler Idempotency for Integration Events](0007-per-handler-idempotency.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)
* [ADR-0013: Business Process vs. Housekeeping Jobs](0013-business-process-vs-housekeeping-jobs.md)
* [ADR-0014: Single Deployable Unit](0014-single-deployable-unit.md)

## Architectural Principle

**Application defines job contracts and declarations; Infrastructure discovers and registers them; Scheduling executes them through the centralized pipeline.**

Automatic discovery reduces registration overhead without transferring business ownership to infrastructure or allowing Hangfire dependencies to leak into Application and Domain.
