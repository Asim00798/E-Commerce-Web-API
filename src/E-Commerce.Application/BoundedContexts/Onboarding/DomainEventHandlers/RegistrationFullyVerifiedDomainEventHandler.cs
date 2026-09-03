using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;                    // IAppContext
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events;

namespace E_Commerce.Application.BoundedContexts.Onboarding.DomainEventHandlers;

public sealed class RegistrationFullyVerifiedDomainEventHandler
    : IDomainEventHandler<RegistrationFullyVerifiedDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;

    public RegistrationFullyVerifiedDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
    }

    public async Task Handle(RegistrationFullyVerifiedDomainEvent domainEvent, CancellationToken ct)
    {
        var integrationEvent = new RegistrationFullyVerifiedIntegrationEvent(
            domainEvent.RegistrationId,
            domainEvent.Email,
            domainEvent.PhoneNumber,
            domainEvent.Username)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}