using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Features.Invoices.Projections;
using E_Commerce.ReadModel.Features.Reports.Dtos;
using E_Commerce.ReadModel.Features.Reports.Queries;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace E_Commerce.ReadModel.Features.Reports.Handlers;

public class GetMonthlySalesReportHandler : IQueryHandler<GetMonthlySalesReportQuery, SalesReportDto>
{
    private readonly IReadDbContext _context;

    public GetMonthlySalesReportHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReportDto> Handle(GetMonthlySalesReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var data = await _context.Set<InvoiceProjection>()
            .Where(x => x.IssuedDate >= startDate && x.IssuedDate < endDate)
            .GroupBy(x => 1)
            .Select(g => new 
            {
                TotalSales = g.Sum(x => x.TotalAmount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new SalesReportDto
        {
            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(request.Month),
            TotalSales = data?.TotalSales ?? 0,
            InvoiceCount = data?.Count ?? 0
        };
    }
}
