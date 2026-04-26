namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.ValueObjects
{
    public sealed record FolderName
    {
        public string Value { get; init; }

        public FolderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Folder name cannot be empty");
            Value = value;
        }

        public override string ToString() => Value;
    }
}
