# Notifications

**E-Commerce Modular Monolith**
**Technical Capability — Communication**
**Status:** Implemented

## 1. Overview

The Notifications capability provides a shared infrastructure for delivering user-facing notifications through external communication channels.

It separates **notification intent and business meaning** from **channel-specific delivery mechanisms**, allowing business modules to initiate notifications without depending on providers such as Twilio or email delivery services.

The capability supports three notification channels:

* **Email** — transactional email notifications.
* **SMS** — text-message notifications delivered through Twilio.
* **Push** — mobile push notifications delivered through Firebase Cloud Messaging.

Notifications are part of the modular monolith's communication capabilities, alongside the Outbox and Post-Commit mechanisms.

## 2. Architectural Responsibilities

The Notifications capability is responsible for:

* Providing channel abstractions for Email, SMS, and Push.
* Rendering channel-specific message content using typed notification models and templates where applicable.
* Applying user notification preferences before delivery.
* Integrating with the transactional Outbox for durable notification workflows.
* Recording notification delivery attempts and outcomes where implemented.
* Delegating external delivery to provider-specific Infrastructure components.
* Supporting correlation and observability across notification processing.

### Responsibility Boundaries

| Component                  | Responsibility                                                                                         |
| -------------------------- | ------------------------------------------------------------------------------------------------------ |
| Business modules           | Determine when a notification is required and own its business meaning.                                |
| Integration-event handlers | Translate business integration events into notification requests.                                      |
| Notifications capability   | Coordinate channel-specific processing, preferences, composition, and delivery.                        |
| Outbox capability          | Persist integration events atomically with business changes and support reliable dispatch and retries. |
| Provider transports        | Communicate with external email, SMS, and push providers.                                              |
| Post-Commit mechanism      | Publish non-authoritative real-time hints after a successful database commit, where applicable.        |

The Notifications capability does not own the business lifecycle of orders, payments, shipments, or registrations. Those responsibilities remain with their respective business modules.

## 3. High-Level Processing Flow

A typical durable notification follows this flow:

```text
Business Operation
       |
       v
Domain Event
       |
       v
Integration Event Written to Outbox
       |
       v
Database Transaction Commits
       |
       v
Outbox Processing Job
       |
       v
Integration-Event Handler
       |
       v
Typed Notification Request
       |
       v
Notification Channel
       |
       +----> Check Notification Preferences
       |
       +----> Compose / Render Message
       |
       +----> Provider Transport
                    |
                    v
             External Provider
```

The exact processing steps depend on the notification channel.

**Important:** Successful processing by the external provider does not necessarily mean that the recipient received or read the notification. Actual delivery-status tracking depends on provider callbacks and corresponding persistence, where implemented.

## 4. Notification Channels

### 4.1 Email

Provides transactional email delivery through a channel abstraction, typed notification models, templates, and provider-specific transport.

Typical uses include account-related messages, order updates, and other business notifications.

See [`email.md`](email.md) or the channel-specific reference document in this directory, depending on the documentation filename.

### 4.2 SMS

Provides transactional SMS delivery through a channel abstraction and Twilio transport.

Typical uses include registration verification and order or shipping updates.

The SMS pipeline supports Outbox-driven processing, correlation, notification auditing, and retry handling.

See the SMS notification pillar documentation in this directory.

### 4.3 Push

Provides mobile push notifications through Firebase Cloud Messaging.

Push notifications are delivery mechanisms rather than the system of record for notification state. The application maintains its own notification data where applicable, while push messages alert clients to relevant activity.

See the push notification pillar documentation in this directory.

## 5. Reliability and Delivery Guarantees

Durable notification workflows use the Outbox mechanism rather than relying on an external provider call occurring directly inside a business transaction.

The intended processing guarantees are:

* **Atomic intent persistence:** The business change and its Outbox record are committed within the same database transaction.
* **Retry handling:** Failed processing can be retried according to the Outbox configuration.
* **Dead-letter handling:** Messages that exhaust the configured retry limit can be moved to dead-letter storage for investigation.
* **Per-handler deduplication:** Successfully processed handlers can be skipped on subsequent attempts when their `ProcessedEvents` markers have been committed.
* **Correlation:** Correlation information can be restored from integration events into the processing log scope.
* **Auditing:** Delivery attempts and outcomes are recorded where the relevant channel's audit implementation is enabled.

### Delivery Guarantee

The Outbox provides **at-least-once processing with per-handler deduplication**, not universal exactly-once external delivery.

If a provider accepts a notification but the application fails before recording successful handler processing, a retry can produce a duplicate. Database transactions cannot atomically cover both the local database and an external provider.

Additionally, an accepted provider request does not guarantee final delivery to a recipient's inbox, handset, or device.

Dead-lettering makes persistent failures inspectable; it does not mean the notification was successfully delivered.

## 6. Notification State and Real-Time Updates

Where user-facing notification records are maintained, the database is the source of truth for notification state.

SignalR can be used to notify connected clients that new information is available. Such real-time messages are **hints**, not durable records: a disconnected client can miss them and should retrieve authoritative state through the application's query endpoints.

The Post-Commit capability ensures that relevant real-time notifications are emitted only after the database transaction commits successfully.

See the Post-Commit documentation under the communication technical capabilities.

## 7. Architectural Boundaries

The Notifications capability follows the modular monolith's dependency rules:

* Application-layer contracts do not depend on Twilio, Firebase, or other provider SDKs.
* Provider-specific configuration and transport implementations belong in Infrastructure.
* Business modules do not directly call external notification providers.
* Notification handlers depend on channel abstractions rather than concrete provider transports.
* External side effects are not treated as part of the local database transaction.
* Hangfire is used through the Scheduling capability rather than being referenced directly by business handlers.

These boundaries allow providers and delivery implementations to evolve without coupling business logic to a particular external service.

## 8. Observability and Operations

Operational investigation may involve:

* Outbox message status and retry count.
* Dead-lettered messages.
* Processed-event markers.
* Channel-specific delivery logs.
* Provider errors and response details.
* Correlation IDs across business processing and notification delivery.

Sensitive information, including email addresses, phone numbers, message contents, and provider credentials, must be handled according to the application's security, privacy, and retention requirements.

Provider acceptance, final delivery status, and recipient engagement are distinct outcomes. Monitoring should not report them as equivalent unless the corresponding delivery-status integration exists.

## 9. Related Technical Capabilities

| Capability                              | Responsibility                                                              |
| --------------------------------------- | --------------------------------------------------------------------------- |
| [Outbox](../outbox/README.md)           | Durable integration-event persistence and dispatch.                         |
| [Post-Commit](../post-commit/README.md) | Side effects and real-time hints that must occur after a successful commit. |
| Scheduling                              | Background-job execution and retry infrastructure.                          |
| Observability                           | Logging, diagnostics, and operational visibility.                           |

## 10. Documentation Structure

Channel-specific documents describe each channel's implementation, contracts, configuration, processing flow, and limitations.

Recommended organization:

```text
notifications/
├── README.md
├── email.md
├── sms.md
└── push.md
```

Use the filenames that match the actual files in the repository; the diagram above illustrates the intended organization rather than requiring existing documents to be renamed.

## 11. Summary

The Notifications capability provides a consistent boundary for Email, SMS, and Push delivery within the E-Commerce modular monolith.

It keeps business decisions in their owning modules, channel orchestration behind application contracts, and provider-specific delivery in Infrastructure. Combined with the Outbox, Scheduling, Post-Commit, and observability capabilities, it supports durable notification processing without introducing unnecessary distributed infrastructure.
