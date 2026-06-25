namespace Domain.BoundedContexts.UserManagement.Security.Enums
{
    public enum RefreshTokenStatus
    {
        Active = 0,
        Rotated = 1,
        Revoked = 2,
        Expired = 3
    }
}
