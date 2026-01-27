using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Helpers
{
    public static class DeliveryTimeCalculator
    {
        public static TimeSpan Calculate(string? city, DeliveryMethod? deliveryMethod)
        {
            if (string.IsNullOrWhiteSpace(city) || deliveryMethod is null)
                return TimeSpan.FromHours(48); // Default fallback

            return city.ToLower() switch
            {
                "cairo" => deliveryMethod switch
                {
                    DeliveryMethod.Standard => TimeSpan.FromHours(24),
                    DeliveryMethod.Express => TimeSpan.FromHours(8),
                    DeliveryMethod.SameDay => TimeSpan.FromHours(4),
                    DeliveryMethod.Scheduled => TimeSpan.FromHours(48),
                    _ => TimeSpan.FromHours(24)
                },

                "alexandria" => deliveryMethod switch
                {
                    DeliveryMethod.Standard => TimeSpan.FromHours(36),
                    DeliveryMethod.Express => TimeSpan.FromHours(12),
                    DeliveryMethod.SameDay => TimeSpan.FromHours(8),
                    DeliveryMethod.Scheduled => TimeSpan.FromHours(60),
                    _ => TimeSpan.FromHours(36)
                },

                "menofia" => deliveryMethod switch
                {
                    DeliveryMethod.Standard => TimeSpan.FromHours(28),
                    DeliveryMethod.Express => TimeSpan.FromHours(10),
                    DeliveryMethod.SameDay => TimeSpan.FromHours(6),
                    DeliveryMethod.Scheduled => TimeSpan.FromHours(54),
                    _ => TimeSpan.FromHours(30)
                },

                _ => deliveryMethod switch
                {
                    DeliveryMethod.Standard => TimeSpan.FromHours(48),
                    DeliveryMethod.Express => TimeSpan.FromHours(24),
                    DeliveryMethod.SameDay => TimeSpan.FromHours(12),
                    DeliveryMethod.Scheduled => TimeSpan.FromHours(72),
                    _ => TimeSpan.FromHours(48)
                }
            };
        }
    }
}
