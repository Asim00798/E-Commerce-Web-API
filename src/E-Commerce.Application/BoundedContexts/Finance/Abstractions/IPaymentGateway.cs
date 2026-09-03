using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Application.BoundedContexts.Finance.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentInitiationResult> InitiatePaymentAsync(
        PaymentInitiationRequest request,
        CancellationToken ct = default);

    Task<PaymentStatusResult> GetPaymentStatusAsync(
        PaymentProviderReference reference,
        CancellationToken ct = default);

    Task<RefundResult> RefundAsync(
        PaymentProviderReference reference,
        Money amount,
        CancellationToken ct = default);

    Task<RefundStatusResult> GetRefundStatusAsync(
    PaymentProviderReference reference,
    CancellationToken ct = default);
}