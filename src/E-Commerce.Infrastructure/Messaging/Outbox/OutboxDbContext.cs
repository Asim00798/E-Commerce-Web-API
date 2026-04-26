namespace E_Commerce.Infrastructure.Messaging.Outbox;

/// <summary>
/// Dedicated EF Core DbContext for the outbox pattern.
/// Isolated from domain DbContexts to allow independent schema migrations.
/// </summary>
public sealed class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: Configure OutboxMessage table, indexes
    }
}
