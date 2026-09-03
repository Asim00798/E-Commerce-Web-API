using E_Commerce.Api.DTOs.Catalog.Products.Requests;
using E_Commerce.Api.DTOs.Catalog.Products.Responses;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductImage;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductTag;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductVariant;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.DecreaseProductStock;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.DiscontinueProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.DraftProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.IncreaseProductStock;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.PublishProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.RemoveProductImage;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.RemoveProductTag;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.RemoveProductVariant;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.SetMainProductImage;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductVariantPrice;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.GetProductById;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.ListProducts;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.SearchProducts;
using E_Commerce.Application.Shared.Files.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Catalog;

[ApiController]
[Route("api/catalog/products")]
public sealed class ProductsController : BaseApiController
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.ShortDescription,
            request.LongDescription,
            request.Dimensions,
            request.Weight,
            request.DateOfManufacture,
            request.DateOfExpiry,
            request.Material,
            request.Color,
            request.BrandId,
            request.CategoryId,
            request.Tags);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetProductById), new { id = result.Data }, result.Data);
    }

    [HttpPost("{productId:guid}/publish")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishProduct(
        Guid productId,
        CancellationToken ct)
    {
        var command = new PublishProductCommand(productId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/draft")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DraftProduct(
        Guid productId,
        CancellationToken ct)
    {
        var command = new DraftProductCommand(productId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/discontinue")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DiscontinueProduct(
        Guid productId,
        CancellationToken ct)
    {
        var command = new DiscontinueProductCommand(productId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/images")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProductImage(
        Guid productId,
        [FromForm] AddProductImageRequest request,
        CancellationToken ct)
    {
        var image = new FileUpload(
            request.Image.OpenReadStream(),
            request.Image.FileName,
            request.Image.ContentType);

        var command = new AddProductImageCommand(productId, image);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/images/{imageId:guid}/main")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetMainProductImage(
        Guid productId,
        Guid imageId,
        CancellationToken ct)
    {
        var command = new SetMainProductImageCommand(productId, imageId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveProductImage(
        Guid productId,
        Guid imageId,
        CancellationToken ct)
    {
        var command = new RemoveProductImageCommand(productId, imageId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/variants")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProductVariant(
        Guid productId,
        [FromBody] AddProductVariantRequest request,
        CancellationToken ct)
    {
        var command = new AddProductVariantCommand(
            productId,
            request.Name,
            request.Sku,
            request.PriceAmount,
            request.Currency,
            request.StockQuantity);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPut("{productId:guid}/variants/{variantId:guid}/price")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProductVariantPrice(
        Guid productId,
        Guid variantId,
        [FromBody] UpdateProductVariantPriceRequest request,
        CancellationToken ct)
    {
        var command = new UpdateProductVariantPriceCommand(
            productId,
            variantId,
            request.NewPriceAmount,
            request.Currency);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveProductVariant(
        Guid productId,
        Guid variantId,
        CancellationToken ct)
    {
        var command = new RemoveProductVariantCommand(productId, variantId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/tags")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProductTag(
        Guid productId,
        [FromBody] AddProductTagRequest request,
        CancellationToken ct)
    {
        var command = new AddProductTagCommand(productId, request.Tag);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{productId:guid}/tags")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveProductTag(
        Guid productId,
        [FromBody] RemoveProductTagRequest request,
        CancellationToken ct)
    {
        var command = new RemoveProductTagCommand(productId, request.Tag);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/stock/increase")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IncreaseProductStock(
        Guid productId,
        Guid variantId,
        [FromBody] IncreaseProductStockRequest request,
        CancellationToken ct)
    {
        var command = new IncreaseProductStockCommand(productId, variantId, request.Quantity);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/stock/decrease")]
    [Authorize(Roles = "CatalogManager,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DecreaseProductStock(
        Guid productId,
        Guid variantId,
        [FromBody] DecreaseProductStockRequest request,
        CancellationToken ct)
    {
        var command = new DecreaseProductStockCommand(productId, variantId, request.Quantity);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpGet("{productId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        Guid productId,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(productId);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        var response = MapToProductResponse(result.Data!);
        return Ok(response);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ProductListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new ListProductsQuery(pageNumber, pageSize);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var responses = result.Data!.Items.Select(MapToProductListResponse).ToList();
        return Ok(responses);
    }

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ProductListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new SearchProductsQuery(searchTerm, pageNumber, pageSize);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var responses = result.Data!.Items.Select(MapToProductListResponse).ToList();
        return Ok(responses);
    }

    private static ProductResponse MapToProductResponse(ProductDto dto)
    {
        return new ProductResponse
        {
            Id = dto.Id,
            Name = dto.Description.Name,
            ShortDescription = dto.Description.ShortDescription,
            LongDescription = dto.Description.LongDescription,
            BrandId = dto.BrandId,
            CategoryId = dto.CategoryId,
            Status = dto.Status,
            Tags = dto.Tags,
            Images = dto.Images,
            Variants = dto.Variants
        };
    }

    private static ProductListResponse MapToProductListResponse(ProductListDto dto)
    {
        return new ProductListResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            ShortDescription = dto.ShortDescription,
            BrandId = dto.BrandId,
            CategoryId = dto.CategoryId,
            Status = dto.Status,
            MinPrice = dto.MinPrice,
            Currency = dto.Currency,
            TotalStock = dto.TotalStock
        };
    }
}