namespace E_Commerce.ReadModel.Features.Invoices.Dtos;

public class InvoiceListDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!; // "Paid", "Unpaid", "Overdue"
    public DateTime IssuedDate { get; set; }
}
