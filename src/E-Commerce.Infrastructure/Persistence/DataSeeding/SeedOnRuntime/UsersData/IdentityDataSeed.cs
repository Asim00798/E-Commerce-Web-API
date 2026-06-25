//using LibraryManagement.Domain.Entities;
//using LibraryManagement.Domain.Interfaces;
//using LibraryManagement.Infrastructure.Persistence;
//using LibraryManagementSystem.Domain.Entities;
//using LibraryManagementSystem.Domain.Entities.Security;
//using LibraryManagementSystem.Domain.Enums;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Threading.Tasks;


//namespace LibraryManagementSystem.Infrastructure.Persistence.DataSeed.Runtime.Security
//{
//    public static class IdentityDataSeed
//    {
//        /// <summary>
//        /// Seeds default roles: Admin, User, Manager, Auditor
//        /// </summary>
//        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
//        {
//            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

//            string[] roles = { "Admin", "User", "Manager", "Auditor" };

//            foreach (var roleName in roles)
//            {
//                if (!await roleManager.RoleExistsAsync(roleName))
//                {
//                    await roleManager.CreateAsync(new Role
//                    {
//                        Name = roleName,
//                        NormalizedName = roleName.ToUpper(),
//                        Description = roleName + " role"
//                    });
//                }
//            }
//            foreach (var roleName in roles)
//            {
//                if (!await roleManager.RoleExistsAsync(roleName))
//                {
//                    var role = new Role
//                    {
//                        Name = roleName,
//                        NormalizedName = roleName.ToUpper(),
//                        Description = roleName + " role"
//                    };

//                    var result = await roleManager.CreateAsync(role);
//                    if (!result.Succeeded)
//                    {
//                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//                        throw new Exception($"Role creation failed for '{roleName}': {errors}");
//                    }
//                }
//            }

//        }

//        /// <summary>
//        /// Seeds a default Admin user with a valid registration.
//        /// </summary>
//        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
//        {
//            await SeedUserAsync(serviceProvider,
//                username: "admin",
//                email: "admin@system.com",
//                password: "Admin@12345",
//                role: "Admin",
//                registrationId: Guid.Parse("33333333-3333-3333-3333-333333333333"));
//        }

//        /// <summary>
//        /// Seeds a default Manager user with a valid registration.
//        /// </summary>
//        public static async Task SeedManagerUserAsync(IServiceProvider serviceProvider)
//        {
//            await SeedUserAsync(serviceProvider,
//                username: "manager",
//                email: "manager@system.com",
//                password: "Manager@12345",
//                role: "Manager",
//                registrationId: Guid.Parse("11111111-1111-1111-1111-111111111111"));
//        }

//        /// <summary>
//        /// Seeds a default Auditor user with a valid registration.
//        /// </summary>
//        public static async Task SeedAuditorUserAsync(IServiceProvider serviceProvider)
//        {
//            await SeedUserAsync(serviceProvider,
//                username: "auditor",
//                email: "auditor@system.com",
//                password: "Auditor@12345",
//                role: "Auditor",
//                registrationId: Guid.Parse("22222222-2222-2222-2222-222222222222"));
//        }

//        /// <summary>
//        /// Helper method to create user if it does not exist, ensuring a valid RegistrationId
//        /// </summary>
//        private static async Task SeedUserAsync(IServiceProvider serviceProvider, string username, string email, string password, string role, Guid registrationId)
//        {
//            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
//            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

//            // Check if user exists
//            var user = await userManager.FindByEmailAsync(email);
//            if (user != null) return; // already exists

//            // Ensure registration exists
//            var registration = await unitOfWork.Registrations.GetByIdAsync(registrationId);
//            if (registration == null)
//            {
//                // Create a minimal person
//                var person = new Person
//                {
//                    FirstName = username,
//                    LastName = username,
//                    DateOfBirth = DateTime.UtcNow.AddYears(-30), // dummy DOB
//                    Gender = GenderType.Male
//                };
//                await unitOfWork.People.AddAsync(person);
//                await unitOfWork.CompleteAsync();

//                // Create registration linked to that person
//                registration = new Registration
//                {
//                    Id = registrationId,
//                    PersonId = person.Id,
//                    Status = RegistrationStatus.Completed
//                };
//                await unitOfWork.Registrations.AddAsync(registration);
//                await unitOfWork.CompleteAsync();
//            }

//            // Create user
//            user = new User
//            {
//                UserName = username,
//                Email = email,
//                EmailConfirmed = true,
//                RegistrationId = registration.Id
//            };

//            var result = await userManager.CreateAsync(user, password);
//            if (!result.Succeeded)
//            {
//                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//                throw new InvalidOperationException($"Failed to create {role} user: {errors}");
//            }

//            // Add to role
//            await userManager.AddToRoleAsync(user, role);
//        }

//    }
//}
