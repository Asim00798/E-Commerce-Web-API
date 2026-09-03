using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEventHandlers;

/// <summary>
/// Handles account provisioning when a registration has been fully verified.
/// Creates the ASP.NET Core Identity user and removes the registration.
/// Idempotency is provided by the <see cref="IdempotentIntegrationEventHandler{T}"/> decorator.
/// </summary>
public sealed class CreateUserOnRegistrationVerifiedHandler
    : IIntegrationEventHandler<RegistrationFullyVerifiedIntegrationEvent>
{
    private readonly IRegistrationRepository _registrationRepo;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserOnRegistrationVerifiedHandler> _logger;

    public CreateUserOnRegistrationVerifiedHandler(
        IRegistrationRepository registrationRepo,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ILogger<CreateUserOnRegistrationVerifiedHandler> logger)
    {
        _registrationRepo = registrationRepo;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(
        RegistrationFullyVerifiedIntegrationEvent evt,
        CancellationToken ct)
    {
        var registration = await _registrationRepo.GetByIdAsync(evt.RegistrationId, ct);

        // Already processed or registration no longer exists.
        if (registration is null)
            return;

        // Create the ASP.NET Core Identity user.
        // The password is already hashed and stored in the registration aggregate.
        var createRequest = new CreateIdentityUserRequest
        {
            Email = evt.Email,
            PhoneNumber = evt.PhoneNumber,
            Username = evt.Username,
            PasswordHash = registration.PasswordHash.Value
        };

        var userId = await _identityService.CreateUserWithPrehashedPasswordAsync(createRequest, ct);

        // Remove the registration — it has fulfilled its purpose.
        _registrationRepo.Remove(registration);

        // Save the removal.
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Account provisioned for registration {RegistrationId}, user {UserId}",
            evt.RegistrationId,
            userId);
    }
}