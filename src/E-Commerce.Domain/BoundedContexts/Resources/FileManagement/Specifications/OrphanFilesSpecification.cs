using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Specifications
{
    public class OrphanFilesSpecification : ISpecification<FileAggregate>
    {
        public Expression<Func<FileAggregate, bool>> ToExpression()
        {
            return file => file.FolderId == null;
        }

        public bool IsSatisfiedBy(FileAggregate entity)
        {
            if (entity == null) return false;
            return entity.FolderId == null;
        }

    }
}
