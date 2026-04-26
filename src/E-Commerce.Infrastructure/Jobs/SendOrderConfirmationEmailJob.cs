namespace E_Commerce.Infrastructure.Jobs;

/// <summary>
/// Background job that dispatches order confirmation emails after an order is placed.
/// </summary>
public sealed class SendOrderConfirmationEmailJob
{
    // TODO: Inject IEmailNotificationService and order query service
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Fetch pending confirmations, send emails, mark as sent
        throw new NotImplementedException();
    }
}
