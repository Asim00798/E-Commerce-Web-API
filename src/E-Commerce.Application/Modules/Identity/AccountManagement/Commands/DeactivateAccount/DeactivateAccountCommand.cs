using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.DeactivateAccount;

public sealed record DeactivateAccountCommand(Guid UserId) : IRequest<Result>;