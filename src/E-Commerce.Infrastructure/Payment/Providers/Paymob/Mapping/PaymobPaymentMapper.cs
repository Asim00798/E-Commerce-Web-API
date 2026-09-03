using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Webhooks;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Mapping;

public static class PaymobPaymentMapper
{
    public static PaymentStatusResult ToPaymentStatusResult(PaymobStatusResponse response)
    {
        if (response.Success)
            return new PaymentStatusResult
            {
                Status = PaymentStatus.Captured,
                ProviderTransactionId = response.TransactionId,
                ProviderMessage = null
            };

        if (response.Pending)
            return new PaymentStatusResult
            {
                Status = PaymentStatus.AwaitingPayment,
                ProviderTransactionId = response.TransactionId,
                ProviderMessage = "Payment is pending."
            };

        return new PaymentStatusResult
        {
            Status = PaymentStatus.Failed,
            ProviderTransactionId = response.TransactionId,
            ProviderMessage = "Payment failed."
        };
    }
}