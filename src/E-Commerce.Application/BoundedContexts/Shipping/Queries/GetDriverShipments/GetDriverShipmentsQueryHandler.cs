using E_Commerce.Application.BoundedContexts.Shipping.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Queries.GetDriverShipments;

public sealed class GetDriverShipmentsQueryHandler
    : IRequestHandler<GetDriverShipmentsQuery, Result<IReadOnlyList<ShipmentDto>>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICurrentUser _currentUser;

    public GetDriverShipmentsQueryHandler(
        IShipmentRepository shipmentRepository,
        ICurrentUser currentUser)
    {
        _shipmentRepository = shipmentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ShipmentDto>>> Handle(
        GetDriverShipmentsQuery query,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<IReadOnlyList<ShipmentDto>>.Failure("User is not authenticated.");

        // Resource authorization: drivers can only access their own assigned shipments
        if (query.DriverId != userId.Value)
            return Result<IReadOnlyList<ShipmentDto>>.Failure("User is not authorized to view these shipments.");

        var shipments = await _shipmentRepository.GetDriverShipmentsAsync(query.DriverId, ct);

        var dtos = shipments.Select(MapToDto).ToList();
        return Result<IReadOnlyList<ShipmentDto>>.Success(dtos);
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