using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (HttpStatusCode.BadRequest, "Validation Error", validationEx.Errors),
            NotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message, null),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Forbidden Access", null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", null),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = message,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (errors != null)
        {
            response.Extensions.Add("errors", errors);
        }

        var result = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(result);
    }
}
