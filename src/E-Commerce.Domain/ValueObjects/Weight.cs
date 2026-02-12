using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Weight
    {
        public decimal Kilograms { get; init; }

        public Weight(decimal kilograms)
        {
            if (kilograms <= 0)
                throw new BusinessRuleViolationException("Weight must be positive.");

            Kilograms = kilograms;
        }

        // ======================
        // Immutable "With" method
        // ======================
        public Weight WithKilograms(decimal kilograms) => new Weight(kilograms);

        public override string ToString() => $"{Kilograms} kg";
    }
}
