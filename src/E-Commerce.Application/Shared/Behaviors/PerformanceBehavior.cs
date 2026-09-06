using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using E_Commerce.Application.Shared.Security.Identity;

namespace E_Commerce.Application.Shared.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        var elapsedMilliseconds = timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = currentUser.UserId?.ToString() ?? "Anonymous";

            logger.LogWarning(
                "E-Commerce Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) by User {UserId}",
                requestName,
                elapsedMilliseconds,
                userId);
        }

        return response;
    }
}