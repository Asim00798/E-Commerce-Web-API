namespace E_Commerce.Application.Common.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
}
