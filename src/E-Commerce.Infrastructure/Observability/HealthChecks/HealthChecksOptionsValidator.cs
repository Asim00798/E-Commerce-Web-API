using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Observability.HealthChecks;

/// <summary>
/// Validates HealthChecksOptions to prevent misconfiguration of thresholds.
/// </summary>
public sealed class HealthChecksOptionsValidator : IValidateOptions<HealthChecksOptions>
{
    public ValidateOptionsResult Validate(string? name, HealthChecksOptions options)
    {
        if (options.OutboxWarningThreshold < 0)
        {
            return ValidateOptionsResult.Fail("OutboxWarningThreshold cannot be negative.");
        }

        if (options.OutboxErrorThreshold < 0)
        {
            return ValidateOptionsResult.Fail("OutboxErrorThreshold cannot be negative.");
        }

        if (options.OutboxErrorThreshold <= options.OutboxWarningThreshold)
        {
            return ValidateOptionsResult.Fail(
                "OutboxErrorThreshold must be greater than OutboxWarningThreshold.");
        }

        return ValidateOptionsResult.Success;
    }
}