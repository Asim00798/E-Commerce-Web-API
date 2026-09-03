namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

public static class CatalogPermissions
{
    public const string ViewBrands = "Catalog.Brands.Read";
    public const string ManageBrands = "Catalog.Brands.Manage";

    public const string ViewCategories = "Catalog.Categories.Read";
    public const string ManageCategories = "Catalog.Categories.Manage";

    public const string ViewProducts = "Catalog.Products.Read";
    public const string ManageProducts = "Catalog.Products.Manage";
}