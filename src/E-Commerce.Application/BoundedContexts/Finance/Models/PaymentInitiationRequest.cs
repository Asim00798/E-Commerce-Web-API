using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Application.BoundedContexts.Finance.Models;

public sealed record PaymentInitiationRequest
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public Money Amount { get; init; } = null!;
    public PaymentMethodType Method { get; init; }
    public string ReturnUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
    public string? IdempotencyKey { get; init; }
}