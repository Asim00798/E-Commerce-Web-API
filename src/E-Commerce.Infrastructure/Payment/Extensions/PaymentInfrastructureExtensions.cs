using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Infrastructure.Payment.Configuration;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Client;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Gateway;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Webhooks;
using E_Commerce.Infrastructure.Persistence.Modules.Finance.Repositories;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Payment.Extensions;

public static class PaymentInfrastructureExtensions
{
    public static IServiceCollection AddFinancePayment(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var paymentSection = configuration.GetSection(PaymentOptions.SectionName);
        var paymentOptions = paymentSection.Get<PaymentOptions>() ?? new PaymentOptions();

        services.AddSingleton<IValidateOptions<PaymobOptions>, PaymobOptionsValidator>();

        services.AddOptions<PaymobOptions>()
            .Bind(paymentSection.GetSection(nameof(paymentOptions.Paymob)))
            .ValidateOnStart();

        services.AddHttpClient<PaymobApiClient>();

        if (!string.Equals(paymentOptions.Provider, "Paymob", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unsupported payment provider '{paymentOptions.Provider}'. Valid provider: 'Paymob'.");
        }

        services.AddScoped<IPaymentGateway, PaymobPaymentGateway>();

        services.AddScoped<PaymobHmacVerifier>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PaymobOptions>>().Value;
            return new PaymobHmacVerifier(options.WebhookSecret);
        });

        services.AddScoped<IPaymentWebhookProcessor, PaymobWebhookProcessor>();

        /// <summary>
        /// Repositories registration for the Finance bounded context.
        /// Such as <see cref="IPaymentRepository"/> and <see cref="IRefundRepository"/>
        /// They are handled by automatic assembly scanning in <see cref="RepositoryRegistrationExtensions"/>
        /// </summary>

        return services;
    }
}