using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.Events.Finance.Discount;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Discount
{
    public class Discount : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public Money Amount { get; private set; }
        public bool IsPercentage { get; private set; } = false;
        public bool IsActive { get; private set; } = true;

        // Optional target
        public Guid? ProductId { get; private set; }
        public Guid? CategoryId { get; private set; }

        // DDD Constructor
        public Discount(string name, decimal amount, bool isPercentage, Guid? productId = null, Guid? categoryId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Discount name cannot be empty.");

            if (amount <= 0)
                throw new BusinessRuleViolationException("Discount amount must be greater than zero.");

            Name = name;
            Amount = new Money(amount);
            IsPercentage = isPercentage;
            ProductId = productId;
            CategoryId = categoryId;
            IsActive = true;

            AddDomainEvent(new DiscountCreated(Id));
        }

        public void Activate()
        {
            if (IsActive) return;

            IsActive = true;
            AddDomainEvent(new DiscountActivated(Id));
        }

        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;
            AddDomainEvent(new DiscountDeactivated(Id));
        }

        public void Apply()
        {
            if (!IsActive)
                throw new BusinessRuleViolationException("Discount is not active.");

            AddDomainEvent(new DiscountApplied(Id));
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Discount name cannot be empty.");

            if (Amount.Amount <= 0)
                throw new InvalidOperationException("Discount amount must be greater than zero.");
        }
    }
}
