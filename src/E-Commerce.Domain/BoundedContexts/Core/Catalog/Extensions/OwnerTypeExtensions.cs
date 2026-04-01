using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.Extensions
{
    public static class OwnerTypeExtensions
    {
        /// <summary>
        /// Gets the bounded context name for this owner type
        /// </summary>
        public static string GetBoundedContext(this OwnerType ownerType)
        {
            return ownerType switch
            {
                OwnerType.Brand => "Catalog",
                OwnerType.Product => "Catalog",
                OwnerType.Category => "Catalog",
                OwnerType.Review => "Catalog",

                OwnerType.Customer => "Identity",
                OwnerType.User => "Identity",

                OwnerType.Employee => "HR",

                OwnerType.Supplier => "Procurement",

                OwnerType.Order => "Ordering",
                OwnerType.Return => "Ordering",

                OwnerType.Invoice => "Billing",
                OwnerType.Payment => "Billing",

                OwnerType.Contract => "Legal",
                OwnerType.Company => "Administration",
                OwnerType.Project => "ProjectManagement",
                OwnerType.SupportTicket => "Support",
                OwnerType.Shipment => "Logistics",
                OwnerType.Claim => "Insurance",
                OwnerType.Warranty => "Product",
                OwnerType.License => "Compliance",

                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets the display name for UI
        /// </summary>
        public static string GetDisplayName(this OwnerType ownerType)
        {
            return ownerType switch
            {
                OwnerType.SupportTicket => "Support Ticket",
                _ => ownerType.ToString()
            };
        }

        /// <summary>
        /// Checks if this owner type belongs to a specific bounded context
        /// </summary>
        public static bool BelongsToContext(this OwnerType ownerType, string contextName)
        {
            return ownerType.GetBoundedContext().Equals(contextName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets all owner types for a specific bounded context
        /// </summary>
        public static IEnumerable<OwnerType> GetByContext(string contextName)
        {
            return Enum.GetValues<OwnerType>()
                .Where(ot => ot.GetBoundedContext().Equals(contextName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
