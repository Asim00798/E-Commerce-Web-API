using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors;
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
        var registration = await GetRegistrationAsync(evt.RegistrationId, ct);
        if (registration is null)
            return; // already processed or removed

        // Create the Identity user with the pre-hashed password
        var userId = await CreateIdentityUserAsync(evt, registration, ct);
        // Remove the registration after successfully creating the user
        await RemoveRegistrationAsync(registration, ct);
        // Log the successful provisioning
        LogProvisioningSuccess(evt.RegistrationId, userId);
    }

    private async Task<Registration?> GetRegistrationAsync(
        Guid registrationId,
        CancellationToken ct)
    {
        return await _registrationRepo.GetByIdAsync(registrationId, ct);
    }

    private async Task<Guid> CreateIdentityUserAsync(
        RegistrationFullyVerifiedIntegrationEvent evt,
        Registration registration,
        CancellationToken ct)
    {
        var createRequest = new CreateIdentityUserRequest
        {
            Email = evt.Email,
            PhoneNumber = evt.PhoneNumber,
            Username = evt.Username,
            PasswordHash = registration.PasswordHash.Value
        };

        return await _identityService.CreateUserWithPrehashedPasswordAsync(createRequest, ct);
    }

    private async Task RemoveRegistrationAsync(
        Registration registration,
        CancellationToken ct)
    {
        _registrationRepo.Remove(registration);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private void LogProvisioningSuccess(Guid registrationId, Guid userId)
    {
        _logger.LogInformation(
            "Account provisioned for registration {RegistrationId}, user {UserId}",
            registrationId,
            userId);
    }
}