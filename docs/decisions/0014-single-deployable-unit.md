# ADR-0014: Single Deployable Unit

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Architecture

## Context

The E-Commerce system is organized as a modular monolith. Its business features and technical capabilities have defined responsibilities and architectural boundaries, but they do not represent independently deployed services.

Deploying every module as a separate service would introduce additional operational complexity, including service-to-service communication, distributed failure handling, deployment coordination, and potentially distributed data consistency.

The system does not currently require independent deployment, scaling, or operational isolation for individual modules. Its architecture therefore prioritizes strong internal modularity while retaining the simplicity of a single application deployment.

## Decision

The E-Commerce application will be deployed as a **single deployable application**, with modules running within the same application process under the normal deployment configuration.

The application will retain explicit module boundaries, layered dependencies, and controlled communication between modules. A single deployment does not permit unrestricted cross-module access or eliminate the need for clear ownership.

The decision establishes the following rules:

1. **One application deployment:** Business features and technical capabilities are packaged and deployed as parts of the E-Commerce application rather than as independently deployed microservices.
2. **In-process module communication:** Modules communicate through appropriate application contracts, domain events, integration events, and other explicitly defined internal mechanisms.
3. **Shared transactional infrastructure:** Modules use the application's shared database infrastructure where appropriate. Operations requiring atomicity must explicitly participate in the same database transaction and Unit of Work.
4. **No mandatory network boundary between modules:** Communication between modules does not require HTTP calls, a message broker, or other network-based service-to-service mechanisms merely because the modules have separate responsibilities.
5. **Centralized technical capabilities:** Cross-cutting mechanisms such as Scheduling, Outbox processing, Notifications, and Observability provide reusable infrastructure without requiring each module to operate as a separate service.
6. **External dependencies remain external:** SQL Server, Redis, Firebase, and external email or SMS providers may run as separate systems. Their existence does not turn the E-Commerce application into a distributed collection of business services.
7. **Deployment simplicity does not override modularity:** Modules retain their ownership boundaries, and implementation details must not be exposed across modules without an appropriate contract.

## Deployment and Runtime Model

The normal deployment consists of one E-Commerce application containing the API, Application, Domain, Infrastructure, and any selectively used ReadModel components.

The application may connect to external infrastructure such as SQL Server, Redis, and notification providers. These dependencies are configured and operated according to their own requirements.

Background processing uses the centralized Scheduling capability, with Hangfire providing the execution infrastructure. Background jobs remain owned by the modules responsible for their behavior, while Scheduling manages their execution pipeline.

The deployment model does not require every module to have its own process, database, deployment pipeline, or network endpoint.

If operational requirements eventually justify running background workers separately from the API host, that deployment change must be evaluated explicitly. It does not automatically require turning business modules into microservices.

## Consequences

### Positive Consequences

* **Simpler operations:** One application deployment reduces the number of independently managed application services.
* **Simpler communication:** Internal module interactions avoid unnecessary network calls and their associated latency and failure modes.
* **Transactional consistency:** Operations that share the same database transaction can preserve atomicity without distributed transactions.
* **Lower infrastructure overhead:** The system avoids introducing service discovery, inter-service authentication, broker infrastructure, and distributed tracing requirements solely to connect internal modules.
* **Easier debugging:** Developers can investigate many cross-module workflows within one application and execution environment.
* **Preserved architectural evolution:** Explicit module boundaries make it possible to evaluate future extraction of selected modules when concrete requirements justify it.

### Trade-offs and Constraints

* **Shared deployment lifecycle:** A change to one module generally requires deploying the application containing it.
* **Shared process resources:** Modules share the host's process resources and can affect one another through excessive CPU usage, memory consumption, thread-pool pressure, or other resource contention.
* **Limited independent scaling:** Individual business modules cannot ordinarily be scaled independently within the same application deployment.
* **Shared operational failure domain:** A process-level failure can affect all modules hosted by that process.
* **Boundary enforcement remains necessary:** A single process makes direct dependencies convenient, so architecture rules and module contracts must prevent uncontrolled coupling.
* **Future extraction requires deliberate work:** Separating a module may require establishing data ownership, communication contracts, deployment configuration, security boundaries, and independent failure handling.

## Alternatives Considered

### 1. Microservices from the Beginning

Deploy each business module as an independently operated service.

**Rejected because:** The current requirements do not justify the additional operational and distributed-system complexity. Separate deployment units would introduce network boundaries and failure modes without a demonstrated need for independent service ownership or scaling.

### 2. A Traditional Monolith Without Explicit Module Boundaries

Deploy one application while allowing unrestricted dependencies and shared implementation details.

**Rejected because:** A single deployment is a deployment decision, not permission to abandon modular architecture. Unrestricted dependencies would make maintenance and future evolution more difficult.

### 3. Independently Hosted Modules Within One Solution

Run selected modules as separate application processes while retaining a shared codebase.

**Deferred because:** Separate hosting introduces additional deployment, communication, and operational concerns. It may become appropriate for specific workloads or isolation requirements, but it is not the default architecture.

## Reconsideration Criteria

Reevaluate this decision when concrete requirements demonstrate a need for one or more of the following:

* Independent deployment or release schedules for a module.
* Substantially different scaling requirements that cannot be addressed adequately by scaling the application as a whole.
* Stronger runtime, security, regulatory, or fault-isolation boundaries.
* Independent data ownership or operational responsibility.
* A workload whose resource consumption or availability requirements materially conflict with the rest of the application.
* Organizational or product boundaries that justify independently operated services.

Any proposed extraction must include an explicit assessment of data ownership, transaction boundaries, integration contracts, consistency requirements, failure handling, observability, deployment, and operational cost.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md) — establishes the overall architectural style and module organization.
* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md) — defines layer dependencies.
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md) — defines internal domain events and cross-module integration contracts.
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md) — establishes durable integration-event publication.
* [ADR-0009: Scheduling Abstraction Over Hangfire](0009-scheduling-abstraction-over-hangfire.md) — defines centralized background-job execution.
* [ADR-0012: Automatic Handler and Trigger Discovery](0012-automatic-handler-and-trigger-discovery.md) — defines discovery and registration of handlers and recurring jobs.

## Summary

**The E-Commerce system is deployed as one application while maintaining explicit internal module boundaries.** Modules share the application runtime and may share transactional infrastructure, but they retain responsibility for their own business behavior and communicate through appropriate contracts.

Independent services and distributed infrastructure will be introduced only when measurable business or operational requirements justify their additional complexity.
