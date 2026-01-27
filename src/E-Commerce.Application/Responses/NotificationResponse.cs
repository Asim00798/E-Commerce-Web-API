using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Responses
{
    public class NotificationResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ReceiverId { get; set; }
        public NotificationReceiverType ReceiverType { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToLocalTime();
        public bool IsRead { get; set; } = false;
    }
}
