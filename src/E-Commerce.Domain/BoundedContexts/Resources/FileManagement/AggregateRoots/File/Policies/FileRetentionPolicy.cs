namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Policies
{
    public class FileRetentionPolicy
    {
        public bool ShouldRetain(DateTime createdAt, int retentionDays)
        {
            return DateTime.UtcNow <= createdAt.AddDays(retentionDays);
        }
    }
}
