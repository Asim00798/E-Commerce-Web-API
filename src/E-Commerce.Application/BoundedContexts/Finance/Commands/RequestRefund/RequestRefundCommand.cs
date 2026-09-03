using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.RequestRefund;

public sealed record RequestRefundCommand(
    Guid PaymentId,
    decimal Amount,
    string Currency,
    string Reason) : IRequest<Result<Guid>>;