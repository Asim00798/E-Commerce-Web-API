using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.Enums;
public interface IDocumentRepository : IRepository<Document>
{
    Task<IReadOnlyList<Document>> GetByOwnerAsync(Guid ownerId, OwnerType ownerType, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetByStatusAsync(DocumentStatus status, CancellationToken ct = default);
    Task<bool> IsTitleUniqueAsync(string title, CancellationToken ct = default); // domain invariant
}