using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Common.Paging;
using E_Commerce.ReadModel.Features.Invoices.Dtos;

namespace E_Commerce.ReadModel.Features.Invoices.Queries;

public record GetUnpaidInvoicesQuery(PagingRequest Paging) : IQuery<IPagedResult<InvoiceListDto>>;
