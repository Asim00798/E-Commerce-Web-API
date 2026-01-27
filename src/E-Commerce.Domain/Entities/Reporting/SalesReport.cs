using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Reporting
{
    public class SalesReport : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int QuantitySold { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0m;
        public DateTimeOffset ReportDate { get; set; } = DateTimeOffset.UtcNow;

        public override void Validate()
        {
            base.Validate();

            if (ProductId == Guid.Empty)
                throw new InvalidOperationException("SalesReport must be linked to a Product.");

            if (QuantitySold < 0 || TotalRevenue < 0)
                throw new InvalidOperationException("SalesReport metrics cannot be negative.");
        }
    }
}
