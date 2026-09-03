using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Queries.GetAccountById;

public sealed class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly IAccountReader _accountReader;
    private readonly IPermissionService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public GetAccountByIdQueryHandler(
        IAccountReader accountReader,
        IPermissionService authorizationService,
        ICurrentUser currentUser)
    {
        _accountReader = accountReader;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<AccountDto>> Handle(GetAccountByIdQuery query, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId is null)
            return Result<AccountDto>.Failure("Unauthorized.");

        var authorized = await _authorizationService.HasPermissionAsync(
            currentUserId.Value,
            AccountPermissions.Read);

        if (!authorized)
            return Result<AccountDto>.Failure("Forbidden.");

        var account = await _accountReader.GetByIdAsync(query.UserId, ct);
        if (account is null)
            return Result<AccountDto>.Failure("Account not found.");

        return Result<AccountDto>.Success(account);
    }
}