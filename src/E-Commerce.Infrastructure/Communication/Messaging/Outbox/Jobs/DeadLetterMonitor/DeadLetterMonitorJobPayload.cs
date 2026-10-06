using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Jobs.DeadLetterMonitor;

/// <summary>
/// Background job that scans the dead-letter table and emits an operational
/// warning when quarantined messages exist.
/// </summary>
public sealed record DeadLetterMonitorJobPayload : IJob;