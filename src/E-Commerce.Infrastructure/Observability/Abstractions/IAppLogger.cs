namespace E_Commerce.Infrastructure.Observability.Abstractions;

public interface IAppLogger
{
    void Info(string message, params object[] args);
    void Warn(string message, params object[] args);
    void Error(Exception ex, string message, params object[] args);
}