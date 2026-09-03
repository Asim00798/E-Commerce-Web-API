using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Commands.UpdateAccount;

public sealed record UpdateAccountCommand(
    string? Email,
    string? PhoneNumber,
    string? UserName) : IRequest<Result>;