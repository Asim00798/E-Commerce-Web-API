using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Beginning transaction for {RequestName}", typeof(TRequest).Name);
            var response = await next();
            logger.LogInformation("Committed transaction for {RequestName}", typeof(TRequest).Name);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling transaction for {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
