using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.DeleteMyPerson;

public sealed class DeleteMyPersonCommandHandler
    : IRequestHandler<DeleteMyPersonCommand, Result>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMyPersonCommandHandler(
        IPersonRepository personRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _personRepository = personRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteMyPersonCommand command,
        CancellationToken ct)
    {
        var identityUserId = _currentUser.UserId;
        if (identityUserId is null)
            return Result.Failure("Authenticated user identifier is missing.");

        var person = await LoadPersonAsync(identityUserId.Value, ct);
        if (person is null)
            return Result.Failure("Person profile not found.");

        await DeleteAsync(person, ct);

        return Result.Success();
    }

    private async Task<Person?> LoadPersonAsync(Guid identityUserId, CancellationToken ct)
    {
        return await _personRepository.GetByIdentityUserIdAsync(identityUserId, ct);
    }

    private async Task DeleteAsync(Person person, CancellationToken ct)
    {
        _personRepository.Remove(person);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}