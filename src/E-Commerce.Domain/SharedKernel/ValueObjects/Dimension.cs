using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.SharedKernel.ValueObjects
{
    public sealed record Dimension
    {
        public decimal Length { get; init; }
        public decimal Width { get; init; }
        public decimal Height { get; init; }

        public Dimension(decimal length, decimal width, decimal height)
        {
            Length = ValidateDimension(length, nameof(length));
            Width = ValidateDimension(width, nameof(width));
            Height = ValidateDimension(height, nameof(height));
        }

        // ======================
        // "With" methods for immutability + validation
        // ======================

        public Dimension WithLength(decimal length) =>
            this with { Length = ValidateDimension(length, nameof(length)) };

        public Dimension WithWidth(decimal width) =>
            this with { Width = ValidateDimension(width, nameof(width)) };

        public Dimension WithHeight(decimal height) =>
            this with { Height = ValidateDimension(height, nameof(height)) };

        // ======================
        // Validation helper
        // ======================

        private static decimal ValidateDimension(decimal value, string propertyName)
        {
            if (value <= 0)
                throw new BusinessRuleViolationException($"{propertyName} must be a positive number.");
            return value;
        }

        public override string ToString() =>
            $"L:{Length} x W:{Width} x H:{Height}";
    }
}
