namespace E_Commerce.Api.DTOs.v1.Files.Responses
{
    public sealed record FileResponse(
        Guid Id,
        string FileName,
        string ContentType,
        long Size,
        DateTime CreatedAtUtc);
}
