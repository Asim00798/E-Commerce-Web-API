using E_Commerce.Api.DTOs.Catalog.Categories.Requests;
using E_Commerce.Api.DTOs.Catalog.Categories.Responses;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.AddCategoryImage;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.CreateCategory;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.RemoveCategoryImage;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.UpdateCategory;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.GetCategoryById;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.ListCategories;
using E_Commerce.Application.Shared.Files.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Catalog;

[ApiController]
[Route("api/catalog/categories")]
public sealed class CategoriesController : BaseApiController
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            return BadRequest(result.Errors);

        var response = new CategoryResponse
        {
            Id = result.Data,
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };

        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data }, response);
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{categoryId:guid}/images")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{categoryId:guid}/images/{fileId:guid}")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveCategoryImage(
        Guid categoryId,
        Guid fileId,
        CancellationToken ct)
    {
        var command = new RemoveCategoryImageCommand(categoryId, fileId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpGet("{categoryId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(
        Guid categoryId,
        CancellationToken ct)
    {
        var query = new GetCategoryByIdQuery(categoryId);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        var response = new CategoryResponse
        {
            Id = result.Data!.Id,
            Name = result.Data.Name,
            Description = result.Data.Description,
            ParentCategoryId = result.Data.ParentCategoryId,
            ImageFileIds = result.Data.ImageFileIds
        };

        return Ok(response);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCategories(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new ListCategoriesQuery(pageNumber, pageSize);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var responses = result.Data!.Items
            .Select(category => new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ImageFileIds = category.ImageFileIds
            })
            .ToList();

        return Ok(responses);
    }
}
