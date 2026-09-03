using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.Credentials.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string ResetToken,
    string NewPassword) : IRequest<Result>;