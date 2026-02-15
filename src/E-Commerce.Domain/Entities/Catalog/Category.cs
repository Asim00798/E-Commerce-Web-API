using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Category : BaseEntity
    {
        public CategoryInfo Info { get; private set; }
        public Guid? ParentCategoryId { get; private set; }

        // Navigation (EF)
        public Category? ParentCategory { get; private set; }
        public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products;

        public Category(CategoryInfo info, Guid? parentCategoryId = null)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            ParentCategoryId = parentCategoryId;
        }

        // -----------------------
        // VO update methods
        // -----------------------
        /*
         Aggregate Root is the single authority for state changes.
         This method enforces all business rules that depend on
         aggregate state and coordinates the Value Object update.
         No external code may bypass this method.
        */
        public void Rename(string newName)
        {
            if (_products.Any())
                throw new BusinessRuleViolationException("Cannot rename category with assigned products.");
            Info = Info.Rename(newName);
        }

        // -----------------------
        // Product management
        // -----------------------
        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (_products.Any(p => p.Id == product.Id))
                return; // Already added

            _products.Add(product);
        }

        public void RemoveProduct(Product product)
        {
            if (product == null) return;
            _products.Remove(product);
        }

        // -----------------------
        // Hierarchy management
        // -----------------------
        public void MoveUnder(Category parent)
        {
            if (parent.Id == this.Id)
                throw new BusinessRuleViolationException("Category cannot be its own parent.");

            if (IsDescendantOf(parent))
                throw new BusinessRuleViolationException("Cannot move a category under its descendant.");

            ParentCategory = parent;
            ParentCategoryId = parent.Id;
        }

        private bool IsDescendantOf(Category potentialAncestor)
        {
            var current = this.ParentCategory;
            while (current != null)
            {
                if (current.Id == potentialAncestor.Id) return true;
                current = current.ParentCategory;
            }
            return false;
        }

        public bool IsRoot() => ParentCategoryId == null;
    }
}
