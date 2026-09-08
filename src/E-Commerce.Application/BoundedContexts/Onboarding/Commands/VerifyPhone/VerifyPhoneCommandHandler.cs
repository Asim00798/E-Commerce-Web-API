using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Verification;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.VerifyPhone;

/// <summary>
/// Handles phone verification by validating the code and updating the aggregate.
/// Idempotent if the phone is already verified.
/// </summary>
public sealed class VerifyPhoneCommandHandler : IRequestHandler<VerifyPhoneCommand, Result>
{
    private readonly IRegistrationRepository _registrationRepo;
    private readonly IVerificationCodeService _verificationService;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyPhoneCommandHandler> _logger;

    public VerifyPhoneCommandHandler(
        IRegistrationRepository registrationRepo,
        IVerificationCodeService verificationService,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<VerifyPhoneCommandHandler> logger)
    {
        _registrationRepo = registrationRepo;
        _verificationService = verificationService;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(VerifyPhoneCommand command, CancellationToken ct)
    {
        var registration = await GetRegistrationAsync(command.RegistrationId, ct);
        if (registration is null)
            return Result.Failure(new[] { "Registration not found." });

        if (IsPhoneAlreadyVerified(registration))
            return Result.Success();

        var isValid = ValidatePhoneCode(registration, command.Code);

        return await VerifyAndSaveAsync(registration, isValid, command.RegistrationId, ct);
    }

    private async Task<Registration?> GetRegistrationAsync(
        Guid registrationId,
        CancellationToken ct)
    {
        return await _registrationRepo.GetByIdAsync(registrationId, ct);
    }

    private static bool IsPhoneAlreadyVerified(Registration registration)
    {
        return registration.PhoneVerification.IsVerified;
    }

    private bool ValidatePhoneCode(Registration registration, string code)
    {
        return _verificationService.VerifyCode(
            code,
            registration.PhoneVerification.CodeHash!);
    }

    private async Task<Result> VerifyAndSaveAsync(
        Registration registration,
        bool isValid,
        Guid registrationId,
        CancellationToken ct)
    {
        try
        {
            registration.VerifyPhone(isValid, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                ex,
                "Phone verification failed for registration {Id}",
                registrationId);
            return Result.Failure(new[] { ex.Message });
        }
    }
}