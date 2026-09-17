using E_Commerce.Api.Attributes;
using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Catalog.Products.Requests;
using E_Commerce.Api.DTOs.v1.Catalog.Products.Responses;
using E_Commerce.Api.DTOs.v1.Shared;
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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Catalog;

/// <summary>
/// Manages product resources, including lifecycle, images, variants, tags, and stock.
/// Reads are available to any authenticated user.
/// Writes require the CatalogManager or Administrator role.
/// </summary>
[ApiController]
[Route("api/catalog/products")]
public sealed class ProductsController : BaseApiController
{
    private const string WriteRoles = "CatalogManager,Administrator";

    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    // ------------------------------------------------------------------
    // Create
    // ------------------------------------------------------------------

    [HttpPost]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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
            return ToValidationProblem(result.Errors);

        // Fetch the created product so the 201 body matches the shape
        // clients receive from GET /products/{id}.
        var queryResult = await _sender.Send(new GetProductByIdQuery(result.Data), ct);

        if (!queryResult.Succeeded)
        {
            // Product was created but cannot be re-read — surface a 201 with
            // a Location header and no body rather than masking the create.
            return CreatedAtAction(
                nameof(GetProductById),
                new { productId = result.Data },
                value: null);
        }

        return CreatedAtAction(
            nameof(GetProductById),
            new { productId = result.Data },
            MapToProductResponse(queryResult.Data!));
    }

    // ------------------------------------------------------------------
    // Lifecycle
    // ------------------------------------------------------------------

    [HttpPost("{productId:guid}/publish")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishProduct(Guid productId, CancellationToken ct)
    {
        var result = await _sender.Send(new PublishProductCommand(productId), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    [HttpPost("{productId:guid}/draft")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DraftProduct(Guid productId, CancellationToken ct)
    {
        var result = await _sender.Send(new DraftProductCommand(productId), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    [HttpPost("{productId:guid}/discontinue")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DiscontinueProduct(Guid productId, CancellationToken ct)
    {
        var result = await _sender.Send(new DiscontinueProductCommand(productId), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Images
    // ------------------------------------------------------------------

    [HttpPost("{productId:guid}/images")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddProductImage(
        Guid productId,
        [FromForm] AddProductImageRequest request,
        CancellationToken ct)
    {
        var image = new FileUpload(
            request.Image.OpenReadStream(),
            request.Image.FileName,
            request.Image.ContentType);

        var result = await _sender.Send(new AddProductImageCommand(productId, image), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    [HttpPost("{productId:guid}/images/{imageId:guid}/main")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetMainProductImage(
        Guid productId,
        Guid imageId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new SetMainProductImageCommand(productId, imageId), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveProductImage(
        Guid productId,
        Guid imageId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveProductImageCommand(productId, imageId), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Variants
    // ------------------------------------------------------------------

    [HttpPost("{productId:guid}/variants")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    [HttpPut("{productId:guid}/variants/{variantId:guid}/price")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveProductVariant(
        Guid productId,
        Guid variantId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveProductVariantCommand(productId, variantId), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Tags
    // ------------------------------------------------------------------

    [HttpPost("{productId:guid}/tags")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddProductTag(
        Guid productId,
        [FromBody] AddProductTagRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new AddProductTagCommand(productId, request.Tag), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // NOTE: DELETE with a request body is unreliable across clients and proxies.
    // If the client contract can change, prefer:
    //   [HttpDelete("{productId:guid}/tags/{tag}")] with a route parameter,
    // or [HttpDelete("{productId:guid}/tags")] with [FromQuery] string tag.
    [HttpDelete("{productId:guid}/tags")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveProductTag(
        Guid productId,
        [FromBody] RemoveProductTagRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveProductTagCommand(productId, request.Tag), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Stock
    // ------------------------------------------------------------------

    [HttpPost("{productId:guid}/variants/{variantId:guid}/stock/increase")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IncreaseProductStock(
        Guid productId,
        Guid variantId,
        [FromBody] IncreaseProductStockRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new IncreaseProductStockCommand(productId, variantId, request.Quantity), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/stock/decrease")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DecreaseProductStock(
        Guid productId,
        Guid variantId,
        [FromBody] DecreaseProductStockRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new DecreaseProductStockCommand(productId, variantId, request.Quantity), ct);
        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Reads
    // ------------------------------------------------------------------

    [HttpGet("{productId:guid}")]
    [Authorize]
    [CacheControl(Public = true, MaxAge = 600)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid productId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetProductByIdQuery(productId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToProductResponse(result.Data!));
    }

    [HttpGet]
    [Authorize]
    [CacheControl(Public = true, MaxAge = 600)]
    [ProducesResponseType(typeof(PaginatedResponse<ProductListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new ListProductsQuery(pageNumber, pageSize), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var page = result.Data!;

        return Ok(new PaginatedResponse<ProductListResponse>
        {
            Items = page.Items.Select(MapToProductListResponse).ToList(),
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalPages = page.TotalPages,
            TotalCount = page.TotalCount,
            HasPreviousPage = page.HasPreviousPage,
            HasNextPage = page.HasNextPage
        });
    }

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResponse<ProductListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new SearchProductsQuery(searchTerm, pageNumber, pageSize), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var page = result.Data!;

        return Ok(new PaginatedResponse<ProductListResponse>
        {
            Items = page.Items.Select(MapToProductListResponse).ToList(),
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalPages = page.TotalPages,
            TotalCount = page.TotalCount,
            HasPreviousPage = page.HasPreviousPage,
            HasNextPage = page.HasNextPage
        });
    }

    // ------------------------------------------------------------------
    // Mapping
    // ------------------------------------------------------------------

    // NOTE: This mapper assigns Application DTOs (ProductImageDto, ProductVariantDto)
    // directly to ProductResponse.Images / .Variants. This violates the API DTO
    // policy — the wire contract is owned by the Application layer here. See the
    // "Follow-up" section of the review. Fix requires new ProductImageResponse
    // and ProductVariantResponse API DTOs.
    private static ProductResponse MapToProductResponse(ProductDto dto) => new()
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

    private static ProductListResponse MapToProductListResponse(ProductListDto dto) => new()
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

    // ------------------------------------------------------------------
    // Error helpers
    // ------------------------------------------------------------------

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