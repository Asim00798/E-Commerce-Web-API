using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.BeginReturn;

public sealed class BeginReturnCommandHandler
    : IRequestHandler<BeginReturnCommand, Result>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public BeginReturnCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        BeginReturnCommand command,
        CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            if (userId is null)
                return Result.Failure("User is not authenticated.");

            var shipment = await _shipmentRepository.GetByIdAsync(command.ShipmentId, ct);
            if (shipment is null)
                return Result.Failure("Shipment not found.");

            var driverCheck = EnsureDriverAssigned(shipment, userId.Value);
            if (!driverCheck.Succeeded)
                return driverCheck;

            shipment.BeginReturn();

            await _shipmentRepository.UpdateAsync(shipment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private Guid? GetUserId()
    {
        return _currentUser.UserId;
    }

    private static Result EnsureDriverAssigned(
        Shipment shipment,
        Guid userId)
    {
        if (shipment.AssignedDriverId != userId)
            return Result.Failure("Driver is not assigned to this shipment.");

        return Result.Success();
    }

}