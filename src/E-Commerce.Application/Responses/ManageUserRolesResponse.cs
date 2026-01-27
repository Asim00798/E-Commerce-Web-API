namespace E_Commerce.Domain.Responses
{
    public class ManageUserRolesResponse
    {
        public Guid UserId { get; set; }
        public List<UserRoles>? UserRoles { get; set; }
    }

    public class UserRoles
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool HasRole { get; set; }
    }
}
