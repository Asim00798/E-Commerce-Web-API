using E_Commerce.Application.Modules.Notifications.Models;
using E_Commerce.Application.Shared.Communication.Notifications.Services;
using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Commands.RegisterPushDevice;

public sealed class RegisterPushDeviceCommandHandler : IRequestHandler<RegisterPushDeviceCommand, Guid>
{
    private readonly IPushDeviceRegistrationService _registrationService;

    public RegisterPushDeviceCommandHandler(IPushDeviceRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<Guid> Handle(RegisterPushDeviceCommand command, CancellationToken cancellationToken)
    {
        var model = new RegisterPushDeviceModel
        {
            UserId = command.UserId,
            FirebaseInstallationId = command.FirebaseInstallationId,
            Platform = command.Platform
        };

        return await _registrationService.RegisterAsync(model, cancellationToken);
    }
}