using E_Commerce.Api.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace E_Commerce.Api.Filters;

/// <summary>
/// Global result filter that applies Cache-Control headers based on [CacheControl] attribute.
/// Only processes GET requests. Action-level attribute overrides controller-level.
/// </summary>
public sealed class CacheControlFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
            return;

        var attribute = GetCacheControlAttribute(context);
        if (attribute is null)
            return;

        attribute.Validate();

        var headerValue = BuildCacheControlHeader(attribute);
        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            context.HttpContext.Response.Headers["Cache-Control"] = headerValue;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static CacheControlAttribute? GetCacheControlAttribute(ResultExecutingContext context)
    {
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        if (actionDescriptor is null)
            return null;

        var actionAttribute = actionDescriptor.MethodInfo
            .GetCustomAttributes(typeof(CacheControlAttribute), inherit: true)
            .FirstOrDefault() as CacheControlAttribute;

        if (actionAttribute is not null)
            return actionAttribute;

        var controllerAttribute = actionDescriptor.ControllerTypeInfo
            .GetCustomAttributes(typeof(CacheControlAttribute), inherit: true)
            .FirstOrDefault() as CacheControlAttribute;

        return controllerAttribute;
    }

    private static string BuildCacheControlHeader(CacheControlAttribute attribute)
    {
        var directives = new List<string>();

        if (attribute.NoStore)
        {
            directives.Add("no-store");
        }
        else if (attribute.NoCache)
        {
            directives.Add("no-cache");
        }
        else
        {
            if (attribute.Public)
                directives.Add("public");
            else if (attribute.Private)
                directives.Add("private");

            if (attribute.MaxAge >= 0)
                directives.Add($"max-age={attribute.MaxAge}");

            if (attribute.SharedMaxAge >= 0)
                directives.Add($"s-maxage={attribute.SharedMaxAge}");

            if (attribute.MustRevalidate)
                directives.Add("must-revalidate");
        }

        return string.Join(", ", directives);
    }
}