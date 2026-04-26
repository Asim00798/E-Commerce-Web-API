namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects
{
    public sealed record FileHash
    {
        public string Value { get; init; }
        public string Algorithm { get; init; }

        public FileHash(string value, string algorithm = "SHA256")
        {
            Value = value;
            Algorithm = algorithm;
        }
    }
}
