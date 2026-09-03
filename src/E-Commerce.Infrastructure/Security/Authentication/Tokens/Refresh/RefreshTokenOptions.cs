namespace E_Commerce.Infrastructure.Security.Authentication.Tokens.Refresh
{
    /// <summary>
    /// Runtime configuration for refresh token lifetime and rotation policy.
    /// </summary>
    public sealed class RefreshTokenOptions
    {
        public const string SectionName = "RefreshToken";

        /// <summary>
        /// Refresh token validity period. Default 30 days.
        /// </summary>
        public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromDays(30);
    }
}
