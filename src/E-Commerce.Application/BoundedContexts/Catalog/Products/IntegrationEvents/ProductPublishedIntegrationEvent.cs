using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.IntegrationEvents;

public record ProductPublishedIntegrationEvent(Guid ProductId, string Name) : INotification;
