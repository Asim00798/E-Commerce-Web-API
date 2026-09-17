namespace E_Commerce.Api.DTOs.v1.People.Responses;

/// <summary>
/// API v1 wire shape for a Person's home address.
///
/// Reshapes relative to the Application AddressDto:
///   - <c>Type</c> is flattened from the AddressType enum to a string
///     so the wire contract does not depend on Domain enum numbering.
///
/// Owned by the API layer. Reused inside <see cref="PersonResponse"/>.
/// </summary>
public sealed record AddressResponse(
    string Street,
    string City,
    string Type,
    string? LocationMapUrl);