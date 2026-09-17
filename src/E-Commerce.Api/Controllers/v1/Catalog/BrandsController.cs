using E_Commerce.Api.Attributes;
using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Catalog.Brands.Requests;
using E_Commerce.Api.DTOs.v1.Catalog.Brands.Responses;
using E_Commerce.Api.DTOs.v1.Shared;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.GetBrandById;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;
using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Catalog;

/// <summary>
/// Manages brand resources.
/// Reads are available to any authenticated user.
/// Writes require the CatalogManager or Administrator role.
/// </summary>
[ApiController]
[Route("api/catalog/brands")]
public sealed class BrandsController : BaseApiController
{
    private const string WriteRoles = $"{SystemRoles.CatalogManager},{SystemRoles.Administrator}";

    private readonly ISender _sender;

    public BrandsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBrand(
        [FromForm] CreateBrandRequest request,
        CancellationToken ct)
    {
        var logo = new FileUpload(
            request.Logo.OpenReadStream(),
            request.Logo.FileName,
            request.Logo.ContentType);

        var command = new CreateBrandCommand(
            request.Name,
            request.Description,
            logo);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        // The command returns only the new brand ID. LogoFileId is not
        // available without a re-query, so the created-response body
        // omits it. Clients can fetch the full brand via the Location
        // header target (GetBrandById), which includes LogoFileId.
        var response = new BrandResponse
        {
            Id = result.Data,
            Name = request.Name,
            DescriptionText = request.Description
        };

        return CreatedAtAction(
            nameof(GetBrandById),
            new { brandId = result.Data },
            response);
    }

    [HttpPut("{brandId:guid}")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateBrand(
        Guid brandId,
        [FromForm] UpdateBrandRequest request,
        CancellationToken ct)
    {
        FileUpload? newLogo = null;

        if (request.Logo is not null)
        {
            newLogo = new FileUpload(
                request.Logo.OpenReadStream(),
                request.Logo.FileName,
                request.Logo.ContentType);
        }

        var command = new UpdateBrandCommand(
            brandId,
            request.Name,
            request.Description,
            newLogo);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    [HttpGet("{brandId:guid}")]
    [Authorize]
    [CacheControl(Public = true, MaxAge = 1800)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandById(
        Guid brandId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetBrandByIdQuery(brandId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    [HttpGet]
    [Authorize]
    [CacheControl(Public = true, MaxAge = 1800)]
    [ProducesResponseType(typeof(PaginatedResponse<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListBrands(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new ListBrandsQuery(pageNumber, pageSize), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var page = result.Data!;

        var response = new PaginatedResponse<BrandResponse>
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

    private static BrandResponse MapToResponse(BrandDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        DescriptionText = dto.DescriptionText,
        // BrandDto.LogoFileId is a non-nullable Guid; Guid.Empty means "no logo".
        // BrandResponse.LogoFileId is Guid? and uses null for the same meaning.
        LogoFileId = dto.LogoFileId == Guid.Empty ? null : dto.LogoFileId
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