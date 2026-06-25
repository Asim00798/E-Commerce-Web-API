using E_Commerce.Api.Extensions;
using E_Commerce.Application;
using E_Commerce.Infrastructure;
using E_Commerce.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// ========== Configure Logging ==========
builder.AddInfrastructureLogging();
// ========== Serilog Configuration ==========
var appName = builder.Configuration["App:Name"] ?? "E-Commerce.Api";
var environment = builder.Environment.EnvironmentName;
var version = builder.Configuration["App:Version"] ?? "1.0.0";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("AppName", appName)
    .Enrich.WithProperty("Environment", environment)
    .Enrich.WithProperty("Version", version)
    .WriteTo.Console()                                      // console sink
    .WriteTo.File(
        path: "logs/ecommerce-.log",                        // rolling file sink
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,                          // keep 7 days of logs
        fileSizeLimitBytes: 10 * 1024 * 1024,               // 10 MB per file
        rollOnFileSizeLimit: true)
    .CreateLogger();

builder.Host.UseSerilog();

// ========== Register Services ==========
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiVersioningExtension();
builder.Services.AddSwaggerExtension();
builder.Services.AddCorsExtension(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
    });
}

// Order matters: CorrelationIdMiddleware BEFORE SerilogRequestLogging
app.UseCorrelationIdMiddleware();
app.UseSerilogRequestLogging();

app.UseGlobalExceptionMiddleware();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
