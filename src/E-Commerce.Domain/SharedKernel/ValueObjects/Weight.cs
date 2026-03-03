using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.SharedKernel.ValueObjects
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
