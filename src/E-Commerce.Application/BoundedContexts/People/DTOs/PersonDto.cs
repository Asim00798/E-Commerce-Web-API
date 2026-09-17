namespace E_Commerce.Application.BoundedContexts.People.DTOs;

/// <summary>
/// Application-level representation of a Person. Flattens the name components
/// and the address into plain shapes. Consumed by the API layer, which maps
/// it to its own versioned DTO.
/// </summary>
public sealed class PersonDto
{
    public Guid Id { get; init; }
    public Guid? IdentityUserId { get; init; }

    public string FirstName { get; init; } = string.Empty;
    public string? SecondName { get; init; }
    public string? ThirdName { get; init; }
    public string LastName { get; init; } = string.Empty;

    public DateOnly DateOfBirth { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public AddressDto? HomeAddress { get; init; }
    public Guid? PersonalImageFileId { get; init; }
}