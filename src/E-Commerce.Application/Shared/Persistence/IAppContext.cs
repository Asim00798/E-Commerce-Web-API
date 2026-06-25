namespace E_Commerce.Application.Shared.Persistence;

public interface IAppContext
{
    // Static metadata
    string AppName { get; }
    string Environment { get; }
    string Version { get; }
    string MachineName { get; }

    // Request-scoped (available during HTTP requests)
    string? CorrelationId { get; }
    string? UserId { get; }
}
