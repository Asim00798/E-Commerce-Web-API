using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.ActivateAccount;

public sealed record ActivateAccountCommand(Guid UserId) : IRequest<Result>;