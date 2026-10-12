# Communication

## Overview

The Communication folder groups related technical capabilities that support communication between application components, business modules, and external recipients.

Although these capabilities serve different purposes, they share a common concern: **reliable communication, event propagation, and coordination across system boundaries and execution stages.**

This folder contains four related capabilities:

| Capability        | Purpose                                                                                                |
| ----------------- | ------------------------------------------------------------------------------------------------------ |
| **Outbox**        | Reliably records integration events alongside business changes and supports their subsequent dispatch. |
| **Notifications** | Manages the delivery of email, SMS, push notifications, and user-facing notifications.                 |
| **Post-Commit**   | Coordinates operations that must execute after a database transaction commits successfully.            |
| **Realtime**      | Delivers live updates to connected clients, primarily through SignalR.                                 |

## Why They Are Grouped Together

These capabilities address complementary parts of the application's communication lifecycle:

* **Outbox** provides durable event publication.
* **Post-Commit** provides a controlled mechanism for triggering work after successful persistence.
* **Notifications** handles notification delivery through supported channels.
* **Realtime** communicates immediate updates to connected clients.

Grouping them together makes related communication concerns easier to discover and maintain while allowing each capability to remain independently documented and responsible for its own technical implementation.

## Architectural Boundary

**Communication is a semantic grouping, not a single technical capability or business bounded context.**

Business modules remain responsible for deciding what happened, when communication is required, and what that communication means. The grouped capabilities provide the technical mechanisms for propagating events, coordinating post-commit work, delivering notifications, and sending realtime updates.

This separation keeps business rules within their owning modules while organizing related communication mechanisms under one documentation category.

---

**Category:** Technical Capabilities
**Grouping:** Communication
**Project:** E-Commerce Modular Monolith
