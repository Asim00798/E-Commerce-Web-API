using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.AssignDriver;

public sealed class AssignDriverCommandValidator : AbstractValidator<AssignDriverCommand>
{
    public AssignDriverCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty();
    }
}