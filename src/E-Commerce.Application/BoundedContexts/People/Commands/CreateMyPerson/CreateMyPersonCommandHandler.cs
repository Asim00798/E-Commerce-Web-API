using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.ValueObjects;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.CreateMyPerson;

public sealed class CreateMyPersonCommandHandler
    : IRequestHandler<CreateMyPersonCommand, Result<Guid>>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMyPersonCommandHandler(
        IPersonRepository personRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _personRepository = personRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateMyPersonCommand command,
        CancellationToken ct)
    {
        var identityUserId = _currentUser.UserId;
        if (identityUserId is null)
            return Result<Guid>.Failure("Authenticated user identifier is missing.");

        if (await ProfileExistsAsync(identityUserId.Value, ct))
            return Result<Guid>.Failure("A person profile already exists for this user.");

        try
        {
            var person = BuildPerson(command, identityUserId.Value);
            await PersistAsync(person, ct);

            return Result<Guid>.Success(person.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }

    private async Task<bool> ProfileExistsAsync(Guid identityUserId, CancellationToken ct)
    {
        var existing = await _personRepository
            .GetByIdentityUserIdAsync(identityUserId, ct);

        return existing is not null;
    }

    private static Person BuildPerson(CreateMyPersonCommand command, Guid identityUserId)
    {
        var name = new FullName(
            command.FirstName,
            command.SecondName,
            command.ThirdName,
            command.LastName);

        var phoneNumber = new PhoneNumber(command.PhoneNumber);
        var email = new Email(command.Email);

        var address = new Address(
            command.HomeAddress.Street,
            command.HomeAddress.City,
            command.HomeAddress.Type,
            command.HomeAddress.LocationMapUrl);

        var person = new Person(
            phoneNumber: phoneNumber,
            email: email,
            address: address,
            personalImage: null,
            name: name,
            dateOfBirth: command.DateOfBirth,
            gender: command.Gender);

        person.LinkIdentityUser(identityUserId);

        return person;
    }

    private async Task PersistAsync(Person person, CancellationToken ct)
    {
        await _personRepository.AddAsync(person, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}