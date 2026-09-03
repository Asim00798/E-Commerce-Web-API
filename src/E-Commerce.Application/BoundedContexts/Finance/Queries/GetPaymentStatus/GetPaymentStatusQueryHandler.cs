using E_Commerce.Application.BoundedContexts.Finance.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Queries.GetPaymentStatus;

public sealed class GetPaymentStatusQueryHandler
    : IRequestHandler<GetPaymentStatusQuery, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentStatusQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PaymentDto>> Handle(
        GetPaymentStatusQuery query,
        CancellationToken ct)
    {
        var payment = await _paymentRepository.GetByIdAsync(query.PaymentId, ct);

        if (payment is null)
        {
            return Result<PaymentDto>.Failure("Payment not found.");
        }

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