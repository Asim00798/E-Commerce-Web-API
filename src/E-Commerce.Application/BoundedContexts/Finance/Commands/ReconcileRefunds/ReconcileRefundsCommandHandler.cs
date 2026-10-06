using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Jobs.ProcessRefund;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.Services;
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
    private readonly IJobScheduler _jobScheduler;
    private readonly IClock _clock;
    private readonly ILogger<ReconcileRefundsCommandHandler> _logger;

    public ReconcileRefundsCommandHandler(
        IRefundRepository refundRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler,
        IClock clock,
        ILogger<ReconcileRefundsCommandHandler> logger)
    {
        _refundRepository = refundRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ReconcileRefundsCommand command,
        CancellationToken ct)
    {
        // Phase 1 — recover stale Requested refunds whose ProcessRefundJob
        // was never enqueued (crash between SaveChangesAsync and Enqueue).
        var orphanCutoff = _clock.UtcNow.AddMinutes(-5);

        var orphanedRequested = await _refundRepository.GetRequestedOlderThanAsync(
            orphanCutoff,
            command.BatchSize,
            ct);

        foreach (var refund in orphanedRequested)
        {
            ct.ThrowIfCancellationRequested();

            _jobScheduler.Enqueue(new ProcessRefundJob(refund.Id));

            _logger.LogInformation(
                "Re-enqueued stale Requested refund {RefundId} (requested at {RequestedAtUtc})",
                refund.Id,
                refund.RequestedAtUtc);
        }

        // Phase 2 — resolve stuck Processing refunds via provider status query.
        var processingCutoff = _clock.UtcNow.AddMinutes(-15);

        var stuckRefunds = await _refundRepository.GetProcessingOlderThanAsync(
            processingCutoff,
            command.BatchSize,
            ct);

        foreach (var refund in stuckRefunds)
        {
            ct.ThrowIfCancellationRequested();
            await ReconcileRefundAsync(refund, ct);
        }

        return Result.Success();
    }

    private async Task ReconcileRefundAsync(
        RefundAggregate refund,
        CancellationToken ct)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(refund.PaymentId, ct);

            if (payment is null)
            {
                _logger.LogWarning(
                    "Refund {RefundId} references missing payment {PaymentId}. " +
                    "Manual reconciliation required.",
                    refund.Id,
                    refund.PaymentId);
                return;
            }

            if (string.IsNullOrWhiteSpace(payment.ProviderTransactionId))
            {
                // The parent payment was never captured, so Paymob has no
                // refund signal to expose. Requeueing would re-issue the
                // refund against Paymob (no idempotency key), risking a
                // double refund. Leave in Processing; operator resolves.
                _logger.LogWarning(
                    "Refund {RefundId} is Processing but parent payment {PaymentId} " +
                    "has no provider transaction id. Manual reconciliation required.",
                    refund.Id,
                    refund.PaymentId);
                return;
            }

            var providerReference = BuildProviderReference(payment);
            var status = await _paymentGateway.GetRefundStatusAsync(providerReference, ct);

            await ApplyRefundStatusAsync(refund, payment, status, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(
                ex,
                "Invalid provider reference while reconciling refund {RefundId}",
                refund.Id);
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

    private static PaymentProviderReference BuildProviderReference(
        PaymentAggregate payment)
    {
        return new PaymentProviderReference
        {
            Provider = payment.Provider,
            IntentionId = payment.ProviderIntentionId,
            // Parent payment transaction — Paymob exposes refund status here,
            // not on the refund's own transaction id.
            TransactionId = payment.ProviderTransactionId
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