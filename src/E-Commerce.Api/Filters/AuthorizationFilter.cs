using Microsoft.AspNetCore.Mvc.Filters;

namespace E_Commerce.Api.Filters
{
    public class AuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context) { }
    }
}
