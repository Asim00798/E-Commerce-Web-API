using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Application.ContextMap.Translators
{
    public static class FileToMarketplaceDocumentTranslator
    {
        public static object ToMarketplaceDocument(FileAggregate file)
        {
            // Placeholder for translator logic
            return new { file.Name.FullName, file.Path.Value };
        }
    }
}
