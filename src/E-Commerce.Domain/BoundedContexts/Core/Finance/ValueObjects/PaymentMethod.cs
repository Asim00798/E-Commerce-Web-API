using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.ValueObjects;

public sealed record PaymentMethod
{
    public PaymentMethodType Type { get; }

    public PaymentMethod(PaymentMethodType type)
    {
        if (!Enum.IsDefined(typeof(PaymentMethodType), type))
            throw new ArgumentException("Invalid payment method type.", nameof(type));

        Type = type;
    }
}