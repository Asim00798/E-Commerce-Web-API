using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.CancelShipment;

public sealed class CancelShipmentCommandHandler
    : IRequestHandler<CancelShipmentCommand, Result>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CancelShipmentCommand command,
        CancellationToken ct)
    {
        try
        {
            var shipment = await _shipmentRepository.GetActiveByOrderIdAsync(
                command.OrderId,
                ct);

            if (shipment is null)
                return Result.Success(); // Idempotent

            shipment.Cancel();

            await _shipmentRepository.UpdateAsync(shipment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}