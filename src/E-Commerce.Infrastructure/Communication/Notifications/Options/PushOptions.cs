namespace E_Commerce.Infrastructure.Communication.Notifications.Options;

public sealed class PushOptions
{
    public const string SectionName = "Push";

    public string? CredentialFilePath { get; init; }
}