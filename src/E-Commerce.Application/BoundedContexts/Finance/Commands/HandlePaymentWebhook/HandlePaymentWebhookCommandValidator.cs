using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.HandlePaymentWebhook;

public sealed class HandlePaymentWebhookCommandValidator : AbstractValidator<HandlePaymentWebhookCommand>
{
    public HandlePaymentWebhookCommandValidator()
    {
        RuleFor(x => x.Provider).NotEmpty();
        RuleFor(x => x.ProviderIntentionId).NotEmpty();
        RuleFor(x => x.ProviderTransactionId).NotEmpty();
    }
}