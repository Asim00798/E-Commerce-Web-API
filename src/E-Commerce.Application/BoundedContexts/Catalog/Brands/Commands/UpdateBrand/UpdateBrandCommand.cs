using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;

public record UpdateBrandCommand(Guid Id, string Name) : IRequest<Unit>;
