namespace E_Commerce.Application.Modules.Authentication.Dtos;

/// <summary>
/// Result of an authentication operation.
/// </summary>
public sealed class AuthenticationResultDto
{
    public bool Succeeded { get; init; }
    public TokenPairDto? Tokens { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static AuthenticationResultDto Success(TokenPairDto tokens) =>
        new() { Succeeded = true, Tokens = tokens };

    public static AuthenticationResultDto Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };

    public static AuthenticationResultDto Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToArray() };
}