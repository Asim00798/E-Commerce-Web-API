using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Conversion;

public static class PaymobCurrencyConverter
{
    public static long ToMinorUnit(Money money)
    {
        // Paymob expects amounts in the smallest currency unit.
        // Current supported currencies have two decimal places.
        // Future currencies with different minor-unit exponents require expansion.
        return decimal.ToInt64(decimal.Round(money.Amount * 100m, 0, MidpointRounding.AwayFromZero));
    }
}