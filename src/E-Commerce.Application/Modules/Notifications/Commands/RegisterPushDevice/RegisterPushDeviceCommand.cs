using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Commands.RegisterPushDevice;

/// <summary>
/// Registers a new push device for the specified user.
/// </summary>
public record RegisterPushDeviceCommand(
    Guid UserId,
    string FirebaseInstallationId,
    string Platform) : IRequest<Guid>;