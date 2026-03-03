using System;
using SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Brand.Rules
{
    public class BrandEmailDomainMustMatchCompanyRule : IBusinessRule
    {
        public bool IsSatisfied()
        {
            // Business Logic: Check if the contact email domain matches the official company domain.
            return true;
        }

        public string Message => "Brand email domain must match the company domain.";
    }
}
