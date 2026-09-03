using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.UpdateAccount;

public sealed class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Result>
{
    private readonly IAccountManagement _accountManagement;
    private readonly IAccountReader _accountReader;
    private readonly ICurrentUser _currentUser;

    public UpdateAccountCommandHandler(
        IAccountManagement accountManagement,
        IAccountReader accountReader,
        ICurrentUser currentUser)
    {
        _accountManagement = accountManagement;
        _accountReader = accountReader;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateAccountCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Failure("Unauthorized.");

        // Ensure the account is active before allowing self-service updates.
        var security = await _accountReader.GetSecurityAsync(userId.Value, ct);
        if (security is null || security.AccountStatus != AccountStatus.Active)
            return Result.Failure("Account is not active.");

        var request = new UpdateAccountRequest
        {
            UserId = userId.Value,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            UserName = command.UserName
        };

        await _accountManagement.UpdateAsync(request, ct);
        return Result.Success();
    }
}