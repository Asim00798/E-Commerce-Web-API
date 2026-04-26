using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Exceptions
{
    public class FolderDomainException : FileManagementDomainException
    {
        public FolderDomainException(string message) : base(message) { }
    }
}
