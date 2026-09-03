using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.MarkPickedUp;

public sealed class MarkPickedUpCommandHandler
    : IRequestHandler<MarkPickedUpCommand, Result>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public MarkPickedUpCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        MarkPickedUpCommand command,
        CancellationToken ct)
    {
        try
        {
            var userId = _currentUser.UserId;
            if (userId is null)
                return Result.Failure("User is not authenticated.");

            var shipment = await _shipmentRepository.GetByIdAsync(command.ShipmentId, ct);
            if (shipment is null)
                return Result.Failure("Shipment not found.");

            // Resource authorization: only assigned driver can pick up
            if (shipment.AssignedDriverId != userId.Value)
                return Result.Failure("Driver is not assigned to this shipment.");

            shipment.MarkPickedUp();

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