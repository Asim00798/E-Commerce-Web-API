using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record ProductDescription
    {
        public string Name { get; }
        public string? ShortDescription { get; }
        public string? LongDescription { get; }
        public Dimension? Dimensions { get; }
        public Weight? Weight { get; }
        public DateTimeOffset? DateOfManufacture { get; }
        public DateTimeOffset? DateOfExpiry { get; }
        public string? Material { get; }
        public string? Color { get; }
            
        public ProductDescription(
            string name,
            string? shortDescription = null,
            string? longDescription = null,
            Dimension? dimensions = null,
            Weight? weight = null,
            DateTimeOffset? dateOfManufacture = null,
            DateTimeOffset? dateOfExpiry = null,
            string? material = null,
            string? color = null
        )
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Product name cannot be empty.");

            if (dateOfManufacture.HasValue && dateOfManufacture.Value.Year < 1800)
                throw new BusinessRuleViolationException("Year of manufacture seems invalid.");

            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Dimensions = dimensions;
            Weight = weight;
            DateOfManufacture = dateOfManufacture;
            DateOfExpiry = dateOfExpiry;
            Material = material;
            Color = color;
        }
    }

}

