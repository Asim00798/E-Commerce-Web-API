using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Repositories
{
    public interface IFileRepository : IRepository<FileAggregate>
    { }
}

