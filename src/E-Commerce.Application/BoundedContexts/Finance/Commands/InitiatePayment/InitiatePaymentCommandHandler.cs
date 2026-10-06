using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Orders;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.BoundedContexts.Core.Finance.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandHandler
    : IRequestHandler<InitiatePaymentCommand, Result<PaymentInitiationResult>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderPricingReader _orderPricingReader;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ICurrentUser _currentUser;

    public InitiatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderPricingReader orderPricingReader,
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway,
        ICurrentUser currentUser)
    {
        _paymentRepository = paymentRepository;
        _orderPricingReader = orderPricingReader;
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
        _currentUser = currentUser;
    }

    public async Task<Result<PaymentInitiationResult>> Handle(
        InitiatePaymentCommand command,
        CancellationToken ct)
    {
        try
        {
            var snapshot = await _orderPricingReader.GetByOrderIdAsync(command.OrderId, ct);
            if (snapshot is null)
                return Result<PaymentInitiationResult>.Failure("Order not found.");

            var currentUserId = _currentUser.UserId;
            if (currentUserId is null)
                return Result<PaymentInitiationResult>.Failure(
                    "Authenticated user identifier is missing.");

            var isPrivileged = _currentUser.IsInRole(SystemRoles.Administrator) ||
                               _currentUser.IsInRole(SystemRoles.Support);

            if (!isPrivileged && snapshot.CustomerId != currentUserId.Value)
                return Result<PaymentInitiationResult>.Failure("Order not found.");

            var existing = await _paymentRepository.GetByOrderIdAsync(command.OrderId, ct);
            if (existing is not null)
                return Result<PaymentInitiationResult>.Failure(
                    "Payment already initiated for this order.");

            var money = new Money(snapshot.Amount, snapshot.Currency);
            var method = new PaymentMethod(command.Method);

            var payment = Payment.Create(
                snapshot.OrderId,
                snapshot.CustomerId,
                money,
                method);

            await _paymentRepository.AddAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var initiationRequest = BuildInitiationRequest(command, payment);

            var initiationResult = await TryInitiatePaymentWithProviderAsync(
                payment, initiationRequest, ct);

            if (initiationResult is null)
                return Result<PaymentInitiationResult>.Failure("Payment initiation failed.");

            payment.AssignProviderIntention(
                initiationResult.Provider,
                initiationResult.IntentionId);

            await _paymentRepository.UpdateAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<PaymentInitiationResult>.Success(initiationResult);
        }
        catch (DomainException ex)
        {
            return Result<PaymentInitiationResult>.Failure(ex.Message);
        }
    }

    private static PaymentInitiationRequest BuildInitiationRequest(
        InitiatePaymentCommand command,
        Payment payment)
    {
        return new PaymentInitiationRequest
        {
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            Amount = payment.Amount,
            Method = command.Method,
            ReturnUrl = command.ReturnUrl,
            CancelUrl = command.CancelUrl,
            IdempotencyKey = command.IdempotencyKey ?? payment.Id.ToString()
        };
    }

    private async Task<PaymentInitiationResult?> TryInitiatePaymentWithProviderAsync(
        Payment payment,
        PaymentInitiationRequest initiationRequest,
        CancellationToken ct)
    {
        PaymentInitiationResult? initiationResult;

        try
        {
            initiationResult = await _paymentGateway.InitiatePaymentAsync(
                initiationRequest, ct);
        }
        catch
        {
            await MarkPaymentFailedAsync(payment, ct);
            return null;
        }

        if (initiationResult is null ||
            string.IsNullOrWhiteSpace(initiationResult.IntentionId))
        {
            await MarkPaymentFailedAsync(payment, ct);
            return null;
        }

        return initiationResult;
    }

    private async Task MarkPaymentFailedAsync(Payment payment, CancellationToken ct)
    {
        payment.Fail();
        await _paymentRepository.UpdateAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}