using E_Commerce.Application.BoundedContexts.Finance.Models;

namespace E_Commerce.Application.BoundedContexts.Finance.Abstractions;

public interface IPaymentWebhookProcessor
{
    Task<PaymentWebhookCommandResult> ProcessAsync(
        string provider,
        string payload,
        string? signature,
        CancellationToken ct = default);
}