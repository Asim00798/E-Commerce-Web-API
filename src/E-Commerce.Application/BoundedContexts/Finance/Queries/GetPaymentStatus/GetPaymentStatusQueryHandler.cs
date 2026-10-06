using E_Commerce.Application.BoundedContexts.Finance.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Queries.GetPaymentStatus;

public sealed class GetPaymentStatusQueryHandler
    : IRequestHandler<GetPaymentStatusQuery, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentUser _currentUser;

    public GetPaymentStatusQueryHandler(
        IPaymentRepository paymentRepository,
        ICurrentUser currentUser)
    {
        _paymentRepository = paymentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<PaymentDto>> Handle(
        GetPaymentStatusQuery query,
        CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId is null)
            return Result<PaymentDto>.Failure("Authenticated user identifier is missing.");

        var payment = await _paymentRepository.GetByIdAsync(query.PaymentId, ct);

        if (payment is null)
            return Result<PaymentDto>.Failure("Payment not found.");

        var isPrivileged = _currentUser.IsInRole(SystemRoles.Administrator) ||
                           _currentUser.IsInRole(SystemRoles.Support);

        if (!isPrivileged && payment.CustomerId != currentUserId.Value)
            return Result<PaymentDto>.Failure("Payment not found.");   // do not leak existence

        var dto = new PaymentDto
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency,
            Status = payment.Status,
            Provider = payment.Provider,
            ProviderIntentionId = payment.ProviderIntentionId,
            ProviderTransactionId = payment.ProviderTransactionId,
            CompletedAtUtc = payment.CompletedAtUtc,
            RefundedAmount = payment.RefundedAmount.Amount
        };

        return Result<PaymentDto>.Success(dto);
    }
}