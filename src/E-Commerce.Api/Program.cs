using E_Commerce.Api.Extensions;
using E_Commerce.Application.DependencyInjection;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Infrastructure.Communication.Realtime.Hubs;
using E_Commerce.Infrastructure.DependencyInjection;
using E_Commerce.Infrastructure.Extensions;
using E_Commerce.Infrastructure.Observability.Logging;
using E_Commerce.Infrastructure.Scheduling.Extensions;
using E_Commerce.ReadModel.Infrastructure.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ========== Observability ==========
builder.AddInfrastructureLogging();

// ========== HTTP ==========
builder.Services.AddHttpContextAccessor();

// ========== Application / Infrastructure / ReadModel ==========
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
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

// ========== SignalR ==========
//builder.Services.AddSignalR();

var app = builder.Build();

// ========== Apply startup DB migrations for EF and ReadModel ==========
await app.ApplyMigrationsAsync();
app.Services.ApplyReadModelMigrations();

// ========== Schedule recurring jobs ==========
//using (var scope = app.Services.CreateScope())
//{
//    scope.ServiceProvider.ScheduleRecurringJobs(typeof(IRecurringJobTrigger).Assembly);
//}

// ========== Middleware pipeline (order matters) ==========
//
// Ordering rules:
//
//   Correlation   — outermost. Pushes the correlation-ID log scope so every
//                   downstream log entry (including Serilog's request completion
//                   log) carries the ID.
//
//   Serilog       — inside correlation. Logs the final HTTP status after all
//                   transformations (exception handling, status rewriting).
//
//   SecurityHeaders — sets hardening headers on all responses.
//
//   Exception     — transforms exceptions into ProblemDetails responses.
//                   Must be inside Serilog so that Serilog observes the final
//                   status code; must be outside Swagger so Swagger errors are
//                   also handled.
//
app.UseForwardedHeadersConfiguration();
app.UseCorrelationId();               // outermost — log scope wraps everything below
app.UseSerilogRequestLogging();       // inside correlation — request logs carry correlation ID
app.UseSecurityHeaders();
app.UseGlobalExceptionHandler();      // inside Serilog — transforms exceptions into HTTP responses

// Swagger before auth so the UI is reachable without a token,
// but inside correlation + exception handling.
app.UseDevelopmentSwagger();

app.UseHttpsConfiguration(app.Environment);
app.UseCorsConfiguration();
app.UseProductionRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

// ========== Endpoints ==========
//app.MapHealthChecks("/health");
//app.MapHub<NotificationHub>("/hubs/notification");
app.MapControllers();

// ========== Run ==========
try
{
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