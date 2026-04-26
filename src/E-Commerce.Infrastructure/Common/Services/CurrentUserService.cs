namespace E_Commerce.Infrastructure.Common.Services;

/// <summary>
/// Infrastructure implementation of ICurrentUser.
/// Resolves the authenticated user's identity from the HTTP context.
/// </summary>
public sealed class CurrentUserService
{
    // TODO: Inject IHttpContextAccessor
    // TODO: Implement ICurrentUser interface from Application layer

    public Guid? UserId { get; }
    public string? UserName { get; }
    public bool IsAuthenticated { get; }
}
