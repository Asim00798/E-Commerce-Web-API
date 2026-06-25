namespace E_Commerce.Application.BoundedContexts.Security;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    public string Roles { get; set; } = string.Empty;
    public string Policy { get; set; } = string.Empty;
}
