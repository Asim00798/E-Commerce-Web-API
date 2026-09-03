using E_Commerce.Application.BoundedContexts.Finance.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Queries.GetPaymentStatus;

public sealed record GetPaymentStatusQuery(Guid PaymentId) : IRequest<Result<PaymentDto>>;