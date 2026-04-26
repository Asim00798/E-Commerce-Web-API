using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities
{
    public class FileTag : BaseEntity
    {
        public string TagValue { get; private set; }

        public FileTag(string tagValue)
        {
            TagValue = tagValue;
        }
    }
}
