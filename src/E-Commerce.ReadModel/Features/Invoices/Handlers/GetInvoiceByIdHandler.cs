using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Features.Invoices.Dtos;
using E_Commerce.ReadModel.Features.Invoices.Projections;
using E_Commerce.ReadModel.Features.Invoices.Queries;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.Features.Invoices.Handlers;

public class GetInvoiceByIdHandler : IQueryHandler<GetInvoiceByIdQuery, InvoiceListDto?>
{
    private readonly IReadDbContext _context;

    public GetInvoiceByIdHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceListDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var projection = await _context.Set<InvoiceProjection>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (projection == null) return null;

        return new InvoiceListDto
        {
            Id = projection.Id,
            InvoiceNumber = projection.InvoiceNumber,
            CustomerName = projection.CustomerName,
            Amount = projection.TotalAmount,
            Status = projection.IsPaid ? "Paid" : (projection.DueDate < DateTime.UtcNow ? "Overdue" : "Unpaid"),
            IssuedDate = projection.IssuedDate
        };
    }
}
