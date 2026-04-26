using E_Commerce.Api.Controllers;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.CreateCategory;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.UpdateCategory;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.GetCategoryById;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.ListCategories;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace E_Commerce.Api.Controllers.v1.Catalog;

[ApiVersion("1.0")]
public class CategoriesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ListCategoriesQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetCategoryByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }
}
