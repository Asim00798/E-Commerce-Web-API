using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcilePayments;

public sealed record ReconcilePaymentsCommand(int BatchSize = 50) : IRequest<Result>;