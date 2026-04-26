using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.IntegrationEvents;

public record ProductCreatedIntegrationEvent(Guid ProductId, string Name) : INotification;
