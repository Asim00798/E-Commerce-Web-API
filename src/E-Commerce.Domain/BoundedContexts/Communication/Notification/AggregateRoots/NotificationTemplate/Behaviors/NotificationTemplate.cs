using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.Communication.Notification.AggregateRoots.NotificationTemplate.Behaviors
{
    public class NotificationTemplate : BaseEntity, IAggregateRoot
    {
        public string Code { get; set; } = string.Empty; // e.g. ORDER_PLACED
        public string Title { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
