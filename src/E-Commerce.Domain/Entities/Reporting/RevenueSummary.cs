using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Reporting
{
    public class RevenueSummary : BaseEntity
    {
        public DateTimeOffset PeriodStart { get; set; }
        public DateTimeOffset PeriodEnd { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal NetRevenue => TotalRevenue - TotalRefunds;

        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (PeriodEnd < PeriodStart)
                throw new InvalidOperationException("RevenueSummary period end must be after start.");

            if (TotalRevenue < 0 || TotalRefunds < 0 || TotalOrders < 0 || TotalCustomers < 0)
                throw new InvalidOperationException("RevenueSummary metrics cannot be negative.");
        }
    }
}
