#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Entities
{
    public class StorePolicy : BaseEntity
    {
        public string Title { get; private set; }
        public string Content { get; private set; }

        public StorePolicy(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }
}

#endif