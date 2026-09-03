using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Api.DTOs.Payments.Requests;

public sealed class InitiatePaymentRequest
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethodType Method { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
}