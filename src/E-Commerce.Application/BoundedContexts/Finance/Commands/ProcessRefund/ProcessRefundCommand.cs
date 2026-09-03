using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ProcessRefund;

public sealed record ProcessRefundCommand(Guid RefundId) : IRequest<Result>;