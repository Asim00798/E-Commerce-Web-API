using E_Commerce.Application.BoundedContexts.Finance.Models;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.HandlePaymentWebhook;

public sealed record HandlePaymentWebhookCommand(
    string Provider,
    string ProviderTransactionId,
    string? ProviderIntentionId,
    bool Success,
    string? Message) : IRequest<PaymentWebhookCommandResult>;