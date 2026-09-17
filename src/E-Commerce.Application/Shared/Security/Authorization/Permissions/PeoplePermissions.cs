namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permissions for the People bounded context.
///
/// The "Own" variants govern self-service operations on the current user's
/// own Person record. The non-Own variants are reserved for future
/// administrative endpoints.
/// </summary>
public static class PeoplePermissions
{
    public const string ReadOwn = "People.ReadOwn";
    public const string CreateOwn = "People.CreateOwn";
    public const string UpdateOwn = "People.UpdateOwn";
    public const string DeleteOwn = "People.DeleteOwn";

    // Reserved for future admin endpoints (GetPersonById, ListPersons, etc.)
    public const string Read = "People.Read";
    public const string Update = "People.Update";
    public const string Delete = "People.Delete";
}