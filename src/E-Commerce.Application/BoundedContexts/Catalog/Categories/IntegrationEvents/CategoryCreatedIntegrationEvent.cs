using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.IntegrationEvents;

public record CategoryCreatedIntegrationEvent(Guid Id, string Name) : INotification;
