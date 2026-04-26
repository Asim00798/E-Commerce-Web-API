namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects
{
    public sealed record FileSize
    {
        public long Bytes { get; init; }

        public FileSize(long bytes)
        {
            if (bytes < 0) throw new ArgumentException("File size cannot be negative");
            Bytes = bytes;
        }

        public string HumanReadable => Bytes switch
        {
            < 1024 => $"{Bytes} B",
            < 1048576 => $"{Bytes / 1024.0:F2} KB",
            < 1073741824 => $"{Bytes / 1048576.0:F2} MB",
            _ => $"{Bytes / 1073741824.0:F2} GB"
        };
    }
}
