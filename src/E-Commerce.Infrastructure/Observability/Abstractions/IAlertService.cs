namespace E_Commerce.Infrastructure.Observability.Abstractions;

public interface IAlertService
{
    Task SendAsync(Alert alert, CancellationToken ct = default);
}