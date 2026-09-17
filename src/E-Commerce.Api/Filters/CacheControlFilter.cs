using E_Commerce.Api.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace E_Commerce.Api.Filters;

/// <summary>
/// Global result filter that applies the Cache-Control policy defined by
/// <see cref="CacheControlAttribute"/>.
///
/// Only GET requests are processed.
/// Successful 2xx responses receive the configured caching policy.
/// Non-2xx responses receive <c>no-store</c> to prevent error responses
/// from being cached by clients or intermediaries.
/// </summary>
public sealed class CacheControlFilter : IResultFilter
{
    private const string HeaderName = "Cache-Control";

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
            return;

        var attribute = ResolveAttribute(context);

        if (attribute is null)
            return;

        var statusCode =
            (context.Result as IStatusCodeActionResult)?.StatusCode
            ?? context.HttpContext.Response.StatusCode;

        if (statusCode is not (>= 200 and < 300))
        {
            context.HttpContext.Response.Headers[HeaderName] = "no-store";
            return;
        }

        var headerValue = BuildCacheControlHeader(attribute);

        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            context.HttpContext.Response.Headers[HeaderName] = headerValue;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // No-op.
    }

    private static CacheControlAttribute? ResolveAttribute(
        ResultExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return null;

        // Action-level policy takes precedence over controller-level policy.
        var actionAttribute = descriptor.MethodInfo
            .GetCustomAttributes(
                typeof(CacheControlAttribute),
                inherit: true)
            .OfType<CacheControlAttribute>()
            .FirstOrDefault();

        if (actionAttribute is not null)
            return actionAttribute;

        return descriptor.ControllerTypeInfo
            .GetCustomAttributes(
                typeof(CacheControlAttribute),
                inherit: true)
            .OfType<CacheControlAttribute>()
            .FirstOrDefault();
    }

    private static string BuildCacheControlHeader(
        CacheControlAttribute attribute)
    {
        var directives = new List<string>(capacity: 5);

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