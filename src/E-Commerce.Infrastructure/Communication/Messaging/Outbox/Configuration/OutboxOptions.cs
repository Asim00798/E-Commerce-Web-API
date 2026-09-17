using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Configuration;

/// <summary>
/// Configuration for the Outbox processing pipeline.
/// </summary>
public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>
    /// Maximum number of processing attempts before a message is moved to the
    /// dead letter table. Set to a value of at least 1 — a value of 0 would
    /// dead-letter every message on its first failure, including transient ones.
    /// </summary>
    [Range(1, 100)]
    public int MaxRetryCount { get; set; } = 5;
}