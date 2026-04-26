#if false
using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects
{
    public sealed record Money
    {
        public decimal Amount { get; init; }
        public Currency Currency { get; init; }

        public Money(decimal amount, Currency currency)
        {
            if (amount < 0)
                throw new BusinessRuleViolationException("Amount cannot be negative");
            
            Amount = amount;
            Currency = currency;
        }

        public static Money Zero(Currency currency) => new(0, currency);
        
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new BusinessRuleViolationException("Cannot operate on money with different currencies");
        }

        public override string ToString() => $"{Currency.Code} {Amount:N2}";
    }
}

#endif