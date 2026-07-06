namespace E_Commerce.Infrastructure.Observability.Abstractions;

public interface IAlertService
{
    Task SendAsync(string message);
}