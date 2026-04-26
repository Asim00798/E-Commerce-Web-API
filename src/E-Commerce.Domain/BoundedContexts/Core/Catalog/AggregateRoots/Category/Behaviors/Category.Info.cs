
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        public void UpdateInfo(CategoryInfo newInfo)
        {
            Info = newInfo ?? throw new ArgumentNullException(nameof(newInfo));
        }
    }
}
