using E_Commerce.Application.Shared.Communication.Notifications.Services;
using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Commands.DeactivatePushDevice;

public sealed class DeactivatePushDeviceCommandHandler : IRequestHandler<DeactivatePushDeviceCommand>
{
    private readonly IPushDeviceRegistrationService _registrationService;

    public DeactivatePushDeviceCommandHandler(IPushDeviceRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task Handle(DeactivatePushDeviceCommand command, CancellationToken cancellationToken)
    {
        await _registrationService.DeactivateAsync(command.DeviceId, cancellationToken);
    }
}