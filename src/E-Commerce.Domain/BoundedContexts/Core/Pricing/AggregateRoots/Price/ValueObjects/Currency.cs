#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects
{
    public sealed record Currency
    {
        public string Code { get; init; }
        public string Symbol { get; init; }

        public Currency(string code, string symbol = "$")
        {
            Code = code?.ToUpper() ?? "USD";
            Symbol = symbol;
        }

        public static Currency Usd => new("USD", "$");
        public static Currency Eur => new("EUR", "€");
    }
}

#endif