namespace E_Commerce.Domain.Helpers
{
    public class JwtAuthResponse
    {
        public string AccessToken { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }

    public class RefreshToken
    {
        public string UserName { get; set; }
        public string TokenString { get; set; }
        public DateTimeOffset ExpireAt { get; set; }
    }
}
