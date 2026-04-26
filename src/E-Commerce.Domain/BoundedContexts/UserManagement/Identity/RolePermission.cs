#if false
using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_Commerce.Domain.SharedKernel.Abstract;


namespace E_Commerce.Domain.BoundedContexts.UserManagement.Identity
{
    public class RolePermission : BaseEntity
    {
        public Guid Id { get; set; } 

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public Guid PermissionId { get; set; }        
        public Permission Permission { get; set; } = null!;
    }
}

#endif