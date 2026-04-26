#if false
using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        public override void Validate()
        {
            base.Validate();

            if (UserId == Guid.Empty)
                throw new InvalidOperationException("RefreshToken must belong to a User.");

            if (string.IsNullOrWhiteSpace(Token))
                throw new InvalidOperationException("RefreshToken value cannot be empty.");

            if (ExpiresAt <= DateTimeOffset.UtcNow)
                throw new InvalidOperationException("RefreshToken expiration must be in the future.");
        }
    }
}

#endif