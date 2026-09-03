using E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Queries.GetAccountSecurity;

public sealed record GetAccountSecurityQuery : IRequest<Result<AccountSecurityDto>>;