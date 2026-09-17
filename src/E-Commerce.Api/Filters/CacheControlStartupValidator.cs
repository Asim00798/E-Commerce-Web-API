using E_Commerce.Api.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace E_Commerce.Api.Filters;

/// <summary>
/// Validates all <see cref="CacheControlAttribute"/> usages during application startup.
///
/// Invalid caching configuration causes the host to fail before it begins
/// serving HTTP traffic. This prevents configuration errors from appearing
/// as runtime 500 responses on the first affected request.
/// </summary>
public sealed class CacheControlStartupValidator : IHostedService
{
    private readonly IActionDescriptorCollectionProvider _actions;
    private readonly ILogger<CacheControlStartupValidator> _logger;

    public CacheControlStartupValidator(
        IActionDescriptorCollectionProvider actions,
        ILogger<CacheControlStartupValidator> logger)
    {
        _actions = actions;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var invalid = new List<string>();
        var validatedCount = 0;

        foreach (var descriptor in _actions.ActionDescriptors.Items
                     .OfType<ControllerActionDescriptor>())
        {
            validatedCount += ValidateAttribute(
                descriptor.ControllerTypeInfo.GetCustomAttributes(
                    typeof(CacheControlAttribute),
                    inherit: true)
                    .OfType<CacheControlAttribute>()
                    .FirstOrDefault(),
                descriptor.ControllerTypeInfo.FullName
                    ?? descriptor.ControllerName,
                invalid);

            validatedCount += ValidateAttribute(
                descriptor.MethodInfo.GetCustomAttributes(
                    typeof(CacheControlAttribute),
                    inherit: true)
                    .OfType<CacheControlAttribute>()
                    .FirstOrDefault(),
                $"{descriptor.ControllerTypeInfo.FullName}.{descriptor.MethodInfo.Name}",
                invalid);
        }

        if (invalid.Count > 0)
        {
            var message =
                $"Invalid [CacheControl] configuration detected on " +
                $"{invalid.Count} endpoint(s):{Environment.NewLine}" +
                string.Join(Environment.NewLine, invalid);

            _logger.LogCritical("{Message}", message);

            throw new InvalidOperationException(message);
        }

        _logger.LogInformation(
            "CacheControl validation passed. {AttributeCount} attribute instance(s) " +
            "validated across {ActionCount} action(s).",
            validatedCount,
            _actions.ActionDescriptors.Items.Count);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static int ValidateAttribute(
        CacheControlAttribute? attribute,
        string location,
        List<string> invalid)
    {
        if (attribute is null)
            return 0;

        try
        {
            attribute.Validate();
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            invalid.Add($"  - {location}: {ex.Message}");
            return 1;
        }
    }
}