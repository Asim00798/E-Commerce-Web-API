using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.ReassignDriver;

public sealed class ReassignDriverCommandValidator : AbstractValidator<ReassignDriverCommand>
{
    public ReassignDriverCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.NewDriverId).NotEmpty();
    }
}