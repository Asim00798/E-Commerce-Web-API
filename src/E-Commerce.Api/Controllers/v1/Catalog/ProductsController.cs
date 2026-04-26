using E_Commerce.Api.Controllers;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.DeleteProduct;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.GetProductById;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.ListProducts;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.SearchProducts;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

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
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}
