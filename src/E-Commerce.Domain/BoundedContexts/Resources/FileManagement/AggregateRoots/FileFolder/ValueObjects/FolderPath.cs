namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.ValueObjects
{
    public sealed record FolderPath
    {
        public string Value { get; init; }

        public FolderPath(string value)
        {
            Value = value;
        }

        public override string ToString() => Value;
    }
}
