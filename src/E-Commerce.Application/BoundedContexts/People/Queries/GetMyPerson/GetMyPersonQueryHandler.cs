using E_Commerce.Application.BoundedContexts.People.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Queries.GetMyPerson;

public sealed class GetMyPersonQueryHandler
    : IRequestHandler<GetMyPersonQuery, Result<PersonDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUser _currentUser;

    public GetMyPersonQueryHandler(
        IPersonRepository personRepository,
        ICurrentUser currentUser)
    {
        _personRepository = personRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<PersonDto>> Handle(
        GetMyPersonQuery query,
        CancellationToken ct)
    {
        var identityUserId = _currentUser.UserId;
        if (identityUserId is null)
            return Result<PersonDto>.Failure("Authenticated user identifier is missing.");

        var person = await LoadPersonAsync(identityUserId.Value, ct);
        if (person is null)
            return Result<PersonDto>.Failure("Person profile not found.");

        return Result<PersonDto>.Success(MapToDto(person));
    }

    private async Task<Person?> LoadPersonAsync(Guid identityUserId, CancellationToken ct)
    {
        return await _personRepository.GetByIdentityUserIdAsync(identityUserId, ct);
    }

    private static PersonDto MapToDto(Person person) => new()
    {
        Id = person.Id,
        IdentityUserId = person.IdentityUserId,
        FirstName = person.Name.FirstName,
        SecondName = person.Name.SecondName,
        ThirdName = person.Name.ThirdName,
        LastName = person.Name.LastName,
        DateOfBirth = person.DateOfBirth,
        Gender = person.Gender.ToString(),
        PhoneNumber = person.PhoneNumber.Value,
        Email = person.Email.Value,
        HomeAddress = MapAddress(person),
        PersonalImageFileId = person.PersonalImage?.FileId
    };

    private static AddressDto? MapAddress(Person person)
    {
        if (person.HomeAddress is null)
            return null;

        return new AddressDto
        {
            Street = person.HomeAddress.Street,
            City = person.HomeAddress.City,
            Type = person.HomeAddress.Type,
            LocationMapUrl = person.HomeAddress.LocationMapUrl
        };
    }
}