using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context;

public partial class AppDbContext
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();
}