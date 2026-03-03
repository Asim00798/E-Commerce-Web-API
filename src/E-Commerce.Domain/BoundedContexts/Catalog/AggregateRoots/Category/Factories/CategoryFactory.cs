using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors;

public static class CategoryFactory
{
    public static Category CreateWithDefaults(string name)
    {
        // Example: initialize defaults
        Category category = new Category();
        // category.AddChild(...);
        // category.RegisterProduct(...);
        return category;
    }
}
