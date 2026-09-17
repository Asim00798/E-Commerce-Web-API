using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Files.Requests;
using E_Commerce.Api.DTOs.v1.Files.Responses;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Files;

/// <summary>
/// Upload, retrieve, download, and delete files managed by the File Storage capability.
/// Authorization is enforced at the API layer via roles; the Application layer
/// does not define commands/queries for this feature (see File Storage design).
/// </summary>
[ApiController]
[Route("api/files")]
[Authorize(Roles = $"{SystemRoles.Administrator},{SystemRoles.Customer},{SystemRoles.Employee}")]
public sealed class FilesController : BaseApiController
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Uploads a file. Returns 201 with a Location header pointing to the
    /// metadata endpoint and a body containing the new file's identifier
    /// as a bare GUID string.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upload(
        [FromForm] UploadFileRequest request,
        CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return ToValidationProblem("No file provided.");

        await using var stream = request.File.OpenReadStream();

        var fileId = await _fileService.UploadAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            ct);

        return CreatedAtAction(
            nameof(GetMetadata),
            new { id = fileId },
            fileId);
    }

    /// <summary>
    /// Returns metadata for a file.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMetadata(Guid id, CancellationToken ct)
    {
        var file = await _fileService.GetAsync(id, ct);

        if (file is null)
            return ToNotFoundProblem(id);

        return Ok(new FileResponse(
            file.Id,
            file.FileName,
            file.ContentType,
            file.Size,
            file.CreatedAtUtc));
    }

    /// <summary>
    /// Downloads the file content. Returns the raw bytes with the stored
    /// content type and file name.
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _fileService.DownloadAsync(id, ct);

        if (result is null)
            return ToNotFoundProblem(id);

        return File(
            result.Content,
            result.Metadata.ContentType,
            result.Metadata.FileName);
    }

    /// <summary>
    /// Marks a file for asynchronous deletion. Returns 202 Accepted — the
    /// physical file is removed by the background cleanup job.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var file = await _fileService.GetAsync(id, ct);

        if (file is null)
            return ToNotFoundProblem(id);

        await _fileService.DeleteAsync(id, ct);

        return Accepted();
    }

    // ------------------------------------------------------------------
    // Error helpers
    // ------------------------------------------------------------------

    private IActionResult ToValidationProblem(string error)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(string.Empty, error);
        return ValidationProblem(modelState);
    }

    private IActionResult ToNotFoundProblem(Guid fileId) =>
        Problem(
            title: "Resource not found.",
            detail: $"File '{fileId}' was not found.",
            statusCode: StatusCodes.Status404NotFound);
}