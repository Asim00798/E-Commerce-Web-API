namespace E_Commerce.ReadModel.Features.Reports.Dtos;

public class SalesReportDto
{
    public string Month { get; set; } = default!;
    public decimal TotalSales { get; set; }
    public int InvoiceCount { get; set; }
}
