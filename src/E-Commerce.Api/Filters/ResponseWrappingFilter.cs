using Microsoft.AspNetCore.Mvc.Filters;

namespace E_Commerce.Api.Filters
{
    public class ResponseWrappingFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context) { }
        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
