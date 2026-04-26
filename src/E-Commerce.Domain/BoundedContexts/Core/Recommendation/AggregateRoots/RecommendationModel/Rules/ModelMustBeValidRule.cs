#if false
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Rules
{
    public class ModelMustBeValidRule : IBusinessRule
    {
        private readonly string _name;

        public ModelMustBeValidRule(string name)
        {
            _name = name;
        }

        public bool IsSatisfied() => !string.IsNullOrWhiteSpace(_name);

        public string Message => "Recommendation model must have a non-empty name.";
    }
}

#endif