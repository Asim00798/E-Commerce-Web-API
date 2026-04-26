namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects
{
    public sealed record MimeType
    {
        public string Value { get; init; }

        public MimeType(string value)
        {
            Value = value;
        }

        public bool IsImage => Value.StartsWith("image/");
        public bool IsVideo => Value.StartsWith("video/");
    }
}
