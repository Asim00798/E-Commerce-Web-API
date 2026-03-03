using AutoMapper;
using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Common.Paging;
using E_Commerce.ReadModel.Features.Invoices.Dtos;
using E_Commerce.ReadModel.Features.Invoices.Projections;
using E_Commerce.ReadModel.Features.Invoices.Queries;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.Features.Invoices.Handlers;

public class GetUnpaidInvoicesHandler : IQueryHandler<GetUnpaidInvoicesQuery, IPagedResult<InvoiceListDto>>
{
    private readonly IReadDbContext _context;

    public GetUnpaidInvoicesHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<IPagedResult<InvoiceListDto>> Handle(GetUnpaidInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<InvoiceProjection>()
            .Where(x => !x.IsPaid)
            .AsNoTracking();

        var mappedQuery = query.Select(x => new InvoiceListDto
        {
            Id = x.Id,
            InvoiceNumber = x.InvoiceNumber,
            CustomerName = x.CustomerName,
            Amount = x.TotalAmount,
            Status = x.DueDate < DateTime.UtcNow ? "Overdue" : "Unpaid",
            IssuedDate = x.IssuedDate
        });

        return await mappedQuery.ToPagedResultAsync(request.Paging.PageNumber, request.Paging.PageSize);
    }
}
