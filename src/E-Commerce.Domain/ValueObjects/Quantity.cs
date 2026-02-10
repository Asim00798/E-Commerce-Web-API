using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Quantity
    {
        public int Value { get; }

        public Quantity(int value)
        {
            if (value <= 0)
                throw new BusinessRuleViolationException("Quantity must be greater than zero");

            Value = value;
        }

        public Quantity Add(Quantity other)
        {
            return new Quantity(Value + other.Value);
        }

        public Quantity Subtract(Quantity other)
        {
            if (Value - other.Value <= 0)
                throw new BusinessRuleViolationException("Resulting quantity must be greater than zero");

            return new Quantity(Value - other.Value);
        }

    }
}
