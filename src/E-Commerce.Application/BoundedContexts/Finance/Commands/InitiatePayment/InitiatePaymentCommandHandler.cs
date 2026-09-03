using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Application.Shared.Models;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;

    public InitiatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<Result<PaymentInitiationResult>> Handle(
        InitiatePaymentCommand command,
        CancellationToken ct)
    {
        try
        {
            var payment = await CreateAndPersistPaymentAsync(command, ct);

            var initiationRequest = BuildInitiationRequest(command, payment);

            var initiationResult = await TryInitiatePaymentWithProviderAsync(
                payment,
                initiationRequest,
                ct);

            if (initiationResult is null)
            {
                return Result<PaymentInitiationResult>.Failure("Payment initiation failed.");
            }

            payment.AssignProviderIntention(
                initiationResult.Provider,
                initiationResult.IntentionId);

            await UpdatePaymentAsync(payment, ct);

            return Result<PaymentInitiationResult>.Success(initiationResult);
        }
        catch (DomainException ex)
        {
            return Result<PaymentInitiationResult>.Failure(ex.Message);
        }
    }

    private async Task<Payment> CreateAndPersistPaymentAsync(
        InitiatePaymentCommand command,
        CancellationToken ct)
    {
        var money = CreateMoney(command);
        var method = new PaymentMethod(command.Method);

        var payment = Payment.Create(
            command.OrderId,
            command.CustomerId,
            money,
            method);

        await _paymentRepository.AddAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return payment;
    }

    private static Money CreateMoney(InitiatePaymentCommand command)
    {
        return new Money(command.Amount, command.Currency);
    }

    private static PaymentInitiationRequest BuildInitiationRequest(
        InitiatePaymentCommand command,
        Payment payment)
    {
        return new PaymentInitiationRequest
        {
            OrderId = command.OrderId,
            CustomerId = command.CustomerId,
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
                initiationRequest,
                ct);
        }
        catch
        {
            await MarkPaymentFailedAsync(payment, ct);
            return null;
        }

        if (initiationResult is null || string.IsNullOrWhiteSpace(initiationResult.IntentionId))
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

    private async Task UpdatePaymentAsync(Payment payment, CancellationToken ct)
    {
        await _paymentRepository.UpdateAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}