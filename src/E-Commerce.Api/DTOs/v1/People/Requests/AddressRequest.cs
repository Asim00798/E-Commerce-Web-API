using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;

namespace E_Commerce.Api.DTOs.v1.People.Requests;

/// <summary>
/// API v1 request shape for a Person's home address.
///
/// Currently mirrors the Application AddressDto field-for-field. Owned by
/// the API layer: renaming or reshaping here will not affect the Application
/// contract, and vice versa.
/// </summary>
public sealed class AddressRequest
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public AddressType Type { get; set; } = AddressType.Home;
    public string? LocationMapUrl { get; set; }
}