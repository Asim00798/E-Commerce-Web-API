using Microsoft.AspNetCore.Mvc.Filters;

namespace E_Commerce.Api.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) { }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
