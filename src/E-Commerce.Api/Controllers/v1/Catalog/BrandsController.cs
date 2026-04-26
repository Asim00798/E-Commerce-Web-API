using E_Commerce.Api.Controllers;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.GetBrandById;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace E_Commerce.Api.Controllers.v1.Catalog;

[ApiVersion("1.0")]
public class BrandsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ListBrandsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetBrandByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBrandCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBrandCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }
}
