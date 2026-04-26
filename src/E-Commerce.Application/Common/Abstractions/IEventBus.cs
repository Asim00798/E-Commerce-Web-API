namespace E_Commerce.Application.Common.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class;
}
