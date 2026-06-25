namespace E_Commerce.Application.Shared.Identity;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? UserName { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);
}