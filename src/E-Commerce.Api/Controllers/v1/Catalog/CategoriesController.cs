using E_Commerce.Api.Attributes;
using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Catalog.Categories.Requests;
using E_Commerce.Api.DTOs.v1.Catalog.Categories.Responses;
using E_Commerce.Api.DTOs.v1.Shared;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.AddCategoryImage;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.CreateCategory;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.RemoveCategoryImage;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.UpdateCategory;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.GetCategoryById;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.ListCategories;
using E_Commerce.Application.Shared.Files.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Catalog;

/// <summary>
/// Manages category resources.
/// Reads are available to any authenticated user.
/// Writes require the CatalogManager or Administrator role.
/// </summary>
[ApiController]
[Route("api/catalog/categories")]
public sealed class CategoriesController : BaseApiController
{
    private const string WriteRoles = "CatalogManager,Administrator";

    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct)
    {
        var command = new CreateCategoryCommand(
            request.Name,
            request.Description,
            request.ParentCategoryId);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var response = new CategoryResponse
        {
            Id = result.Data,
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };

        return CreatedAtAction(
            nameof(GetCategoryById),
            new { categoryId = result.Data },
            response);
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateCategory(
        Guid categoryId,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(
            categoryId,
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.ClearParent);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    [HttpPost("{categoryId:guid}/images")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddCategoryImage(
        Guid categoryId,
        [FromForm] AddCategoryImageRequest request,
        CancellationToken ct)
    {
        var image = new FileUpload(
            request.Image.OpenReadStream(),
            request.Image.FileName,
            request.Image.ContentType);

        var command = new AddCategoryImageCommand(categoryId, image);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    [HttpDelete("{categoryId:guid}/images/{fileId:guid}")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveCategoryImage(
        Guid categoryId,
        Guid fileId,
        CancellationToken ct)
    {
        var command = new RemoveCategoryImageCommand(categoryId, fileId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    [HttpGet("{categoryId:guid}")]
    [Authorize]
    [CacheControl(Public = true, MaxAge = 1800)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(
        Guid categoryId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCategoryByIdQuery(categoryId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    [HttpGet]
    [Authorize]
    [CacheControl(Public = true, MaxAge = 1800)]
    [ProducesResponseType(typeof(PaginatedResponse<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCategories(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new ListCategoriesQuery(pageNumber, pageSize), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var page = result.Data!;

        var response = new PaginatedResponse<CategoryResponse>
        {
            Items = page.Items.Select(MapToResponse).ToList(),
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalPages = page.TotalPages,
            TotalCount = page.TotalCount,
            HasPreviousPage = page.HasPreviousPage,
            HasNextPage = page.HasNextPage
        };

        return Ok(response);
    }

    // ------------------------------------------------------------------
    // Mapping
    // ------------------------------------------------------------------

    private static CategoryResponse MapToResponse(CategoryDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        ParentCategoryId = dto.ParentCategoryId,
        ImageFileIds = dto.ImageFileIds
    };

    // ------------------------------------------------------------------
    // Error helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns an RFC 7807 validation problem with the given errors.
    /// Shape matches GlobalExceptionMiddleware's ProblemDetails output
    /// so clients see one error contract across the API.
    /// </summary>
    private IActionResult ToValidationProblem(string[] errors)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in errors)
        {
            modelState.AddModelError(string.Empty, error);
        }

        return ValidationProblem(modelState);
    }

    private IActionResult ToNotFoundProblem(string[] errors) =>
        Problem(
            title: "Resource not found.",
            detail: string.Join(" ", errors),
            statusCode: StatusCodes.Status404NotFound);
}