#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects
{
    public sealed record Region
    {
        public string Code { get; init; }
        public string Name { get; init; }

        public Region(string code, string name)
        {
            Code = code?.ToUpper() ?? "GLOBAL";
            Name = name;
        }

        public static Region Global => new("GLOBAL", "Global Region");
    }
}

#endif