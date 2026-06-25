//using LibraryManagementSystem.Domain.Entities.Security;
//using LibraryManagement.Domain.Interfaces;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LibraryManagementSystem.Infrastructure.Persistence.DataSeed.Runtime.Security
//{
//    public static class RolePermissionSeed
//    {
//        public static async Task SeedRolePermissionsAsync(IServiceProvider serviceProvider)
//        {
//            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
//            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

//            // Get Roles
//            var adminRole = await roleManager.FindByNameAsync("Admin");
//            var managerRole = await roleManager.FindByNameAsync("Manager");
//            var auditorRole = await roleManager.FindByNameAsync("Auditor");

//            if (adminRole == null || managerRole == null || auditorRole == null)
//                throw new InvalidOperationException("Roles must exist before seeding role permissions.");

//            var rolePermissions = new[]
//            {
//                // Admin gets all permissions
//                new RolePermission
//                {
//                    Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
//                    RoleId = adminRole.Id,
//                    PermissionId = Guid.Parse("11111111-1111-1111-1111-111111111111")
//                },
//                new RolePermission
//                {
//                    Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
//                    RoleId = adminRole.Id,
//                    PermissionId = Guid.Parse("22222222-2222-2222-2222-222222222222")
//                },
//                new RolePermission
//                {
//                    Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
//                    RoleId = adminRole.Id,
//                    PermissionId = Guid.Parse("33333333-3333-3333-3333-333333333333")
//                },

//                // Manager gets only ManageBooks
//                new RolePermission
//                {
//                    Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
//                    RoleId = managerRole.Id,
//                    PermissionId = Guid.Parse("22222222-2222-2222-2222-222222222222")
//                },

//                // Auditor gets only ViewBooks
//                new RolePermission
//                {
//                    Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
//                    RoleId = auditorRole.Id,
//                    PermissionId = Guid.Parse("11111111-1111-1111-1111-111111111111")
//                }
//            };

//            foreach (var rp in rolePermissions)
//            {
//                if (await unitOfWork.RolePermissions.GetByIdAsync(rp.Id) == null)
//                    await unitOfWork.RolePermissions.AddAsync(rp);
//            }

//            await unitOfWork.CompleteAsync();
//        }
//    }
//}
