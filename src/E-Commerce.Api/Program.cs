using E_Commerce.Api.Extensions;
using E_Commerce.Application.DependencyInjection;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Infrastructure.Communication.Realtime.Extensions;
using E_Commerce.Infrastructure.DependencyInjection;
using E_Commerce.Infrastructure.Extensions;
using E_Commerce.Infrastructure.Observability.HealthChecks.Extensions;
using E_Commerce.Infrastructure.Observability.Logging;
using E_Commerce.Infrastructure.Observability.Metrics;
using E_Commerce.Infrastructure.Observability.Tracing.Extensions;
using E_Commerce.Infrastructure.Scheduling.Extensions;
using E_Commerce.ReadModel.Infrastructure.DependencyInjection;
using Serilog;

// ========== Create Builder ==========
var builder = WebApplication.CreateBuilder(args);

// ========== Observability ==========
builder.AddInfrastructureLogging();
builder.Services.AddApplicationHealthChecks(builder.Configuration);
builder.Services.AddApplicationMetrics();
builder.Services.AddApplicationTracing();

// ========== HTTP ==========
builder.Services.AddHttpContextAccessor();

// ========== Application / Infrastructure / ReadModel ==========
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration, typeof(InfrastructureServiceRegistration).Assembly);
builder.Services.AddReadModel(builder.Configuration);

// ========== Controllers & API Explorer ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========== API-level configuration ==========
builder.Services.AddApiConfiguration(builder.Configuration);
builder.Services.AddApiVersioningConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddHttpsConfiguration();
builder.Services.AddProductionRateLimiting(builder.Configuration);
builder.Services.AddLowercaseRouting();
builder.Services.AddHttpCaching();
builder.Services.AddForwardedHeadersConfiguration(builder.Configuration);

// ========== Build ==========
var app = builder.Build();

// ========== Middleware pipeline (order matters) ==========
app.UseForwardedHeadersConfiguration();
app.UseCorrelationId();
app.UseSerilogRequestLogging();
app.UseSecurityHeaders();
app.UseGlobalExceptionHandler();

app.UseDevelopmentSwagger();

app.UseHttpsConfiguration(app.Environment);
app.UseCorsConfiguration();
app.UseProductionRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

// ========== Endpoints ==========
app.MapHealthChecks("/health");
app.MapSignalRRealTimeHub();
app.MapControllers();

// ========== Run ==========
try
{
    await app.ApplyMigrationsAsync();
    app.Services.ApplyReadModelMigrations();

    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.ScheduleRecurringJobs(
            typeof(IRecurringJobTrigger).Assembly,                      // Application
            typeof(InfrastructureServiceRegistration).Assembly);        // Infrastructure
    }

    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}