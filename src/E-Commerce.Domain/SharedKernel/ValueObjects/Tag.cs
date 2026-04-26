using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.SharedKernel.ValueObjects
{
    /// <summary>
    /// Represents a domain tag (e.g. for products, categories).
    /// Promoted to SharedKernel for cross-context utility.
    /// </summary>
    public sealed record Tag : IValueObject
    {
        public TagName Name { get; private set; }

        public Tag(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = new TagName(name);
        }

        public Tag(TagName name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name.ToString();
    }
}
