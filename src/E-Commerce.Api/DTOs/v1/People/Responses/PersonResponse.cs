namespace E_Commerce.Api.DTOs.v1.People.Responses;

/// <summary>
/// Public API v1 representation of a Person.
///
/// Hides: the internal IdentityUserId link — clients address their own
/// Person through <c>/people/me</c> and never need the identity identifier.
///
/// Reshapes: Gender is flattened from the Domain enum to a string, and
/// FullName is flattened from a value object to four separate fields —
/// consistent with how Application PersonDto exposes them.
///
/// Owned by the API layer for v1 contract isolation.
/// </summary>
public sealed record PersonResponse(
    Guid Id,
    string FirstName,
    string? SecondName,
    string? ThirdName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Email,
    AddressResponse? HomeAddress,
    Guid? PersonalImageFileId);