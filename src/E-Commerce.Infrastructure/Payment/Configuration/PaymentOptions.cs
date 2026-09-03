using E_Commerce.Infrastructure.Payment.Providers.Paymob;

namespace E_Commerce.Infrastructure.Payment.Configuration;

public sealed class PaymentOptions
{
    public const string SectionName = "Payment";

    public string Provider { get; set; } = "Paymob";

    public PaymobOptions Paymob { get; set; } = new();
}