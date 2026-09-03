using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;                    // IAppContext
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
    private readonly IAppContext _appContext;                        // <-- added
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
        var registration = await _repo.GetByIdAsync(command.RegistrationId, ct);
        if (registration is null)
            return Result.Failure(new[] { "Registration not found." });

        try
        {
            var hashedCode = _verification.GenerateCode(out var plainCode);
            registration.SetPhoneVerificationCode(hashedCode, _clock.UtcNow);

            var evt = new PhoneVerificationRequestedIntegrationEvent
            {
                RegistrationId = registration.Id,
                PhoneNumber = registration.PhoneNumber.Value,
                Code = plainCode,
                CorrelationId = _appContext.CorrelationId        // <-- propagate
            };
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
}