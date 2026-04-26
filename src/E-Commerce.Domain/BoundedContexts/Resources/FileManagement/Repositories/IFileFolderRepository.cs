using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using FileFolderAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Behaviors.FileFolder;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Repositories
{
    public interface IFileFolderRepository : IRepository<FileFolderAggregate>
    {}
}

