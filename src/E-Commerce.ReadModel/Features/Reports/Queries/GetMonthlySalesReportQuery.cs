using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Features.Reports.Dtos;

namespace E_Commerce.ReadModel.Features.Reports.Queries;

/// <summary>
/// Cross-context query to generate a monthly sales report.
/// </summary>
public sealed record GetMonthlySalesReportQuery(int Year, int Month) : IQuery<SalesReportDto>;
