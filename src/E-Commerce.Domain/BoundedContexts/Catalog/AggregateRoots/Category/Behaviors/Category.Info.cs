
using E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        public CategoryInfo Info { get; private set; }
    }
}
