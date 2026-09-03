using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.ActivateAccount;

public sealed class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, Result>
{
    private readonly IAccountManagement _accountManagement;
    private readonly IPermissionService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public ActivateAccountCommandHandler(
        IAccountManagement accountManagement,
        IPermissionService authorizationService,
        ICurrentUser currentUser)
    {
        _accountManagement = accountManagement;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ActivateAccountCommand command, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId is null)
            return Result.Failure("Unauthorized.");

        var authorized = await _authorizationService.HasPermissionAsync(
            currentUserId.Value,
            AccountPermissions.Activate);

        if (!authorized)
            return Result.Failure("Forbidden.");

        await _accountManagement.ActivateAsync(command.UserId, ct);
        return Result.Success();
    }
}