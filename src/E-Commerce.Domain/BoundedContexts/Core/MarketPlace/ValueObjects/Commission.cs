#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.ValueObjects
{
    public sealed record Commission
    {
        public decimal Percentage { get; init; }
        public decimal FlatFee { get; init; }

        public Commission(decimal percentage, decimal flatFee = 0)
        {
            Percentage = percentage;
            FlatFee = flatFee;
        }

        public decimal Calculate(decimal amount) => (amount * (Percentage / 100)) + FlatFee;
    }
}

#endif