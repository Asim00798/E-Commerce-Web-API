using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        public void AddAttribute(CategoryAttribute attribute)
        {
            if (_attributes.Any(a => a.Id == attribute.Id))
                throw new BusinessRuleViolationException("Attribute already exists.");
            _attributes.Add(attribute);
        }

        public void RemoveAttribute(Guid attributeId)
        {
            var attr = _attributes.FirstOrDefault(a => a.Id == attributeId);
            if (attr != null) _attributes.Remove(attr);
        }
    }
}
