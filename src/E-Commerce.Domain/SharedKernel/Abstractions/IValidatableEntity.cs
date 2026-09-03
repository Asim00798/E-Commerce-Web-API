
namespace E_Commerce.Domain.SharedKernel.Abstractions
{
    // <summary>
    /// Represents an entity that can be validated.
    /// Validatable entities implement this interface to provide a mechanism
    /// for validating their state and ensuring that they adhere to business rules
    /// and constraints.
    /// Validation happens on save command, and if the entity is invalid, a BusinessRuleViolationException is thrown.
    /// </summary>      
    public interface IValidatableEntity
    {
        void Validate();
    }
}
