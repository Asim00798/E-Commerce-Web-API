namespace E_Commerce.ReadModel.Features.Invoices.Projections;

public class InvoiceProjection
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPaid { get; set; }
}
