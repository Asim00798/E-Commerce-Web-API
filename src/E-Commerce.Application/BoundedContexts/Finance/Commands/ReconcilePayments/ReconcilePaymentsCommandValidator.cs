using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcilePayments;

public sealed class ReconcilePaymentsCommandValidator : AbstractValidator<ReconcilePaymentsCommand>
{
    public ReconcilePaymentsCommandValidator()
    {
        RuleFor(x => x.BatchSize)
            .InclusiveBetween(1, 500);
    }
}