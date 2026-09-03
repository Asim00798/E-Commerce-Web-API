using System.Net.Http;
using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Time;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using RefundAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors.Refund;
using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcileRefunds;

public sealed class ReconcileRefundsCommandHandler
    : IRequestHandler<ReconcileRefundsCommand, Result>
{
    private readonly IRefundRepository _refundRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;
    private readonly ILogger<ReconcileRefundsCommandHandler> _logger;

    public ReconcileRefundsCommandHandler(
        IRefundRepository refundRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        IDateTime dateTime,
        ILogger<ReconcileRefundsCommandHandler> logger)
    {
        _refundRepository = refundRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ReconcileRefundsCommand command,
        CancellationToken ct)
    {
        var cutoff = CalculateRefundReconciliationCutoff();

        var stuckRefunds = await _refundRepository.GetProcessingOlderThanAsync(
            cutoff,
            command.BatchSize,
            ct);

        foreach (var refund in stuckRefunds)
        {
            ct.ThrowIfCancellationRequested();
            await ReconcileRefundAsync(refund, ct);
        }

        return Result.Success();
    }

    private DateTime CalculateRefundReconciliationCutoff()
    {
        return _dateTime.UtcNow.AddMinutes(-15);
    }

    private async Task ReconcileRefundAsync(
        RefundAggregate refund,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refund.ProviderTransactionId))
            {
                // Without a provider refund ID, we cannot query status.
                // Requeue for another attempt.
                await RequeueRefundAsync(refund, ct);
                return;
            }

            var payment = await LoadPaymentForRefundAsync(refund.PaymentId, ct);

            if (payment is null)
            {
                await RequeueRefundAsync(refund, ct);
                return;
            }

            var providerReference = BuildProviderReference(payment, refund);
            var status = await _paymentGateway.GetRefundStatusAsync(providerReference, ct);

            await ApplyRefundStatusAsync(refund, payment, status, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Provider transport failure while reconciling refund {RefundId}",
                refund.Id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Invalid provider operation while reconciling refund {RefundId}",
                refund.Id);
        }
    }

    private async Task<PaymentAggregate?> LoadPaymentForRefundAsync(
        Guid paymentId,
        CancellationToken ct)
    {
        return await _paymentRepository.GetByIdAsync(paymentId, ct);
    }

    private async Task RequeueRefundAsync(
        RefundAggregate refund,
        CancellationToken ct)
    {
        refund.Requeue();

        await _refundRepository.UpdateAsync(refund, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static PaymentProviderReference BuildProviderReference(
        PaymentAggregate payment,
        RefundAggregate refund)
    {
        return new PaymentProviderReference
        {
            Provider = payment.Provider,
            IntentionId = payment.ProviderIntentionId,
            // The provider-side identifier for the refund itself.
            TransactionId = refund.ProviderTransactionId
        };
    }

    private async Task ApplyRefundStatusAsync(
        RefundAggregate refund,
        PaymentAggregate payment,
        RefundStatusResult status,
        CancellationToken ct)
    {
        if (status.Outcome == RefundOutcome.Succeeded)
        {
            refund.Complete();
            payment.ApplyRefund(refund.Amount);

            await _refundRepository.UpdateAsync(refund, ct);
            await _paymentRepository.UpdateAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return;
        }

        if (status.Outcome == RefundOutcome.Failed)
        {
            refund.Fail(status.ErrorMessage);

            await _refundRepository.UpdateAsync(refund, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return;
        }

        // Unknown outcome: leave as Processing for the next reconciliation cycle.
    }
}