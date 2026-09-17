using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Dispatching;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Configuration;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Decorators;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Implementation;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Processing;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Serialization;
using E_Commerce.Infrastructure.Persistence.Modules.Outbox.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Extensions;

/// <summary>
/// Registers the transactional outbox pipeline: writer, dispatcher,
/// serializer, dispatch service, and enrichment decorator.
///
/// Repository implementations (<c>IOutboxMessageRepository</c>,
/// <c>IProcessedEventRepository</c>) are registered by
/// <c>RepositoryRegistrationExtensions</c> via assembly scanning.
/// </summary>
public static class OutboxInfrastructureExtensions
{
    public static IServiceCollection AddOutboxMessaging(this IServiceCollection services)
    {
        // -----------------------------------------------------------------
        // Options
        // -----------------------------------------------------------------
        services.AddOptions<OutboxOptions>()
            .BindConfiguration(OutboxOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // -----------------------------------------------------------------
        // Outbox core
        // -----------------------------------------------------------------
        services.AddScoped<IOutboxMessageWriter, OutboxMessageWriter>();
        services.AddSingleton<OutboxSerializer>();
        services.AddScoped<OutboxDispatchService>();

        // -----------------------------------------------------------------
        // Event dispatchers
        // -----------------------------------------------------------------
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        // -----------------------------------------------------------------
        // Dead letter
        // -----------------------------------------------------------------
        services.AddScoped<IDeadLetterRepository, DeadLetterRepository>();

        // -----------------------------------------------------------------
        // Decorators — must run after the base registrations
        // -----------------------------------------------------------------
        services.TryDecorate<IOutboxMessageWriter, EnrichedOutboxMessageWriter>();

        return services;
    }
}