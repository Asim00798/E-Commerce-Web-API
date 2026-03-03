using E_Commerce.Domain.BoundedContexts.Identity;
using E_Commerce.Domain.SharedKernel.Abstract;
using System;
using System.Collections.Generic;


namespace E_Commerce.Domain.BoundedContexts.Identity
{
    public class Permission : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
