namespace E_Commerce.Api.DTOs.v1.Files.Requests
{
    public sealed class UploadFileRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
