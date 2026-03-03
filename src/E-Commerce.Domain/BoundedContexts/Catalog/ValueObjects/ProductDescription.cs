
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects
{
    public sealed record ProductDescription
    {
        public string Name { get; init; }
        public string? ShortDescription { get; init; }
        public string? LongDescription { get; init; }
        public Dimension? Dimensions { get; init; }
        public Weight? Weight { get; init; }
        public DateTimeOffset? DateOfManufacture { get; init; }
        public DateTimeOffset? DateOfExpiry { get; init; }
        public string? Material { get; init; }
        public string? Color { get; init; }

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
            Name = ValidateName(name);
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Dimensions = dimensions;
            Weight = weight;
            DateOfManufacture = ValidateManufactureDate(dateOfManufacture);
            DateOfExpiry = dateOfExpiry;
            Material = material;
            Color = color;
        }

        // ======================
        // Immutable "With" methods
        // ======================
        public ProductDescription WithName(string name) =>
            this with { Name = ValidateName(name) };

        public ProductDescription WithShortDescription(string? shortDesc) =>
            this with { ShortDescription = shortDesc };

        public ProductDescription WithLongDescription(string? longDesc) =>
            this with { LongDescription = longDesc };

        public ProductDescription WithDimensions(Dimension? dimensions) =>
            this with { Dimensions = dimensions };

        public ProductDescription WithWeight(Weight? weight) =>
            this with { Weight = weight };

        public ProductDescription WithDateOfManufacture(DateTimeOffset? date) =>
            this with { DateOfManufacture = ValidateManufactureDate(date) };

        public ProductDescription WithDateOfExpiry(DateTimeOffset? date) =>
            this with { DateOfExpiry = date };

        public ProductDescription WithMaterial(string? material) =>
            this with { Material = material };

        public ProductDescription WithColor(string? color) =>
            this with { Color = color };

        // ======================
        // Validation helpers
        // ======================
        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Product name cannot be empty.");
            return name.Trim();
        }

        private static DateTimeOffset? ValidateManufactureDate(DateTimeOffset? date)
        {
            if (date.HasValue && date.Value.Year < 1800)
                throw new BusinessRuleViolationException("Year of manufacture seems invalid.");
            return date;
        }

        public override string ToString() =>
            $"{Name} ({Material ?? "N/A"}, {Color ?? "N/A"})";
    }
}
