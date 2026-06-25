namespace E_Commerce.Application.Shared.Communication.Notifications
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(Guid orderId, Guid customerId);
    }
}
