using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.Credentials.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly ICredentialManagement _credentialManagement;
    private readonly IAccountReader _accountReader;
    private readonly ICurrentUser _currentUser;

    public ChangePasswordCommandHandler(
        ICredentialManagement credentialManagement,
        IAccountReader accountReader,
        ICurrentUser currentUser)
    {
        _credentialManagement = credentialManagement;
        _accountReader = accountReader;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Failure("Unauthorized.");

        var security = await _accountReader.GetSecurityAsync(userId.Value, ct);
        if (security is null || security.AccountStatus != AccountStatus.Active)
            return Result.Failure("Account is not active.");

        await _credentialManagement.ChangePasswordAsync(
            userId.Value,
            command.CurrentPassword,
            command.NewPassword,
            ct);

        return Result.Success();
    }
}