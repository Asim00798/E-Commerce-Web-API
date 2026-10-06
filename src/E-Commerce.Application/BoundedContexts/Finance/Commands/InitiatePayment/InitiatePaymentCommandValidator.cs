using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandValidator
    : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.ReturnUrl).NotEmpty();
        RuleFor(x => x.CancelUrl).NotEmpty();
    }
}