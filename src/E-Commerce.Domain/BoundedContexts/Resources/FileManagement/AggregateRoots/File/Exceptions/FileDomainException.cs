using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Exceptions
{
    public class FileDomainException : FileManagementDomainException
    {
        public FileDomainException(string message) : base(message) { }
    }
}
