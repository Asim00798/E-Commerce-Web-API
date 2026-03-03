namespace E_Commerce.Domain.SharedKernel.Services.Pricing;

public class PricingService : IPricingService
{
    public decimal CalculatePrice(decimal basePrice, decimal discount)
    {
        // Placeholder implementation
        return basePrice - discount;
    }
}
