using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;

namespace E_Commerce.Infrastructure.Persistence.Context;

public partial class AppDbContext
{
    public DbSet<PaymentAggregate> Payments => Set<PaymentAggregate>();

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    public DbSet<Refund> Refunds => Set<Refund>();
}