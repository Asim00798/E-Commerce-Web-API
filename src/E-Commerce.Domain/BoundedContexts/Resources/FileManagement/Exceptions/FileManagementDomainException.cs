using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Exceptions
{
    public class FileManagementDomainException : DomainException
    {
        public FileManagementDomainException(string message) : base(message) { }
    }
}
