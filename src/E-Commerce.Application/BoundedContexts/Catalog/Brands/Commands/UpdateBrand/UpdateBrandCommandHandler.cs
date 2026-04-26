using MediatR;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;

public class UpdateBrandCommandHandler(
    IBrandRepository brandRepository) : IRequestHandler<UpdateBrandCommand, Unit>
{
    public async Task<Unit> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand == null) throw new NotFoundException(nameof(brand), request.Id);

        brand.Update(request.Name);
        await brandRepository.UpdateAsync(brand, cancellationToken);

        return Unit.Value;
    }
}
