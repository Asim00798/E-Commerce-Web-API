using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Administration
{
    public class NotificationTemplate : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // e.g. ORDER_PLACED
        public string Title { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
