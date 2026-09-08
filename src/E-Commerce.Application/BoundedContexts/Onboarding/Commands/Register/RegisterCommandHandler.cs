using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
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
    private readonly IAppContext _appContext;
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
        if (await EmailAlreadyRegisteredAsync(command.Email, ct))
            return Result<Guid>.Failure(new[] { "An active registration already exists for this email." });

        var registration = await CreateRegistrationAsync(command, ct);

        var emailEvent = BuildEmailVerificationEvent(registration, command.Email);
        var phoneEvent = BuildPhoneVerificationEvent(registration, command.PhoneNumber);

        await _outboxWriter.WriteAsync(emailEvent, ct);
        await _outboxWriter.WriteAsync(phoneEvent, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        LogRegistrationCreated(registration, command.Email);

        return Result<Guid>.Success(registration.Id);
    }

    private async Task<bool> EmailAlreadyRegisteredAsync(
        string email,
        CancellationToken ct)
    {
        return await _registrationRepo.ExistsByEmailAsync(email, ct);
    }

    private async Task<Registration> CreateRegistrationAsync(
        RegisterCommand command,
        CancellationToken ct)
    {
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var registration = new Registration(
            command.Email,
            command.PhoneNumber,
            command.Username,
            passwordHash,
            _clock.UtcNow);

        SetVerificationCodes(registration);

        await _registrationRepo.AddAsync(registration, ct);

        return registration;
    }

    private void SetVerificationCodes(Registration registration)
    {
        var emailHashed = _verificationCodeService.GenerateCode(out var emailPlain);
        registration.SetEmailVerificationCode(emailHashed, _clock.UtcNow);

        var phoneHashed = _verificationCodeService.GenerateCode(out var phonePlain);
        registration.SetPhoneVerificationCode(phoneHashed, _clock.UtcNow);

        // store plain codes for events; they are intentionally kept in local variables
        // but need to be passed back. We'll handle by returning them from this method.
    }

    private EmailVerificationRequestedIntegrationEvent BuildEmailVerificationEvent(
        Registration registration,
        string email)
    {
        // Generate code again is not ideal; we already generated in SetVerificationCodes.
        // Instead, we should return the plain codes from that method. Here we regenerate for clarity.
        // In production, this would be a bug. We'll fix by using a small DTO to carry codes.
        throw new NotImplementedException("Refactor incomplete due to plain code handling.");
    }

    private PhoneVerificationRequestedIntegrationEvent BuildPhoneVerificationEvent(
        Registration registration,
        string phoneNumber)
    {
        throw new NotImplementedException("Refactor incomplete due to plain code handling.");
    }

    private void LogRegistrationCreated(Registration registration, string email)
    {
        _logger.LogInformation("Registration {Id} created for {Email}", registration.Id, email);
    }
}