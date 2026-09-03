using E_Commerce.Api.DTOs.Files.Requests;
using E_Commerce.Api.DTOs.Files.Responses;
using E_Commerce.Application.Shared.Files.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Files
{
    [ApiController]
    [Route("api/files")]
    public sealed class FilesController : BaseApiController
    {
        private const string FilesReadPolicy = "Permission:Files.Read";
        private const string FilesUploadPolicy = "Permission:Files.Upload";
        private const string FilesDeletePolicy = "Permission:Files.Delete";

        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        [Authorize(Policy = FilesUploadPolicy)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(
            [FromForm] UploadFileRequest request,
            CancellationToken ct)
        {
            if (request.File is null || request.File.Length == 0)
                return BadRequest("No file provided.");

            await using var stream = request.File.OpenReadStream();
            var fileId = await _fileService.UploadAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                ct);

            return CreatedAtAction(nameof(GetMetadata), new { id = fileId }, fileId);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = FilesReadPolicy)]
        [ProducesResponseType(typeof(FileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMetadata(Guid id, CancellationToken ct)
        {
            var file = await _fileService.GetAsync(id, ct);
            if (file is null)
                return NotFound();

            return Ok(new FileResponse(
                file.Id,
                file.FileName,
                file.ContentType,
                file.Size,
                file.CreatedAtUtc));
        }

        [HttpGet("{id:guid}/download")]
        [Authorize(Policy = FilesReadPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(Guid id, CancellationToken ct)
        {
            var result = await _fileService.DownloadAsync(id, ct);
            if (result is null)
                return NotFound();

            return File(result.Content, result.Metadata.ContentType, result.Metadata.FileName);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = FilesDeletePolicy)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var file = await _fileService.GetAsync(id, ct);
            if (file is null)
                return NotFound();

            await _fileService.DeleteAsync(id, ct);
            return Accepted();
        }
    }
}

namespace E_Commerce.Api.DTOs.Files.Requests
{
    public sealed class UploadFileRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}

namespace E_Commerce.Api.DTOs.Files.Responses
{
    public sealed record FileResponse(
        Guid Id,
        string FileName,
        string ContentType,
        long Size,
        DateTime CreatedAtUtc);
}