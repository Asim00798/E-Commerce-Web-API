using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Unit>;
