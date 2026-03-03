namespace E_Commerce.Domain.SharedKernel.Interfaces
{
    public interface IBusinessRule
    {
        /// <summary>
        /// Evaluates whether the rule is satisfied.
        /// </summary>
        /// <returns>True if the rule is satisfied; otherwise, false.</returns>
        bool IsSatisfied();

        /// <summary>
        /// Gets the message to be used when the rule is broken.
        /// </summary>
        string Message { get; }
    }
}