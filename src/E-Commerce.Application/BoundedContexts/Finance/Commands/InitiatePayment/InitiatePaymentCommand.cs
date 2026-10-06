using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.InitiatePayment;

public sealed record InitiatePaymentCommand(
    Guid OrderId,
    PaymentMethodType Method,
    string ReturnUrl,
    string CancelUrl,
    string? IdempotencyKey) : IRequest<Result<PaymentInitiationResult>>;