using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Commands.DeactivatePushDevice;

/// <summary>
/// Deactivates a previously registered push device.
/// </summary>
public record DeactivatePushDeviceCommand(Guid DeviceId) : IRequest;