using Microsoft.Extensions.Options;

namespace E_Commerce.ReadModel.Infrastructure.Configuration;

/// <summary>
/// Validates ReadModelOptions.
/// </summary>
public sealed class ReadModelOptionsValidator : IValidateOptions<ReadModelOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        ReadModelOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.MigrationConnectionString))
        {
            return ValidateOptionsResult.Fail(
                "ReadModel:MigrationConnectionString is required.");
        }

        return ValidateOptionsResult.Success;
    }
}