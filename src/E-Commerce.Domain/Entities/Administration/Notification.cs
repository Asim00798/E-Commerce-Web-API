using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Entities.Administration
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public NotificationChannel Channel { get; set; }
        public NotificationStatus Status { get; set; }

        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public string? FailureReason { get; set; }
    }
}
