using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.Credentials.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : IRequest<Result>;