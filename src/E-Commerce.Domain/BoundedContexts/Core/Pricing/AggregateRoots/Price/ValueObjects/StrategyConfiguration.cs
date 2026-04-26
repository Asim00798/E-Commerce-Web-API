#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects
{
    public sealed record StrategyConfiguration
    {
        public string StrategyName { get; init; }
        public Dictionary<string, string> Parameters { get; init; } = new();

        public StrategyConfiguration(string strategyName, Dictionary<string, string>? parameters = null)
        {
            StrategyName = strategyName;
            if (parameters != null)
            {
                Parameters = parameters;
            }
        }
    }
}

#endif