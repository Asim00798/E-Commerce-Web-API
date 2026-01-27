using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Helpers
{
    public static class DeliveryCostCalculator
    {
        public static decimal Calculate(string? city, DeliveryMethod? deliveryMethod)
        {
            if (string.IsNullOrWhiteSpace(city) || deliveryMethod is null)
                return 30m; // Default fallback cost

            return city.ToLower() switch
            {
                "cairo" => deliveryMethod switch
                {
                    DeliveryMethod.Standard => 20m,
                    DeliveryMethod.Express => 50m,
                    DeliveryMethod.SameDay => 70m,
                    DeliveryMethod.Scheduled => 40m,
                    _ => 30m
                },

                "alexandria" => deliveryMethod switch
                {
                    DeliveryMethod.Standard => 25m,
                    DeliveryMethod.Express => 55m,
                    DeliveryMethod.SameDay => 75m,
                    DeliveryMethod.Scheduled => 45m,
                    _ => 35m
                },

                "menofia" => deliveryMethod switch
                {
                    DeliveryMethod.Standard => 22m,
                    DeliveryMethod.Express => 52m,
                    DeliveryMethod.SameDay => 72m,
                    DeliveryMethod.Scheduled => 42m,
                    _ => 32m
                },

                _ => deliveryMethod switch
                {
                    DeliveryMethod.Standard => 40m,
                    DeliveryMethod.Express => 70m,
                    DeliveryMethod.SameDay => 90m,
                    DeliveryMethod.Scheduled => 60m,
                    _ => 50m
                }
            };
        }
    }
}
