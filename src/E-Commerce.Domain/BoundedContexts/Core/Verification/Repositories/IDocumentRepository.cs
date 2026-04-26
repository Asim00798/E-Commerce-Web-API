using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors;
namespace E_Commerce.Domain.BoundedContexts.Core.Verification.Repositories
{
    /// <summary>
    /// Abstraction for accessing Document aggregates.
    /// Implementation belongs in the Infrastructure layer.
    /// </summary>
    public interface IDocumentRepository : IRepository<Document>
    { }
}
