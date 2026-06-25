
namespace E_Commerce.Infrastructure.Persistence.Extensions;

public static class DbContextLoggingExtensions
{
    /// <summary>
    /// Logs a summary of the pending entity state changes in the change tracker.
    /// Purely observational – does not modify any tracked entity.
    /// </summary>
    /// <param name="context">The DbContext whose change tracker will be inspected.</param>
    /// <param name="logger">The logger to write to.</param>
    public static void LogChangeTrackerSummary(this DbContext context, ILogger logger)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
        {
            logger.LogDebug("SaveChanges called with no pending changes");
            return;
        }

        var added = entries.Count(e => e.State == EntityState.Added);
        var modified = entries.Count(e => e.State == EntityState.Modified);
        var deleted = entries.Count(e => e.State == EntityState.Deleted);

        logger.LogInformation(
            "Saving changes to database: {Added} added, {Modified} modified, {Deleted} deleted",
            added, modified, deleted);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            foreach (var entry in entries)
            {
                logger.LogDebug(
                    "Entity: {EntityName} State: {State}",
                    entry.Entity.GetType().Name,
                    entry.State);
            }
        }
    }
}