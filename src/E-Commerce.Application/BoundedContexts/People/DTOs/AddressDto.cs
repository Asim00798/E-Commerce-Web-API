using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;

namespace E_Commerce.Application.BoundedContexts.People.DTOs;

public sealed class AddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public AddressType Type { get; init; } = AddressType.Home;
    public string? LocationMapUrl { get; init; }
}