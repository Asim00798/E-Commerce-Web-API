using E_Commerce.Domain.SharedKernel.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.ValueObjects
{
    public sealed record CategoryAttributeInfo
    {
        public string Name { get; init; } 
        public string? Description { get; init; }
        public AttributeType Type { get; init; }
        public bool IsRequired { get; private set; }

        public CategoryAttributeInfo(string name, AttributeType type, string? description = null)
        {
            Name = ValidateName(name);
            Type = ValidateType(type);
            Description = ValidateDescription(description);
        }

        internal CategoryAttributeInfo Rename(string name)
            => this with { Name = ValidateName(name) };
        internal CategoryAttributeInfo ChangeType(AttributeType type)
            => this with { Type = ValidateType(type) };
        internal CategoryAttributeInfo UpdateDescription(string? description)
            => this with { Description = ValidateDescription(description) };
        internal CategoryAttributeInfo SetRequired(bool isRequired)
            => this with { IsRequired = isRequired };
        internal CategoryAttributeInfo SetOptional()
            => this with { IsRequired = false };

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Attribute name cannot be empty.");
            return name.Trim();
        }

        private static AttributeType ValidateType(AttributeType type)
        {
            if (!Enum.IsDefined(typeof(AttributeType), type))
                throw new BusinessRuleViolationException("Invalid attribute type.");
            return type;
        }

        private static string? ValidateDescription(string? desc)
        {
            if (desc != null && desc.Length > 300)
                throw new BusinessRuleViolationException("Description too long.");
            return desc?.Trim();
        }

    }
}
