#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Services
{
    public class CommissionCalculationService
    {
        public decimal CalculateMarketplaceFee(decimal saleAmount, Commission configuration)
        {
            return configuration.Calculate(saleAmount);
        }
    }
}

#endif