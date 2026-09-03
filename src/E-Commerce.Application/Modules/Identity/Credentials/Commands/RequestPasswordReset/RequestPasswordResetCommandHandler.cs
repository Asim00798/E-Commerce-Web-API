using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Modules.Identity.Credentials.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Modules.Identity.Credentials.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Result>
{
    private readonly IAccountReader _accountReader;
    private readonly ICredentialManagement _credentialManagement;
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        IAccountReader accountReader,
        ICredentialManagement credentialManagement,
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _accountReader = accountReader;
        _credentialManagement = credentialManagement;
        _outboxWriter = outboxWriter;
        _appContext = appContext;
        _logger = logger;
    }

    public async Task<Result> Handle(RequestPasswordResetCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim();
        var account = await _accountReader.GetByEmailAsync(email, ct);

        // Always return the same outward result to prevent account enumeration.
        if (account is null)
        {
            _logger.LogInformation("Password reset requested for unknown email.");
            return Result.Success();
        }

        var resetToken = await _credentialManagement.GeneratePasswordResetTokenAsync(account.UserId, ct);

        var integrationEvent = new PasswordResetRequestedIntegrationEvent
        {
            UserId = account.UserId,
            Email = account.Email,
            ResetToken = resetToken,
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);

        _logger.LogInformation("Password reset requested for user {UserId}.", account.UserId);
        return Result.Success();
    }
}