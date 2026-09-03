using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Files.Services;

namespace E_Commerce.Infrastructure.Files.Services;

public sealed class FileContentInspector : IFileContentInspector
{
    private static readonly Dictionary<string, FileType> SignatureMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = FileType.Image,
        ["image/jpeg"] = FileType.Image,
        ["image/gif"] = FileType.Image,
        ["image/webp"] = FileType.Image
    };

    public async Task<FileInspectionResult> InspectAsync(
        Stream content,
        FileType expectedType,
        CancellationToken ct = default)
    {
        var (detectedMime, isRecognized) = await DetectMimeAsync(content, ct);

        var detectedType = detectedMime is null ? FileType.Unknown : MapToFileType(detectedMime);

        var validationStatus = DetermineValidationStatus(detectedType, isRecognized, expectedType);

        return new FileInspectionResult(detectedType, validationStatus);
    }

    private async Task<(string? MimeType, bool IsRecognized)> DetectMimeAsync(
        Stream content,
        CancellationToken ct)
    {
        if (!content.CanSeek)
            return (null, false);

        var originalPosition = content.Position;
        var buffer = new byte[12];
        var read = await content.ReadAsync(buffer, 0, buffer.Length, ct);
        content.Position = originalPosition;

        if (read == 0)
            return (null, false);

        foreach (var signature in SignatureMap)
        {
            if (StartsWith(buffer, signature.Key, read))
                return (signature.Key, true);
        }

        if (IsWebP(buffer, read))
            return ("image/webp", true);

        return (null, false);
    }

    private static FileValidationStatus DetermineValidationStatus(
        FileType detectedType,
        bool isRecognized,
        FileType expectedType)
    {
        if (detectedType == FileType.Unknown)
            return FileValidationStatus.Unknown;

        if (!isRecognized)
            return FileValidationStatus.Invalid;

        if (detectedType != expectedType)
            return FileValidationStatus.Invalid;

        return FileValidationStatus.Valid;
    }

    private static FileType MapToFileType(string mimeType)
    {
        return SignatureMap.TryGetValue(mimeType, out var fileType)
            ? fileType
            : FileType.Unknown;
    }

    private static bool StartsWith(byte[] buffer, string mimeType, int bytesRead)
    {
        byte[]? signature = GetSignatureForMime(mimeType);
        if (signature is null || bytesRead < signature.Length)
            return false;

        for (var i = 0; i < signature.Length; i++)
        {
            if (buffer[i] != signature[i])
                return false;
        }

        return true;
    }

    private static byte[]? GetSignatureForMime(string mimeType)
    {
        return mimeType switch
        {
            "image/png" => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
            "image/jpeg" => new byte[] { 0xFF, 0xD8, 0xFF },
            "image/gif" => new byte[] { 0x47, 0x49, 0x46, 0x38 },
            _ => null
        };
    }

    private static bool IsWebP(byte[] buffer, int bytesRead)
    {
        if (bytesRead < 12)
            return false;

        return buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
               buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
    }
}