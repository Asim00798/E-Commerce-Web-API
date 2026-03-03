namespace E_Commerce.Domain.SharedKernel.Services.Payments;

public class PaymentPolicyService : IPaymentPolicyService
{
    public bool IsPaymentAllowed(decimal amount)
    {
        // Placeholder implementation
        return amount > 0;
    }
}
