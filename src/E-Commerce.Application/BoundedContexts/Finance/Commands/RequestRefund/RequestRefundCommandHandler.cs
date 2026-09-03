using E_Commerce.Application.BoundedContexts.Finance.Jobs.ProcessRefund;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;
using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.RequestRefund;

public sealed class RequestRefundCommandHandler
    : IRequestHandler<RequestRefundCommand, Result<Guid>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRefundRepository _refundRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobScheduler _jobScheduler;

    public RequestRefundCommandHandler(
        IPaymentRepository paymentRepository,
        IRefundRepository refundRepository,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler)
    {
        _paymentRepository = paymentRepository;
        _refundRepository = refundRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result<Guid>> Handle(
        RequestRefundCommand command,
        CancellationToken ct)
    {
        try
        {
            var payment = await GetPaymentAsync(command.PaymentId, ct);
            if (payment is null)
            {
                return Result<Guid>.Failure("Payment not found.");
            }

            var refundAmount = CreateRefundAmount(command);

            if (!IsRefundEligible(payment, refundAmount))
            {
                return Result<Guid>.Failure("Refund is not eligible for the current payment state.");
            }

            var refund = CreateRefund(payment.Id, refundAmount, command.Reason);

            await SaveRefundAsync(refund, ct);
            EnqueueRefundProcessing(refund.Id);

            return Result<Guid>.Success(refund.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }

    private async Task<PaymentAggregate?> GetPaymentAsync(Guid paymentId, CancellationToken ct)
    {
        return await _paymentRepository.GetByIdAsync(paymentId, ct);
    }

    private static Money CreateRefundAmount(RequestRefundCommand command)
    {
        return new Money(command.Amount, command.Currency);
    }

    private static bool IsRefundEligible(PaymentAggregate payment, Money refundAmount)
    {
        return payment.CanApplyRefund(refundAmount);
    }

    private static Refund CreateRefund(Guid paymentId, Money refundAmount, string reason)
    {
        return Refund.Create(paymentId, refundAmount, reason);
    }

    private async Task SaveRefundAsync(Refund refund, CancellationToken ct)
    {
        await _refundRepository.AddAsync(refund, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private void EnqueueRefundProcessing(Guid refundId)
    {
        _jobScheduler.Enqueue(new ProcessRefundJob(refundId));
    }
}