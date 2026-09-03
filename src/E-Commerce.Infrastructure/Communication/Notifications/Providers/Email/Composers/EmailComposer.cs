using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;
using E_Commerce.Infrastructure.Communication.Notifications.Rendering;

namespace E_Commerce.Infrastructure.Communication.Notifications.Providers.Email.Composers;

public sealed class EmailComposer
{
    private readonly RazorTemplateRenderer _renderer;

    public EmailComposer(RazorTemplateRenderer renderer) => _renderer = renderer;

    public async Task<EmailMessage> ComposeAsync<T>(T model, CancellationToken cancellationToken)
        where T : IEmailNotificationModel
    {
        var body = await _renderer.RenderAsync(model.TemplateName, model, cancellationToken);

        return new EmailMessage
        {
            To = GetRecipient(model),
            Subject = GetSubject(model),
            Body = body,
            IsHtml = true
        };
    }

    private static string GetRecipient<T>(T model) =>
        (model?.GetType().GetProperty("RecipientEmail")?.GetValue(model) as string) ?? string.Empty;

    private static string GetSubject<T>(T model) =>
        (model?.GetType().GetProperty("Subject")?.GetValue(model) as string) ?? "Notification";
}