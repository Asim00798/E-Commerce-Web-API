using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.Credentials.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly ICredentialManagement _credentialManagement;

    public ResetPasswordCommandHandler(ICredentialManagement credentialManagement)
        => _credentialManagement = credentialManagement;

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        await _credentialManagement.ResetPasswordAsync(
            command.UserId,
            command.ResetToken,
            command.NewPassword,
            ct);

        return Result.Success();
    }
}