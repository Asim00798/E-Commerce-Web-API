using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcileRefunds;

public sealed class ReconcileRefundsCommandValidator : AbstractValidator<ReconcileRefundsCommand>
{
    public ReconcileRefundsCommandValidator()
    {
        RuleFor(x => x.BatchSize)
            .InclusiveBetween(1, 500);
    }
}