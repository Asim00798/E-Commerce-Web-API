using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Features.Reports.Dtos;

namespace E_Commerce.ReadModel.Features.Reports.Queries;

public record GetMonthlySalesReportQuery(int Year, int Month) : IQuery<SalesReportDto>;
