using MediatR;
using Microsoft.Extensions.Logging;
using E_Commerce.Application.Common.Abstractions;

namespace E_Commerce.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId ?? "Anonymous";

        logger.LogInformation("E-Commerce Request: {Name} {@UserId} {@Request}",
            requestName, userId, request);

        return await next();
    }
}
