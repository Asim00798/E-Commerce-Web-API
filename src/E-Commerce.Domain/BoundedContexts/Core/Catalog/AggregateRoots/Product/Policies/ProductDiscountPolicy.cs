using System;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Product.Policies
{
    public class ProductDiscountPolicy
    {
        public Money ApplyDiscount(Money price, object discount)
        {
            // Business Logic: Apply valid coupon codes, seasonal sales, or bundle discounts to the product's price.
            return price;
        }
    }
}
