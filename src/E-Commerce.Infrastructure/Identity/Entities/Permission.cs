using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Infrastructure.Identity.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
