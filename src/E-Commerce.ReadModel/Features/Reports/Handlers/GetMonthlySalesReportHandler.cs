using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Features.Reports.Dtos;
using E_Commerce.ReadModel.Features.Reports.Queries;

namespace E_Commerce.ReadModel.Features.Reports.Handlers;

/// <summary>
/// Handles <see cref="GetMonthlySalesReportQuery"/> by aggregating cross-context data.
/// </summary>
public sealed class GetMonthlySalesReportHandler : IQueryHandler<GetMonthlySalesReportQuery, SalesReportDto>
{
    public Task<SalesReportDto> HandleAsync(GetMonthlySalesReportQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement cross-context aggregation logic
        throw new NotImplementedException();
    }
}
