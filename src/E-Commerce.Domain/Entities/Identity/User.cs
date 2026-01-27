using E_Commerce.Domain.Entities;
using E_Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Domain.Entities.Identity
{
    public class User : IdentityUser<Guid>
    {
        // Required 1–1 link: User must originate from Registration
        public Guid RegistrationId { get; set; }
        public Registration Registration { get; set; } = null!;

    }

}
