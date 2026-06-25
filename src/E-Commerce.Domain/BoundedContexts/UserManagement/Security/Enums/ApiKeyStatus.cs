namespace Domain.BoundedContexts.UserManagement.Security.Enums
{
    public enum ApiKeyStatus
    {
        Pending = 0,
        Active = 1,
        Revoked = 2,
        Compromised = 3,
        Expired = 4
    }
}
