namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects
{
    public sealed record FilePath
    {
        public string Value { get; init; }

        public FilePath(string value)
        {
            Value = value;
        }

        public override string ToString() => Value;
    }
}
