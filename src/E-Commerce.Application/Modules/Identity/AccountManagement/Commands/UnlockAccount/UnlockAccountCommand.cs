using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.UnlockAccount;

public sealed record UnlockAccountCommand(Guid UserId) : IRequest<Result>;