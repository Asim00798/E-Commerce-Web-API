using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Brand.Behaviors;

public static class BrandFactory
{
    public static Brand CreateWithDefaults(string name)
    {
        // Example: initialize defaults
        Brand brand = new Brand("","");
        // Brand.Add(...);
        // Brand.(...);
        return brand;
    }
}
