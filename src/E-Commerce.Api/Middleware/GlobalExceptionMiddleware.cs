using System.Net;
using E_Commerce.Application.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client cancelled the request; let ASP.NET Core handle it naturally.
            throw;
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(
                    ex,
                    "An unhandled exception occurred after the response had already started.");
                throw;
            }

            LogException(ex, context);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private void LogException(Exception exception, HttpContext context)
    {
        var logLevel = GetLogLevel(exception);

        _logger.Log(
            logLevel,
            exception,
            "Unhandled exception while processing {Method} {Path}.",
            context.Request.Method,
            context.Request.Path);
    }

    private static LogLevel GetLogLevel(Exception exception) => exception switch
    {
        ConcurrencyException => LogLevel.Warning,
        ValidationException => LogLevel.Warning,
        UnauthorizedAccessException => LogLevel.Warning,
        ForbiddenAccessException => LogLevel.Warning,
        NotFoundException => LogLevel.Debug,
        _ => LogLevel.Error
    };

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = MapException(exception);
        var problemDetails = BuildProblemDetails(context, statusCode, title, exception, errors);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static (HttpStatusCode StatusCode, string Title, IDictionary<string, string[]>? Errors)
        MapException(Exception exception) => exception switch
        {
            ValidationException validationEx =>
                (HttpStatusCode.BadRequest, "Validation Error", validationEx.Errors),

            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message, null),

            ForbiddenAccessException =>
                (HttpStatusCode.Forbidden, "Forbidden Access", null),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized", null),

            ConcurrencyException =>
                (HttpStatusCode.Conflict, "Concurrency Conflict", null),

            _ =>
                (HttpStatusCode.InternalServerError, "Internal Server Error", null)
        };

    private static ProblemDetails BuildProblemDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        Exception exception,
        IDictionary<string, string[]>? errors)
    {
        var isInternalServerError = statusCode == HttpStatusCode.InternalServerError;

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            // Never leak internal exception messages to the client for 500s.
            // For validation errors, errors dictionary already carries details.
            Detail = isInternalServerError
                ? "An unexpected error occurred."
                : (errors is null ? exception.Message : null),
            Instance = context.Request.Path
        };

        // Prefer OpenTelemetry trace id when available, else fall back to request id.
        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            problem.Extensions["traceId"] = traceId;
        }
        else
        {
            problem.Extensions["traceId"] = context.TraceIdentifier;
        }

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return problem;
    }
}