using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Infrastructure.Identity.Entities
{
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public Guid PermissionId { get; set; }        
        public Permission Permission { get; set; } = null!;
    }
}