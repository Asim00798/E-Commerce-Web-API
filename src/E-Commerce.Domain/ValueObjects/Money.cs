using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "USD")
        {
            if (amount < 0)
                throw new BusinessRuleViolationException("Amount cannot be negative");

            if (string.IsNullOrWhiteSpace(currency))
                throw new BusinessRuleViolationException("Currency cannot be empty");

            Amount = amount;
            Currency = currency.ToUpper();
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new BusinessRuleViolationException("Cannot add money with different currencies");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new BusinessRuleViolationException("Cannot subtract money with different currencies");

            if (Amount - other.Amount < 0)
                throw new BusinessRuleViolationException("Resulting amount cannot be negative");

            return new Money(Amount - other.Amount, Currency);
        }

    }
}
