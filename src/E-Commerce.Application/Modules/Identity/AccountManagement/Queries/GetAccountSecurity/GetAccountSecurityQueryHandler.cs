using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Queries.GetAccountSecurity;

public sealed class GetAccountSecurityQueryHandler : IRequestHandler<GetAccountSecurityQuery, Result<AccountSecurityDto>>
{
    private readonly IAccountReader _accountReader;
    private readonly ICurrentUser _currentUser;

    public GetAccountSecurityQueryHandler(
        IAccountReader accountReader,
        ICurrentUser currentUser)
    {
        _accountReader = accountReader;
        _currentUser = currentUser;
    }

    public async Task<Result<AccountSecurityDto>> Handle(GetAccountSecurityQuery query, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<AccountSecurityDto>.Failure("Unauthorized.");

        var security = await _accountReader.GetSecurityAsync(userId.Value, ct);
        if (security is null)
            return Result<AccountSecurityDto>.Failure("Account not found.");

        return Result<AccountSecurityDto>.Success(security);
    }
}