using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Weight
    {
        public decimal Kilograms { get; }

        public Weight(decimal kilograms)
        {
            if (kilograms <= 0)
                throw new BusinessRuleViolationException("Weight must be positive.");

            Kilograms = kilograms;
        }
    }
}
