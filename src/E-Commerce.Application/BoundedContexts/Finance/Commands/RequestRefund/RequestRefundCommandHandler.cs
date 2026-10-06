using E_Commerce.Application.BoundedContexts.Finance.Jobs.ProcessRefund;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.RequestRefund;

public sealed class RequestRefundCommandHandler
    : IRequestHandler<RequestRefundCommand, Result<Guid>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRefundRepository _refundRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobScheduler _jobScheduler;
    private readonly ICurrentUser _currentUser;

    public RequestRefundCommandHandler(
        IPaymentRepository paymentRepository,
        IRefundRepository refundRepository,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler,
        ICurrentUser currentUser)
    {
        _paymentRepository = paymentRepository;
        _refundRepository = refundRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
    RequestRefundCommand command,
    CancellationToken ct)
    {
        try
        {
            var currentUserId = _currentUser.UserId;
            if (currentUserId is null)
                return Result<Guid>.Failure("Authenticated user identifier is missing.");

            var payment = await _paymentRepository.GetByIdAsync(command.PaymentId, ct);
            if (payment is null)
                return Result<Guid>.Failure("Payment not found.");

            var isPrivileged = _currentUser.IsInRole(SystemRoles.Administrator) ||
                               _currentUser.IsInRole(SystemRoles.Support);

            if (!isPrivileged && payment.CustomerId != currentUserId.Value)
                return Result<Guid>.Failure("Payment not found.");

            var refundAmount = new Money(command.Amount, command.Currency);

            if (!payment.CanApplyRefund(refundAmount))
                return Result<Guid>.Failure(
                    "Refund is not eligible for the current payment state.");

            var outstanding = await _refundRepository
                .GetOutstandingByPaymentIdAsync(payment.Id, ct);

            var outstandingTotal = outstanding.Sum(r => r.Amount.Amount);
            var remaining = payment.Amount.Amount - payment.RefundedAmount.Amount;

            if (outstandingTotal + refundAmount.Amount > remaining)
                return Result<Guid>.Failure(
                    "Refund amount exceeds remaining refundable balance.");

            var refund = Refund.Create(
                payment.Id,
                payment.OrderId,
                refundAmount,
                command.Reason);

            await _refundRepository.AddAsync(refund, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _jobScheduler.Enqueue(new ProcessRefundJob(refund.Id));

            return Result<Guid>.Success(refund.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}