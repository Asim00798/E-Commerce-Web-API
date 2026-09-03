using MediatR;
using Microsoft.Extensions.Logging;
using E_Commerce.Application.Shared.Security.Identity;

namespace E_Commerce.Application.Shared.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId?.ToString() ?? "Anonymous";

        // Log only the request name and user ID.
        // The full request object is intentionally excluded from logs
        // to prevent accidental exposure of sensitive data
        // (passwords, credit card numbers, personal information, etc.).
        logger.LogInformation(
            "E-Commerce Request: {Name} by User {@UserId}",
            requestName,
            userId);

        return await next();
    }
}