using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Identity.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = false;
        // Navigation properties
        public ICollection<RolePermission>? RolePermissions { get; private set; }

        // DDD Constructor
        public Role(string roleName, string? description = null)
        {
            Name = roleName;
            Description = description;
        }

        // Empty constructor for EF
        protected Role() { }
    }
}

