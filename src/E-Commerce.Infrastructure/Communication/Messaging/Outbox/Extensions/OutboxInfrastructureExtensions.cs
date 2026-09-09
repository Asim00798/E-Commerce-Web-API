using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Dispatching;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Decorators;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Implementation;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Processing;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Serialization;
using E_Commerce.Infrastructure.Persistence.Modules.Outbox.Repositories;
using E_Commerce.Infrastructure.Persistence.Modules.Outbox.Repository;
using E_Commerce.Infrastructure.Persistence.Outbox.Repository;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Extensions;

public static class OutboxInfrastructureExtensions
{
    public static IServiceCollection AddOutboxMessaging(this IServiceCollection services)
    {
        /// <summary>
        /// Repositories auto registration handled by <see cref="RepositoryRegistrationExtensions"/>
        ///services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        ///services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();
        ///Those repositories are registered in the <see cref="RepositoryRegistrationExtensions"/> class, so we don't need to register them here.
        /// </summary>
        services.AddScoped<IOutboxMessageWriter, OutboxMessageWriter>();
        services.AddSingleton<OutboxSerializer>();
        services.AddScoped<OutboxDispatchService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();        
        services.AddScoped<IDeadLetterRepository, DeadLetterRepository>();
        // Apply the enrichment decorator (using Scrutor or manual)
        services.TryDecorate<IOutboxMessageWriter, EnrichedOutboxMessageWriter>();

        return services;
    }
}
