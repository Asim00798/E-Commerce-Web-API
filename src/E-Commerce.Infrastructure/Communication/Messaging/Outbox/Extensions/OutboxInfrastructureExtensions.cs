using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Persistence;
using E_Commerce.Infrastructure.Communication.Messaging.Dispatching;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Implementation;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Processing;
using E_Commerce.Infrastructure.Communication.Messaging.Serialization;
using E_Commerce.Infrastructure.Persistence.Modules.Outbox.Repositories;
using E_Commerce.Infrastructure.Persistence.Outbox.Repository;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Extensions;

public static class OutboxInfrastructureExtensions
{
    public static IServiceCollection AddOutboxMessaging(this IServiceCollection services)
    {
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();  // OutboxRepository will be created in Persistence
        services.AddScoped<IOutboxMessageWriter, OutboxMessageWriter>();
        services.AddSingleton<OutboxSerializer>();
        services.AddScoped<OutboxDispatchService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        // ProcessedEventRepository will be created in Persistence,
        // but we need to register it here for the OutboxProcessor
        services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();

        return services;
    }
}
