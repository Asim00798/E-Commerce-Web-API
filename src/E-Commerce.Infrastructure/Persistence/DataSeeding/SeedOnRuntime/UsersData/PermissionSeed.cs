//using LibraryManagement.Domain.Entities.Security;
//using LibraryManagement.Domain.Interfaces;
//using LibraryManagementSystem.Domain.Entities.Security;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Threading.Tasks;

//namespace LibraryManagementSystem.Infrastructure.Persistence.DataSeed.Runtime.Security
//{
//    public static class PermissionSeed
//    {
//        public static async Task SeedPermissionsAsync(IServiceProvider serviceProvider)
//        {
//            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

//            var permissions = new[]
//            {
//                new Permission
//                {
//                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
//                    Name = "ViewBooks",
//                    Description = "Allows viewing of books."
//                },
//                new Permission
//                {
//                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
//                    Name = "ManageBooks",
//                    Description = "Allows adding, editing, and deleting books."
//                },
//                new Permission
//                {
//                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
//                    Name = "ManageUsers",
//                    Description = "Allows managing user accounts and roles."
//                }
//            };

//            foreach (var permission in permissions)
//            {
//                if (await unitOfWork.Permissions.GetByIdAsync(permission.Id) == null)
//                {
//                    await unitOfWork.Permissions.AddAsync(permission);
//                }
//            }

//            await unitOfWork.CompleteAsync();
//        }
//    }
//}
