namespace E_Commerce.Application.Shared.Security.Identity;

public class IdentityOperationException : Exception
{
    public IdentityOperationException(string message) : base(message) { }
    public IdentityOperationException(string message, Exception inner) : base(message, inner) { }
}