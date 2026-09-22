using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace E_Commerce.Infrastructure.Persistence.Context;

/// <summary>
/// Factory for design-time DbContext creation.
/// Used by the EF Core tooling (<c>dotnet ef</c>) to construct an
/// <see cref="AppDbContext"/> when running migrations from the CLI.
///
/// The factory reads configuration from the Api project's output directory
/// so it sees the same appsettings files the application uses at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // The EF CLI runs from the solution root; the Api project's appsettings
        // files live under E-Commerce.Api/. Resolve them relative to this assembly
        // (which is E-Commerce.Infrastructure), then point at the Api project.
        var basePath = ResolveApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. " +
                "Ensure appsettings.json or appsettings.Development.json " +
                "in the Api project contains the connection string.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    // ------------------------------------------------------------------
    // Path resolution
    // ------------------------------------------------------------------

    /// <summary>
    /// Resolves the absolute path to the Api project so the configuration
    /// builder can find its appsettings files. The path is computed from the
    /// Infrastructure assembly's location — <c>E-Commerce.Infrastructure/bin/{config}/{tfm}/</c>
    /// — then walked up to the solution root and into <c>E-Commerce.Api/</c>.
    /// </summary>
    private static string ResolveApiProjectPath()
    {
        // Starting point: .../src/E-Commerce.Infrastructure/bin/Debug/net8.0/
        var assemblyLocation = AppContext.BaseDirectory;

        // Walk up three levels: net8.0 -> Debug -> bin
        var infraProjectDir = Path.GetFullPath(
            Path.Combine(assemblyLocation, "..", "..", ".."));

        // Walk up one more: E-Commerce.Infrastructure -> src
        var solutionDir = Path.GetFullPath(
            Path.Combine(infraProjectDir, ".."));

        var apiProjectDir = Path.Combine(solutionDir, "E-Commerce.Api");

        if (!Directory.Exists(apiProjectDir))
        {
            throw new DirectoryNotFoundException(
                $"Could not locate the Api project at '{apiProjectDir}'. " +
                "Verify the folder layout matches the expected solution structure.");
        }

        return apiProjectDir;
    }
}