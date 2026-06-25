using E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Application.Catalog.Products.Commands.CreateProduct;
public class CreateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _dispatcher;

    public CreateProductHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher dispatcher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
    }

    public async Task Handle(CreateProductCommand command)
    {
        var product = new Product(new ProductDescription(command.Name), Guid.NewGuid());

        await _repository.AddAsync(product);

        await _unitOfWork.SaveChangesAsync();

        // IMPORTANT STEP
        await _dispatcher.DispatchAsync(product.DomainEvents);

        product.ClearDomainEvents();
    }
}