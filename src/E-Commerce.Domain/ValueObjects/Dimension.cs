using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Dimension
    {
        public decimal Length { get; }
        public decimal Width { get; }
        public decimal Height { get; }

        public Dimension(decimal length, decimal width, decimal height)
        {
            if (length <= 0 || width <= 0 || height <= 0)
                throw new BusinessRuleViolationException("All dimensions must be positive numbers.");

            Length = length;
            Width = width;
            Height = height;
        }
    }
}
