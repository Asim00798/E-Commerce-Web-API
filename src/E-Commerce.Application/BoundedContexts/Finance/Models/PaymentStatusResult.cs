using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Application.BoundedContexts.Finance.Models;

public sealed record PaymentStatusResult
{
    public PaymentStatus Status { get; init; }
    public string? ProviderTransactionId { get; init; }
    public string? ProviderMessage { get; init; }
}