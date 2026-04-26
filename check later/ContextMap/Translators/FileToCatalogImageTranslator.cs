using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Application.ContextMap.Translators
{
    public static class FileToCatalogImageTranslator
    {
        public static object ToCatalogImage(FileAggregate file)
        {
            // Placeholder for translator logic (mapping File to Catalog-specific image DTO/Entity)
            return new { file.Name.FullName, file.Path.Value };
        }
    }
}
