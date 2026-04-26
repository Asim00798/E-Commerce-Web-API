
namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.Services
{
    /// <summary>
    /// Generic read service for querying document validity from the Verification context.
    /// Works for any owner type (Brand, Product, Seller, etc.).
    /// </summary>
    public interface IDocumentReadService
    {
        /// <summary>
        /// Checks whether a document of a specific type is valid for a given owner.
        /// </summary>
        /// <param name="ownerId">Identifier of the owning entity (e.g., Brand.Id, Product.Id).</param>
        /// <param name="ownerType">Discriminator, e.g., "Brand", "Product".</param>
        /// <param name="documentType">Type of document, e.g., "TrademarkCertificate", "SafetyCertificate".</param>
        Task<bool> HasValidDocumentAsync(Guid ownerId, string ownerType, string documentType, CancellationToken ct = default);
    }
}