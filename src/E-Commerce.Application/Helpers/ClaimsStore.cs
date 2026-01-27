using System.Security.Claims;

namespace E_Commerce.Domain.Helpers
{
    public static class ClaimsStore
    {
        public static List<Claim> claims = new()
        {
            new Claim("Create Admin", "False"),
            new Claim("Edit Admin", "False"),
            new Claim("Get Admin", "False"),
            new Claim("Get All Admins", "False"),
            new Claim("Delete Admin", "False"),

            new Claim("Create Employee", "False"),
            new Claim("Edit Employee", "False"),
            new Claim("Get Employee", "False"),
            new Claim("Get All Employee", "False"),
            new Claim("Delete Employee", "False"),

            new Claim("Edit Customer", "False"),
            new Claim("Get Customer", "False"),
            new Claim("Get All Customer", "False"),
            new Claim("Delete Customer", "False"),
        };
    }
}
