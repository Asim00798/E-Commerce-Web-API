using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcileRefunds;

public sealed record ReconcileRefundsCommand(int BatchSize = 50) : IRequest<Result>;