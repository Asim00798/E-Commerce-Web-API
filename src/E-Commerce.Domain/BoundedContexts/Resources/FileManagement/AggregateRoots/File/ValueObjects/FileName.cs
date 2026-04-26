namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects
{
    public sealed record FileName
    {
        public string Name { get; init; }
        public string Extension { get; init; }

        public FileName(string fullName)
        {
            var parts = fullName.Split('.');
            Name = parts[0];
            Extension = parts.Length > 1 ? parts[^1] : string.Empty;
        }

        public string FullName => string.IsNullOrEmpty(Extension) ? Name : $"{Name}.{Extension}";
    }
}
