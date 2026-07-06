using E_Commerce.Application.Common.Constants;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace E_Commerce.Infrastructure.Runtime;

public class ExecutionContext : IAppContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ICurrentUser _currentUser;

    public ExecutionContext(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        ICurrentUser currentUser)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
        _currentUser = currentUser;
    }

    public string AppName => _configuration["App:Name"] ?? "E-Commerce.Api";
    public string Environment => _hostEnvironment.EnvironmentName;
    public string Version => _configuration["App:Version"] ?? "1.0.0";
    public string MachineName => System.Environment.MachineName;

    public string? CorrelationId =>
        _httpContextAccessor.HttpContext?.Items[ContextKeys.CorrelationId]?.ToString();

    public string? UserId => _currentUser.UserId.ToString();
}
