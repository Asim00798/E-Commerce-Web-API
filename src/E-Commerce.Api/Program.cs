using E_Commerce.Api.Extensions;
using E_Commerce.Application.DependencyInjection;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Infrastructure.Communication.Realtime.Hubs;
using E_Commerce.Infrastructure.DependencyInjection;
using E_Commerce.Infrastructure.Observability.Logging;
using E_Commerce.Infrastructure.Scheduling.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ========== Observability ==========
builder.AddInfrastructureLogging();

// ========== HTTP ==========
builder.Services.AddHttpContextAccessor();

// ========== Application / Infrastructure ==========
//builder.Services.AddApplication();
//builder.Services.AddInfrastructure(builder.Configuration);

// ========== API-level configuration ==========
builder.Services.AddApiConfiguration(builder.Configuration);
builder.Services.AddApiVersioningConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddHttpsConfiguration();
builder.Services.AddProductionRateLimiting(builder.Configuration);
builder.Services.AddLowercaseRouting();
builder.Services.AddHttpCaching();

// ========== Controllers & SignalR ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSignalR();

var app = builder.Build();

// ========== Schedule recurring jobs ==========
//using (var scope = app.Services.CreateScope())
//{
//    scope.ServiceProvider.ScheduleRecurringJobs(typeof(IRecurringJobTrigger).Assembly);
//}

// ========== Swagger (dev only) — before auth so UI is always reachable ==========
app.UseDevelopmentSwagger();

// ========== Middleware pipeline (order matters) ==========
app.UseSecurityHeaders();
app.UseCorrelationId();
app.UseGlobalExceptionHandler();

app.UseSerilogRequestLogging();

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