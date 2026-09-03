using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using RefundAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors.Refund;
using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ProcessRefund;

public sealed class ProcessRefundCommandHandler
    : IRequestHandler<ProcessRefundCommand, Result>
{
    private readonly IRefundRepository _refundRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessRefundCommandHandler(
        IRefundRepository refundRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork)
    {
        _refundRepository = refundRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ProcessRefundCommand command,
        CancellationToken ct)
    {
        try
        {
            return await ProcessRefundAsync(command, ct);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ProcessRefundAsync(
        ProcessRefundCommand command,
        CancellationToken ct)
    {
        var refund = await LoadRefundAsync(command.RefundId, ct);

        if (refund is null)
            return Result.Failure("Refund not found.");

        if (IsRefundFinal(refund))
            return Result.Success();

        var claimResult = await ClaimRefundForProcessingAsync(refund, ct);

        if (!claimResult.Succeeded)
            return Result.Failure(claimResult.Errors);

        refund = claimResult.Data!;

        var paymentResult = await LoadPaymentOrRequeueAsync(refund, ct);

        if (!paymentResult.Succeeded)
            return Result.Failure(paymentResult.Errors);

        var payment = paymentResult.Data!;

        var providerResult = await RequestProviderRefundAsync(
            payment,
            refund,
            ct);

        return await ApplyProviderRefundResultAsync(
            refund,
            payment,
            providerResult,
            ct);
    }

    private async Task<RefundAggregate?> LoadRefundAsync(
        Guid refundId,
        CancellationToken ct)
    {
        return await _refundRepository.GetByIdAsync(refundId, ct);
    }

    private static bool IsRefundFinal(RefundAggregate refund)
    {
        return refund.Status is RefundStatus.Completed or RefundStatus.Failed;
    }

    private async Task<Result<RefundAggregate>> ClaimRefundForProcessingAsync(
        RefundAggregate refund,
        CancellationToken ct)
    {
        if (refund.Status == RefundStatus.Requested)
            return await ClaimRequestedRefundAsync(refund, ct);

        if (refund.Status == RefundStatus.Processing)
            return Result<RefundAggregate>.Failure("Refund is already in processing state.");

        return Result<RefundAggregate>.Failure("Refund is not in a processable state.");
    }

    private async Task<Result<RefundAggregate>> ClaimRequestedRefundAsync(
        RefundAggregate refund,
        CancellationToken ct)
    {
        var claimed = await _refundRepository.TryMarkProcessingAsync(refund.Id, ct);

        if (!claimed)
            return Result<RefundAggregate>.Failure("Refund is already being processed.");

        var updated = await _refundRepository.GetByIdAsync(refund.Id, ct);

        if (updated is null || updated.Status != RefundStatus.Processing)
            return Result<RefundAggregate>.Failure("Refund is already being processed.");

        return Result<RefundAggregate>.Success(updated);
    }

    private async Task<Result<PaymentAggregate>> LoadPaymentOrRequeueAsync(
        RefundAggregate refund,
        CancellationToken ct)
    {
        var payment = await _paymentRepository.GetByIdAsync(refund.PaymentId, ct);

        if (payment is not null)
            return Result<PaymentAggregate>.Success(payment);

        await RequeueRefundAsync(refund, ct);

        return Result<PaymentAggregate>.Failure("Payment not found.");
    }

    private async Task RequeueRefundAsync(
        RefundAggregate refund,
        CancellationToken ct)
    {
        refund.Requeue();

        await _refundRepository.UpdateAsync(refund, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<RefundResult> RequestProviderRefundAsync(
        PaymentAggregate payment,
        RefundAggregate refund,
        CancellationToken ct)
    {
        var reference = BuildPaymentProviderReference(payment);

        return await _paymentGateway.RefundAsync(
            reference,
            refund.Amount,
            ct);
    }

    private static PaymentProviderReference BuildPaymentProviderReference(
        PaymentAggregate payment)
    {
        return new PaymentProviderReference
        {
            Provider = payment.Provider,
            IntentionId = payment.ProviderIntentionId,
            TransactionId = payment.ProviderTransactionId
        };
    }

    private async Task<Result> ApplyProviderRefundResultAsync(
    RefundAggregate refund,
    PaymentAggregate payment,
    RefundResult providerResult,
    CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(providerResult.ProviderTransactionId))
        {
            refund.SetProviderTransactionId(providerResult.ProviderTransactionId);
        }

        if (providerResult.Succeeded)
        {
            refund.Complete();
            payment.ApplyRefund(refund.Amount);

            await _refundRepository.UpdateAsync(refund, ct);
            await _paymentRepository.UpdateAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        if (providerResult.Outcome == RefundOutcome.Unknown)
        {
            await _refundRepository.UpdateAsync(refund, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Failure(
                providerResult.ErrorMessage ?? "Refund outcome unknown. Reconciliation required.");
        }

        refund.Fail(providerResult.ErrorMessage);

        await _refundRepository.UpdateAsync(refund, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Failure(providerResult.ErrorMessage ?? "Refund failed.");
    }
}