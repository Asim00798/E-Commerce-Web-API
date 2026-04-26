#if false
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Tax
{
    public class Tax : BaseEntity
    {
        public string Name { get; private set; }
        public decimal Rate { get; private set; } // 0.05 = 5%
        public bool IsActive { get; private set; }

        public Guid? ProductId { get; private set; }
        public Guid? CategoryId { get; private set; }

        public Tax(string name, decimal rate, Guid? productId = null, Guid? categoryId = null)
        {
            Name = ValidateName(name);
            Rate = ValidateRate(rate);
            ProductId = productId;
            CategoryId = categoryId;
            IsActive = true;

            AddDomainEvent(new TaxCreated(Id));
        }

        // -----------------------
        // Behavior
        // -----------------------
        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            AddDomainEvent(new TaxActivated(Id));
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            AddDomainEvent(new TaxDeactivated(Id));
        }

        public void ChangeRate(decimal newRate)
        {
            newRate = ValidateRate(newRate);
            if (Rate == newRate) return;

            Rate = newRate;
            AddDomainEvent(new TaxRateChanged(Id, newRate));
        }

        public void Rename(string newName)
        {
            newName = ValidateName(newName);
            if (Name == newName) return;

            Name = newName;
            AddDomainEvent(new TaxRenamed(Id, newName));
        }

        // -----------------------
        // Validation helpers
        // -----------------------
        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Tax name cannot be empty.");
            return name.Trim();
        }

        private static decimal ValidateRate(decimal rate)
        {
            if (rate < 0 || rate > 1)
                throw new BusinessRuleViolationException("Tax rate must be between 0 and 1.");
            return rate;
        }
    }
}

#endif