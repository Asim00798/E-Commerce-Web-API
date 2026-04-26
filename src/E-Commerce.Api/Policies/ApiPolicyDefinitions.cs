using Microsoft.AspNetCore.Authorization;

namespace E_Commerce.Api.Policies;

public static class ApiPolicyDefinitions
{
    public const string AdminOnly = "AdminOnly";

    public static void AddApiPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(AdminOnly, policy => policy.RequireRole("Admin"));
    }
}
