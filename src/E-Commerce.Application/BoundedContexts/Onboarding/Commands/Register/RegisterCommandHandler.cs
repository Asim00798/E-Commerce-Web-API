using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;                    // IAppContext
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Cryptography;
using E_Commerce.Application.Shared.Security.Verification;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IRegistrationRepository _registrationRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;                        // <-- added
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IRegistrationRepository registrationRepo,
        IPasswordHasher passwordHasher,
        IVerificationCodeService verificationCodeService,
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _registrationRepo = registrationRepo;
        _passwordHasher = passwordHasher;
        _verificationCodeService = verificationCodeService;
        _outboxWriter = outboxWriter;
        _appContext = appContext;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand command, CancellationToken ct)
    {
        if (await _registrationRepo.ExistsByEmailAsync(command.Email, ct))
            return Result<Guid>.Failure(new[] { "An active registration already exists for this email." });

        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var registration = new Registration(
            command.Email,
            command.PhoneNumber,
            command.Username,
            passwordHash,
            _clock.UtcNow);

        var emailHashed = _verificationCodeService.GenerateCode(out var emailPlain);
        registration.SetEmailVerificationCode(emailHashed, _clock.UtcNow);

        var phoneHashed = _verificationCodeService.GenerateCode(out var phonePlain);
        registration.SetPhoneVerificationCode(phoneHashed, _clock.UtcNow);

        await _registrationRepo.AddAsync(registration, ct);

        var emailEvent = new EmailVerificationRequestedIntegrationEvent
        {
            RegistrationId = registration.Id,
            Email = command.Email,
            Code = emailPlain,
            CorrelationId = _appContext.CorrelationId        // <-- propagate
        };
        var phoneEvent = new PhoneVerificationRequestedIntegrationEvent
        {
            RegistrationId = registration.Id,
            PhoneNumber = command.PhoneNumber,
            Code = phonePlain,
            CorrelationId = _appContext.CorrelationId        // <-- propagate
        };

        await _outboxWriter.WriteAsync(emailEvent, ct);
        await _outboxWriter.WriteAsync(phoneEvent, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Registration {Id} created for {Email}", registration.Id, command.Email);
        return Result<Guid>.Success(registration.Id);
    }
}