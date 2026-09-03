using E_Commerce.Application.BoundedContexts.Shipping.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentById;

public sealed class GetShipmentByIdQueryHandler
    : IRequestHandler<GetShipmentByIdQuery, Result<ShipmentDto>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICurrentUser _currentUser;

    public GetShipmentByIdQueryHandler(
        IShipmentRepository shipmentRepository,
        ICurrentUser currentUser)
    {
        _shipmentRepository = shipmentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ShipmentDto>> Handle(
        GetShipmentByIdQuery query,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<ShipmentDto>.Failure("User is not authenticated.");

        var shipment = await _shipmentRepository.GetByIdAsync(query.ShipmentId, ct);
        if (shipment is null)
            return Result<ShipmentDto>.Failure("Shipment not found.");

        // Resource authorization
        var isCustomer = shipment.CustomerId == userId.Value;
        var isDriver = shipment.AssignedDriverId == userId.Value;

        if (!isCustomer && !isDriver)
            return Result<ShipmentDto>.Failure("User is not authorized to view this shipment.");

        var dto = MapToDto(shipment);
        return Result<ShipmentDto>.Success(dto);
    }

    private static ShipmentDto MapToDto(Shipment shipment)
    {
        return new ShipmentDto
        {
            ShipmentId = shipment.Id,
            OrderId = shipment.OrderId,
            CustomerId = shipment.CustomerId,
            Status = shipment.Status.ToString(),
            TrackingNumber = shipment.TrackingNumber,
            AssignedDriverId = shipment.AssignedDriverId,
            FullName = shipment.DeliveryAddress.FullName,
            PhoneNumber = shipment.DeliveryAddress.PhoneNumber,
            Street = shipment.DeliveryAddress.Street,
            City = shipment.DeliveryAddress.City,
            LocationMapUrl = shipment.DeliveryAddress.LocationMapUrl,
            DeliveryAttempts = shipment.DeliveryAttempts
                .Select(x => new DeliveryAttemptDto
                {
                    AttemptNumber = x.AttemptNumber,
                    AttemptedAtUtc = x.AttemptedAtUtc,
                    Result = x.Result.ToString(),
                    FailureReason = x.FailureReason,
                    Notes = x.Notes
                })
                .ToList()
        };
    }
}