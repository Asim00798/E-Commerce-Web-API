using E_Commerce.ReadModel.Features.Reports.Dtos;
using MediatR;

namespace E_Commerce.ReadModel.Features.Reports.Queries;

/// <summary>
/// Cross-context query to generate a monthly sales report.
/// </summary>
public sealed record GetMonthlySalesReportQuery(int Year, int Month) : IRequest<SalesReportDto>;
