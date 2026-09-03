using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Verification;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.VerifyEmail;

/// <summary>
/// Handles email verification by validating the code and updating the aggregate.
/// Idempotent if the email is already verified.
/// </summary>
public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IRegistrationRepository _registrationRepo;
    private readonly IVerificationCodeService _verificationService;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        IRegistrationRepository registrationRepo,
        IVerificationCodeService verificationService,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _registrationRepo = registrationRepo;
        _verificationService = verificationService;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken ct)
    {
        var registration = await _registrationRepo.GetByIdAsync(command.RegistrationId, ct);
        if (registration is null)
            return Result.Failure(new[] { "Registration not found." });

        // Idempotent: already verified
        if (registration.EmailVerification.IsVerified)
            return Result.Success();

        var isValid = _verificationService.VerifyCode(command.Code, registration.EmailVerification.CodeHash!);

        try
        {
            registration.VerifyEmail(isValid, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Email verification failed for registration {Id}", command.RegistrationId);
            return Result.Failure(new[] { ex.Message });
        }
        // Concurrency exceptions propagate to the global middleware
    }
}