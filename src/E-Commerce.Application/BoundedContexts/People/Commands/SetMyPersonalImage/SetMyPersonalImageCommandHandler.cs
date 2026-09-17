using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.SetMyPersonalImage;

public sealed class SetMyPersonalImageCommandHandler
    : IRequestHandler<SetMyPersonalImageCommand, Result>
{
    private readonly IPersonRepository _personRepository;
    private readonly IFileService _fileService;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public SetMyPersonalImageCommandHandler(
        IPersonRepository personRepository,
        IFileService fileService,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _personRepository = personRepository;
        _fileService = fileService;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetMyPersonalImageCommand command,
        CancellationToken ct)
    {
        var identityUserId = _currentUser.UserId;
        if (identityUserId is null)
            return Result.Failure("Authenticated user identifier is missing.");

        var person = await LoadPersonAsync(identityUserId.Value, ct);
        if (person is null)
            return Result.Failure("Person profile not found.");

        // Upload the new file first. If the subsequent save fails, the file is
        // orphaned and removed by the file storage cleanup job (per the File
        // Storage consistency model — no distributed transaction).
        var fileId = await UploadImageAsync(command, ct);

        try
        {
            person.SetPersonalImage(fileId);
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

    private async Task<Guid> UploadImageAsync(
        SetMyPersonalImageCommand command,
        CancellationToken ct)
    {
        return await _fileService.UploadAsync(
            command.Image.Content,
            command.Image.FileName,
            command.Image.ContentType,
            ct);
    }

    private async Task PersistAsync(Person person, CancellationToken ct)
    {
        await _personRepository.UpdateAsync(person, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}