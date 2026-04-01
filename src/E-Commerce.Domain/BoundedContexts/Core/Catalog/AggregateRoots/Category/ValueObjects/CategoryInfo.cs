using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.ValueObjects
{
    public sealed record CategoryInfo
    {
        public CategoryName Name { get; init; }
        public string? Description { get; init; }

        public CategoryInfo(CategoryName name, string? description = null)
        {
            Name = name;
            Description = ValidateDescription(description);
        }        

        public CategoryInfo UpdateDescription(string? newDescription)
        {
            newDescription = ValidateDescription(newDescription);

            if (newDescription == Description) return this;

            return this with { Description = newDescription };
        }

        // ----------------------
        // Private helper methods
        // ----------------------

        private static string? ValidateDescription(string? description)
        {
            if (description != null && description.Length > 500)
                throw new BusinessRuleViolationException("Description cannot exceed 500 characters.");
            return description?.Trim();
        }
    }
}
