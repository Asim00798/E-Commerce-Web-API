using RazorLight;

namespace E_Commerce.Infrastructure.Communication.Notifications.Rendering;

/// <summary>
/// Compiles Razor templates at runtime and renders them with a given model.
/// Thread‑safe, designed to be registered as a singleton.
/// </summary>
public sealed class RazorTemplateRenderer
{
    private readonly RazorLightEngine _engine;

    /// <summary>
    /// Initialises the renderer for a specific template directory.
    /// </summary>
    /// <param name="templateRootPath">Directory containing .cshtml templates.</param>
    public RazorTemplateRenderer(string templateRootPath)
    {
        if (string.IsNullOrWhiteSpace(templateRootPath))
            throw new ArgumentNullException(nameof(templateRootPath));
        if (!Directory.Exists(templateRootPath))
            throw new DirectoryNotFoundException($"Template root directory not found: {templateRootPath}");

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templateRootPath)   // templates are on disk
            .UseMemoryCachingProvider()               // cache compiled templates
            .Build();
    }

    /// <summary>
    /// Renders a template using the provided model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="templateName">Template file name without .cshtml extension.</param>
    /// <param name="model">Data passed to the template.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Rendered string.</returns>
    public async Task<string> RenderAsync<T>(string templateName, T model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentNullException(nameof(templateName));

        // RazorLight throws if the template is missing or invalid – let it bubble up.
        return await _engine.CompileRenderAsync($"{templateName}.cshtml", model);
    }
}