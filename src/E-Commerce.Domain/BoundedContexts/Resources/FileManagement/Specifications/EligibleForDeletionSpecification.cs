using System.Linq.Expressions;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.SharedKernel.Specifications;
using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Specifications
{
    /// <summary>
    /// Business rule to identify files that are eligible for permanent deletion/purge from the system.
    /// </summary>
    public class EligibleForDeletionSpecification : ISpecification<FileAggregate>
    {
        public Expression<Func<FileAggregate, bool>> ToExpression()
        {
            return file => file.Status == FileStatusEnum.Deleted;
        }

        public bool IsSatisfiedBy(FileAggregate entity)
        {
            if (entity == null) return false;
            return entity.Status == FileStatusEnum.Deleted;
        }
    }
}
