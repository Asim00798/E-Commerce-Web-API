namespace E_Commerce.Domain.Responses
{
    public class ManageUserClaimsResponse
    {
        public Guid UserId { get; set; }
        public List<UserClaims> UserClaims { get; set; }
    }

    public class UserClaims
    {
        public string Type { get; set; }
        public bool Value { get; set; }
    }
}
