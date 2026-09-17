using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.ValueObjects;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.UpdateMyPerson;

public sealed class UpdateMyPersonCommandHandler
    : IRequestHandler<UpdateMyPersonCommand, Result>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyPersonCommandHandler(
        IPersonRepository personRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _personRepository = personRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateMyPersonCommand command,
        CancellationToken ct)
    {
        var identityUserId = _currentUser.UserId;
        if (identityUserId is null)
            return Result.Failure("Authenticated user identifier is missing.");

        var person = await LoadPersonAsync(identityUserId.Value, ct);
        if (person is null)
            return Result.Failure("Person profile not found.");

        try
        {
            ApplyChanges(person, command);
            await PersistAsync(person, ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Person?> LoadPersonAsync(Guid identityUserId, CancellationToken ct)
    {
        return await _personRepository.GetByIdentityUserIdAsync(identityUserId, ct);
    }

    private static void ApplyChanges(Person person, UpdateMyPersonCommand command)
    {
        person.UpdatePhoneNumber(new PhoneNumber(command.PhoneNumber));
        person.UpdateEmail(new Email(command.Email));

        person.UpdateAddress(new Address(
            command.HomeAddress.Street,
            command.HomeAddress.City,
            command.HomeAddress.Type,
            command.HomeAddress.LocationMapUrl));
    }

    private async Task PersistAsync(Person person, CancellationToken ct)
    {
        await _personRepository.UpdateAsync(person, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}