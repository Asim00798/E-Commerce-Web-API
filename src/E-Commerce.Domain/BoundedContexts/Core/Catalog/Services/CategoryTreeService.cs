using E_Commerce.Domain.Catalog.AggregateRoots.Category;
using E_Commerce.Domain.Catalog.Exceptions;

namespace E_Commerce.Domain.Catalog.Services
{
    public class CategoryTreeService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryTreeService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IReadOnlyList<Category>> GetAncestorsAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
                ?? throw new CategoryNotFoundException(categoryId);

            var ancestors = new List<Category>();
            var current = category;
            while (current.ParentId != null)
            {
                current = await _categoryRepository.GetByIdAsync(current.ParentId, cancellationToken)
                    ?? throw new CategoryNotFoundException($"Parent category {current.ParentId} not found");
                ancestors.Add(current);
            }
            return ancestors.AsReadOnly();
        }

        public async Task<string> GenerateBreadcrumbAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
        {
            var ancestors = await GetAncestorsAsync(categoryId, cancellationToken);
            var names = ancestors.Select(c => c.Name).Reverse().ToList();
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            names.Add(category.Name);
            return string.Join(" > ", names);
        }
    }
}
