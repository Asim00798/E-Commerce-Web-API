using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using System;

namespace E_Commerce.Domain.Entities.Ordering
{
    public class OrderStatusHistory : BaseEntity
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? ChangedByUserId { get; set; } // Optional admin or system user

        // Navigation
        public Order? Order { get; set; }
        public User? ChangedBy { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (ChangedAt > DateTimeOffset.UtcNow)
                throw new InvalidOperationException("ChangedAt cannot be in the future.");
        }
    }
}
