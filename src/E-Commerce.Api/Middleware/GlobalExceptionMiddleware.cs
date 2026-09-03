using E_Commerce.Application.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace E_Commerce.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IExceptionResponseMapper _responseMapper;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IExceptionResponseMapper responseMapper)
    {
        _next = next;
        _logger = logger;
        _responseMapper = responseMapper;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Let ASP.NET Core handle client cancellation naturally.
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
        DbUpdateConcurrencyException => LogLevel.Warning,
        ValidationException => LogLevel.Warning,
        UnauthorizedAccessException => LogLevel.Warning,
        ForbiddenAccessException => LogLevel.Warning,
        NotFoundException => LogLevel.Debug,
        _ => LogLevel.Error
    };

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = _responseMapper.Map(exception);
        var problemDetails = BuildProblemDetails(context, statusCode, title, exception, errors);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

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
            Detail = isInternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return problem;
    }
}