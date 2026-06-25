using System.Text.Json;

namespace E_Commerce.Infrastructure.Communication.Messaging.Serialization;

/// <summary>
/// Handles JSON serialization and deserialization of integration events
/// for the Outbox pattern. This class is the single point of contact
/// for converting <see cref="IIntegrationEvent"/> objects to durable
/// <see cref="OutboxMessage"/> payloads and back.
/// </summary>
/// <remarks>
/// <para>
/// The serializer uses <c>System.Text.Json</c> with a consistent set of
/// options (<c>camelCase</c> property naming, no indentation) to ensure
/// compact, deterministic storage in the Outbox table.
/// </para>
/// <para>
/// Because the integration event type is only known at runtime (it is
/// stored as a fully qualified type name in the <c>EventType</c> column),
/// the deserialization method accepts a <see cref="Type"/> argument.
/// </para>
/// </remarks>
public class OutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an integration event to a compact JSON string.
    /// </summary>
    /// <typeparam name="T">The concrete <see cref="IIntegrationEvent"/> type.</typeparam>
    /// <param name="obj">The integration event instance to serialize.</param>
    /// <returns>A JSON string representation of the event.</returns>
    public string Serialize<T>(T obj)
        => JsonSerializer.Serialize(obj, Options);

    /// <summary>
    /// Deserializes a JSON string back into an integration event object.
    /// </summary>
    /// <param name="json">The JSON payload from the Outbox message.</param>
    /// <param name="type">
    /// The concrete <see cref="Type"/> of the integration event (resolved
    /// from the <c>EventType</c> column at runtime).
    /// </param>
    /// <returns>
    /// The deserialized integration event as <see cref="object"/>, or
    /// <c>null</c> if the JSON is invalid or empty.
    /// </returns>
    public object? Deserialize(string json, Type type)
        => JsonSerializer.Deserialize(json, type, Options);
}