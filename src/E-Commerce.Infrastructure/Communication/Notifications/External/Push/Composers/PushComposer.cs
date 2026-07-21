using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;
using E_Commerce.Infrastructure.Communication.Notifications.Rendering;

namespace E_Commerce.Infrastructure.Communication.Notifications.External.Push.Composers;

/// <summary>
/// Composes a <see cref="PushMessage"/> from a typed notification model.
/// </summary>
public sealed class PushComposer
{
    private readonly RazorTemplateRenderer _renderer;

    public PushComposer(RazorTemplateRenderer renderer) => _renderer = renderer;

    public async Task<PushMessage> ComposeAsync<T>(T model, CancellationToken cancellationToken)
        where T : INotificationModel
    {
        var body = await _renderer.RenderAsync(model.TemplateName, model, cancellationToken);

        return new PushMessage
        {
            FirebaseInstallationId = GetRecipientId(model),   // maps to FID in infrastructure
            Title = GetTitle(model),
            Body = body
        };
    }

    /// <summary>
    /// Extracts the recipient identifier from the model.
    /// For the Application‑layer <c>PushNotification</c>, this is <c>RecipientId</c>.
    /// </summary>
    private static string GetRecipientId<T>(T model) =>
        (model?.GetType().GetProperty("RecipientId")?.GetValue(model) as string) ?? string.Empty;

    /// <summary>
    /// Extracts the push notification title from the model.
    /// </summary>
    private static string GetTitle<T>(T model) =>
        (model?.GetType().GetProperty("Title")?.GetValue(model) as string) ?? "Notification";
}