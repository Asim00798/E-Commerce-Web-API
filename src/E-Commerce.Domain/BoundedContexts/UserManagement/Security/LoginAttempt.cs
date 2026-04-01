using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security
{
    public class LoginAttempt : BaseEntity
    {
        public Guid? UserId { get; set; }           // Nullable: can attempt login with unregistered email
        public string? UsernameOrEmail { get; set; }

        public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsSuccessful { get; set; } = false;
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }

        public User? User { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (!string.IsNullOrWhiteSpace(UsernameOrEmail) && UsernameOrEmail.Length < 3)
                throw new InvalidOperationException("Username or Email must have at least 3 characters.");
        }
    }
}
