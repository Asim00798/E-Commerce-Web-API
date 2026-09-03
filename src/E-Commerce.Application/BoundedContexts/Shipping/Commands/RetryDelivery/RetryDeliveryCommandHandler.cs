using E_Commerce.Application.BoundedContexts.Shipping.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using Microsoft.Extensions.Options;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.RetryDelivery;

public sealed class RetryDeliveryCommandHandler
    : IRequestHandler<RetryDeliveryCommand, Result>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IOptions<ShippingOptions> _options;

    public RetryDeliveryCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IOptions<ShippingOptions> options)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _options = options;
    }

    public async Task<Result> Handle(
        RetryDeliveryCommand command,
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

            if (shipment.AssignedDriverId != userId.Value)
                return Result.Failure("Driver is not assigned to this shipment.");

            var maxAttempts = _options.Value.MaximumDeliveryAttempts;

            shipment.Retry(maxAttempts);
            
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