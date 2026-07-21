using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;
using E_Commerce.Infrastructure.Communication.Notifications.Rendering;

namespace E_Commerce.Infrastructure.Communication.Notifications.External.Sms.Composers;

public sealed class SmsComposer
{
    private readonly RazorTemplateRenderer _renderer;

    public SmsComposer(RazorTemplateRenderer renderer) => _renderer = renderer;

    public async Task<SmsMessage> ComposeAsync<T>(T model, CancellationToken cancellationToken)
        where T : INotificationModel
    {
        var text = await _renderer.RenderAsync(model.TemplateName, model, cancellationToken);

        return new SmsMessage
        {
            PhoneNumber = GetPhoneNumber(model),
            Text = text
        };
    }

    private static string GetPhoneNumber<T>(T model) =>
        (model?.GetType().GetProperty("PhoneNumber")?.GetValue(model) as string) ?? string.Empty;
}