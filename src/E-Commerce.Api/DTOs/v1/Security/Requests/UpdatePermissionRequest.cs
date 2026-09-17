namespace E_Commerce.Api.DTOs.v1.Security.Requests
{
    public sealed record UpdatePermissionRequest(
    string Name,
    string? Description);

}
