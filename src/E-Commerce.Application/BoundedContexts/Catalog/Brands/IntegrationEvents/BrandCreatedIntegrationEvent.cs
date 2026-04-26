using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.IntegrationEvents;

public record BrandCreatedIntegrationEvent(Guid Id, string Name) : INotification;
