using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Features.Invoices.Dtos;

namespace E_Commerce.ReadModel.Features.Invoices.Queries;

public record GetInvoiceByIdQuery(Guid Id) : IQuery<InvoiceListDto?>;
