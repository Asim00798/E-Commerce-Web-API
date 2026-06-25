using E_Commerce.Domain.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Identity.Entities
{
    public class User : IdentityUser<Guid>
    {
        // Required 1�1 link: User must originate from Registration
        public Guid? RegistrationId { get; private set; }
        public bool IsActive { get; private set; } = false;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public User(string userName, string email, Guid? registrationId = null)
        {
            UserName = userName;
            Email = email;
            RegistrationId = registrationId;
        }

        // Empty constructor for EF
        protected User() { }
    }
}

