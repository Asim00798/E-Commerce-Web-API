using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;
using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.RecordDeliveryAttempt;

public sealed class RecordDeliveryAttemptCommandValidator : AbstractValidator<RecordDeliveryAttemptCommand>
{
    public RecordDeliveryAttemptCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Result).IsInEnum();
        RuleFor(x => x.FailureReason)
            .NotEmpty()
            .When(x => x.Result != DeliveryAttemptResult.Delivered)
            .MaximumLength(250);
        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}