using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.AssignDriver;

public sealed class AssignDriverCommandHandler
    : IRequestHandler<AssignDriverCommand, Result>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignDriverCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        AssignDriverCommand command,
        CancellationToken ct)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(command.ShipmentId, ct);
            if (shipment is null)
                return Result.Failure("Shipment not found.");

            shipment.AssignDriver(command.DriverId);

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