using E_Commerce.ReadModel.Features.Reports.Dtos;
using E_Commerce.ReadModel.Features.Reports.Queries;
using MediatR;

namespace E_Commerce.ReadModel.Features.Reports.Handlers;

/// <summary>
/// Handles <see cref="GetMonthlySalesReportQuery"/> by aggregating cross-context data.
/// </summary>
public sealed class GetMonthlySalesReportHandler : IRequestHandler<GetMonthlySalesReportQuery, SalesReportDto>
{
    public Task<SalesReportDto> Handle(GetMonthlySalesReportQuery query, CancellationToken cancellationToken)
    {
        // TODO: Implement cross-context aggregation logic
        throw new NotImplementedException();
    }
}
