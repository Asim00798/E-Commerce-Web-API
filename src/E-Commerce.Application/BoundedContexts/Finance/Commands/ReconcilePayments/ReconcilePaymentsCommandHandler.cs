using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcilePayments;

public sealed class ReconcilePaymentsCommandHandler
    : IRequestHandler<ReconcilePaymentsCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<ReconcilePaymentsCommandHandler> _logger;

    public ReconcilePaymentsCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<ReconcilePaymentsCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ReconcilePaymentsCommand command,
        CancellationToken ct)
    {
        var cutoff = CalculateReconciliationCutoff();

        var stalePayments = await _paymentRepository
            .GetAwaitingPaymentWithTransactionOlderThanAsync(
                cutoff,
                command.BatchSize,
                ct);

        foreach (var payment in stalePayments)
        {
            ct.ThrowIfCancellationRequested();
            await ReconcilePaymentAsync(payment, ct);
        }

        return Result.Success();
    }

    private DateTime CalculateReconciliationCutoff()
    {
        return _clock.UtcNow.AddMinutes(-30);
    }

    private async Task ReconcilePaymentAsync(
        PaymentAggregate payment,
        CancellationToken ct)
    {
        try
        {
            var statusResult = await GetPaymentStatusAsync(payment, ct);

            if (statusResult is null)
            {
                _logger.LogWarning(
                    "Payment provider returned null status for payment {PaymentId}",
                    payment.Id);

                return;
            }

            await ApplyPaymentStatusAsync(payment, statusResult, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Provider transport failure while reconciling payment {PaymentId}",
                payment.Id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Invalid provider operation while reconciling payment {PaymentId}",
                payment.Id);
        }
    }

    private async Task<PaymentStatusResult?> GetPaymentStatusAsync(
        PaymentAggregate payment,
        CancellationToken ct)
    {
        var reference = BuildProviderReference(payment);

        return await _paymentGateway.GetPaymentStatusAsync(reference, ct);
    }

    private static PaymentProviderReference BuildProviderReference(
        PaymentAggregate payment)
    {
        return new PaymentProviderReference
        {
            Provider = payment.Provider,
            IntentionId = payment.ProviderIntentionId,
            TransactionId = payment.ProviderTransactionId
        };
    }

    private async Task ApplyPaymentStatusAsync(
        PaymentAggregate payment,
        PaymentStatusResult statusResult,
        CancellationToken ct)
    {
        if (statusResult.Status == PaymentStatus.Captured)
        {
            var transactionId = statusResult.ProviderTransactionId ??
                                payment.ProviderTransactionId;

            payment.Capture(transactionId!);

            await _paymentRepository.UpdateAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return;
        }

        if (statusResult.Status == PaymentStatus.Failed)
        {
            payment.Fail();

            await _paymentRepository.UpdateAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}