using E_Commerce.Domain.SharedKernel.Exceptions;
using System.Runtime.InteropServices;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.ValueObjects
{
    public sealed record CategoryName
    {
        public string Value { get; private set; }

        public CategoryName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("Category name is required.");

            Value = value.Trim();
        }

        public CategoryName WithName(string value) =>
            this with { Value = ValidateName(value) };

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Category name cannot be empty");
            return name.Trim();
        }

        public override string ToString() => Value;
    }

}
