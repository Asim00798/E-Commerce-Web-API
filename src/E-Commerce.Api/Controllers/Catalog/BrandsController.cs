using E_Commerce.Api.Attributes;
using E_Commerce.Api.DTOs.Catalog.Brands.Requests;
using E_Commerce.Api.DTOs.Catalog.Brands.Responses;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.GetBrandById;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;
using E_Commerce.Application.Shared.Files.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Catalog;

[ApiController]
[Route("api/catalog/brands")]
public sealed class BrandsController : BaseApiController
{
    private readonly ISender _sender;

    public BrandsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            return BadRequest(result.Errors);

        var response = new BrandResponse
        {
            Id = result.Data,
            Name = request.Name,
            DescriptionText = request.Description
        };

        return CreatedAtAction(nameof(GetBrandById), new { id = result.Data }, response);
    }

    [HttpPut("{brandId:guid}")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpGet("{brandId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [CacheControl(Public = true, MaxAge = 1800)]
    public async Task<IActionResult> GetBrandById(
        Guid brandId,
        CancellationToken ct)
    {
        var query = new GetBrandByIdQuery(brandId);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        var response = new BrandResponse
        {
            Id = result.Data!.Id,
            Name = result.Data.Name,
            DescriptionText = result.Data.DescriptionText
        };

        return Ok(response);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<BrandResponse>), StatusCodes.Status200OK)]
    [CacheControl(Public = true, MaxAge = 1800)]
    public async Task<IActionResult> ListBrands(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new ListBrandsQuery(pageNumber, pageSize);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var responses = result.Data!.Items
            .Select(brand => new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                DescriptionText = brand.DescriptionText
            })
            .ToList();

        return Ok(responses);
    }
}
