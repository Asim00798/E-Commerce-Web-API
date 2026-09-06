using Asp.Versioning;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductDescription;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.GetProductById;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.ListProducts;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.SearchProducts;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.v1.Catalog;

[ApiVersion("1.0")]
public class ProductsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ListProductsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchProductsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductDescriptionCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
}
