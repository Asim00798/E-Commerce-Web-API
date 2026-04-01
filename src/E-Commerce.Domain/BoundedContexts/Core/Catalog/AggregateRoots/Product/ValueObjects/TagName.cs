using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.ValueObjects
{
    public sealed record TagName
    {
        public string Value { get; init; }

        public TagName(string value)
        {
            Value = ValidateTag(value, "Tag name");
        }

        /// <summary>
        /// Returns a new TagName instance with updated value.
        /// </summary>
        public TagName WithValue(string newValue) =>
            new TagName(ValidateTag(newValue, "Tag name"));

        // ----------------------
        // Private helper
        // ----------------------
        private static string ValidateTag(string value, string tag)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException($"{tag} cannot be empty.");

            if (value.Length > 50)
                throw new BusinessRuleViolationException($"{tag} cannot exceed 50 characters.");

            return value.Trim();
        }

        public override string ToString() => Value;
    }
}
