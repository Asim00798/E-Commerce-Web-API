using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.SharedKernel.ValueObjects
{
    public sealed record Money
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; }

        public Money(decimal amount, string currency = "USD")
        {
            Amount = ValidateAmount(amount);
            Currency = ValidateCurrency(currency);
        }

        // ======================
        // Immutable operations
        // ======================
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            if (Amount - other.Amount < 0)
                throw new BusinessRuleViolationException("Resulting amount cannot be negative");

            return new Money(Amount - other.Amount, Currency);
        }

        public Money WithAmount(decimal amount) => new Money(ValidateAmount(amount), Currency);
        public Money WithCurrency(string currency) => new Money(Amount, ValidateCurrency(currency));

        // ======================
        // Validation helpers
        // ======================
        private static decimal ValidateAmount(decimal amount)
        {
            if (amount < 0)
                throw new BusinessRuleViolationException("Amount cannot be negative");
            return amount;
        }

        private static string ValidateCurrency(string? currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new BusinessRuleViolationException("Currency cannot be empty");
            return currency.Trim().ToUpper();
        }

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new BusinessRuleViolationException("Cannot operate on money with different currencies");
        }

        public override string ToString() => $"{Currency} {Amount:N2}";
    }
}
