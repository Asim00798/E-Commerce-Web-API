using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Verification;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.ResendPhone;

public sealed class ResendPhoneCommandHandler : IRequestHandler<ResendPhoneCommand, Result>
{
    private readonly IRegistrationRepository _repo;
    private readonly IVerificationCodeService _verification;
    private readonly IOutboxMessageWriter _outbox;
    private readonly IAppContext _appContext;
    private readonly IClock _clock;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ResendPhoneCommandHandler> _logger;

    public ResendPhoneCommandHandler(
        IRegistrationRepository repo,
        IVerificationCodeService verification,
        IOutboxMessageWriter outbox,
        IAppContext appContext,
        IClock clock,
        IUnitOfWork uow,
        ILogger<ResendPhoneCommandHandler> logger)
    {
        _repo = repo;
        _verification = verification;
        _outbox = outbox;
        _appContext = appContext;
        _clock = clock;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(ResendPhoneCommand command, CancellationToken ct)
    {
        var registration = await GetRegistrationAsync(command.RegistrationId, ct);
        if (registration is null)
            return Result.Failure(new[] { "Registration not found." });

        try
        {
            var plainCode = GenerateAndSetPhoneVerificationCode(registration);
            var evt = BuildPhoneVerificationEvent(registration, plainCode);

            await _outbox.WriteAsync(evt, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Resend phone code failed for registration {Id}", command.RegistrationId);
            return Result.Failure(new[] { ex.Message });
        }
    }

    private async Task<Registration?> GetRegistrationAsync(
        Guid registrationId,
        CancellationToken ct)
    {
        return await _repo.GetByIdAsync(registrationId, ct);
    }

    private string GenerateAndSetPhoneVerificationCode(Registration registration)
    {
        var hashedCode = _verification.GenerateCode(out var plainCode);
        registration.SetPhoneVerificationCode(hashedCode, _clock.UtcNow);
        return plainCode;
    }

    private PhoneVerificationRequestedIntegrationEvent BuildPhoneVerificationEvent(
        Registration registration,
        string plainCode)
    {
        return new PhoneVerificationRequestedIntegrationEvent
        {
            RegistrationId = registration.Id,
            PhoneNumber = registration.PhoneNumber.Value,
            Code = plainCode,
            CorrelationId = _appContext.CorrelationId
        };
    }
}