using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.LockAccount;

public sealed record LockAccountCommand(Guid UserId, DateTimeOffset? LockoutEnd) : IRequest<Result>;