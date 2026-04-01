using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Enums;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.Communication.Notification.AggregateRoots.Notification.Behaviors
{
    public class Notification : BaseEntity, IAggregateRoot
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
