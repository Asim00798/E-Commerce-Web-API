using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Infrastructure.Observability.Abstractions;

public interface IHealthCheckService
{
    Task<HealthReport> CheckAsync(CancellationToken ct = default);
}