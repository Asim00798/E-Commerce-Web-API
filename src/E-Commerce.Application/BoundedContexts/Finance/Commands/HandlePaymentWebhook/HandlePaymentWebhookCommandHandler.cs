using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.HandlePaymentWebhook;

public sealed class HandlePaymentWebhookCommandHandler
    : IRequestHandler<HandlePaymentWebhookCommand, PaymentWebhookCommandResult>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HandlePaymentWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentWebhookCommandResult> Handle(
        HandlePaymentWebhookCommand command,
        CancellationToken ct)
    {
        try
        {
            var payment = await _paymentRepository.GetByProviderIntentionIdAsync(
                command.ProviderIntentionId!, ct);

            if (payment is null)
            {
                return PaymentWebhookCommandResult.Failure(
                    "Payment not found.",
                    PaymentWebhookErrorType.Transient);
            }

            if (IsAlreadyFinalized(payment))
                return PaymentWebhookCommandResult.Success();

            if (command.Success)
            {
                return await CapturePaymentIfAwaitingAsync(
                    payment, command.ProviderTransactionId, ct);
            }

            await FailPaymentIfPossibleAsync(payment, ct);
            return PaymentWebhookCommandResult.Success();
        }
        catch (DomainException ex)
        {
            return PaymentWebhookCommandResult.Failure(
                ex.Message,
                PaymentWebhookErrorType.Validation);
        }
    }

    private static bool IsAlreadyFinalized(PaymentAggregate payment)
    {
        return payment.Status is PaymentStatus.Captured or PaymentStatus.Failed;
    }

    private async Task<PaymentWebhookCommandResult> CapturePaymentIfAwaitingAsync(
        PaymentAggregate payment,
        string providerTransactionId,
        CancellationToken ct)
    {
        if (payment.Status == PaymentStatus.Pending)
        {
            // Tx2 hasn't committed yet; provider retry will find AwaitingPayment.
            return PaymentWebhookCommandResult.Failure(
                "Payment not yet ready for capture.",
                PaymentWebhookErrorType.Transient);
        }

        if (payment.Status != PaymentStatus.AwaitingPayment)
            return PaymentWebhookCommandResult.Success();

        payment.Capture(providerTransactionId);
        await _paymentRepository.UpdateAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return PaymentWebhookCommandResult.Success();
    }

    private async Task FailPaymentIfPossibleAsync(
        PaymentAggregate payment,
        CancellationToken ct)
    {
        if (payment.Status is not (PaymentStatus.Pending or PaymentStatus.AwaitingPayment))
        {
            return;
        }

        payment.Fail();
        await _paymentRepository.UpdateAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}