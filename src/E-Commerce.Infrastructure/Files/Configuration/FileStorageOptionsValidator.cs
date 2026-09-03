using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Files.Configuration;

public sealed class FileStorageOptionsValidator : IValidateOptions<FileStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, FileStorageOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Provider))
            errors.Add("FileStorage:Provider is required.");

        if (options.MaxFileSizeBytes <= 0)
            errors.Add("FileStorage:MaxFileSizeBytes must be greater than zero.");

        if (options.Provider?.Equals("Local", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (string.IsNullOrWhiteSpace(options.Local?.RootPath))
                errors.Add("FileStorage:Local:RootPath is required when Provider is Local.");
        }

        if (options.OrphanGracePeriod <= TimeSpan.Zero)
            errors.Add("FileStorage:OrphanGracePeriod must be positive.");

        if (options.DeletionClaimLease <= TimeSpan.Zero)
            errors.Add("FileStorage:DeletionClaimLease must be positive.");

        if (errors.Count > 0)
            return ValidateOptionsResult.Fail(errors);

        return ValidateOptionsResult.Success;
    }
}