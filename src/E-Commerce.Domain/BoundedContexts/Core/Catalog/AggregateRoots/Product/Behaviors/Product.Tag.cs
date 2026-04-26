using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product 
    {
        public void AddTag(string tag)
        {
            EnsureIsDraft();
            if (_tags.Any(t => t.Name.Value == tag))
                throw new BusinessRuleViolationException("Duplicate tag.");

            _tags.Add(new Tag(tag));
        }
        public void RemoveTag(string tag)
        {
            EnsureIsDraft();
            var existingTag = _tags.FirstOrDefault(t => t.Name.Value == tag)
                ?? throw new BusinessRuleViolationException("Tag not found.");
            
            _tags.Remove(existingTag);
        }
    }
}
