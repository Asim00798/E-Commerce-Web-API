using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Queries.GetCurrentAccount;

public sealed class GetCurrentAccountQueryHandler : IRequestHandler<GetCurrentAccountQuery, Result<AccountDto>>
{
    private readonly IAccountReader _accountReader;
    private readonly ICurrentUser _currentUser;

    public GetCurrentAccountQueryHandler(
        IAccountReader accountReader,
        ICurrentUser currentUser)
    {
        _accountReader = accountReader;
        _currentUser = currentUser;
    }

    public async Task<Result<AccountDto>> Handle(GetCurrentAccountQuery query, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<AccountDto>.Failure("Unauthorized.");

        var account = await _accountReader.GetByIdAsync(userId.Value, ct);
        if (account is null)
            return Result<AccountDto>.Failure("Account not found.");

        return Result<AccountDto>.Success(account);
    }
}