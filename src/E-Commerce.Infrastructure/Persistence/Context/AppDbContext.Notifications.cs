using E_Commerce.Infrastructure.Communication.Notifications.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
    }
}
