using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.SharedKernel.ValueObjects;

public sealed record PhoneNumber
{
    public string Value { get; init; }
    public bool IsPrimary { get; init; } = false;

    public PhoneNumber(string value, bool isPrimary = false)
    {
        Value = ValidateValue(value);
        IsPrimary = isPrimary;
    }

    // ======================
    // "With" methods for immutability + validation
    // ======================

    public PhoneNumber WithValue(string value) =>
        this with { Value = ValidateValue(value) };

    public PhoneNumber WithIsPrimary(bool isPrimary) =>
        this with { IsPrimary = isPrimary };

    // ======================
    // Validation helpers
    // ======================

    private static string ValidateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleViolationException("Contact value cannot be empty.");
        if (value.Length > 10+3) // 10 digits + 3 for formatting characters (e.g., dashes, parentheses)
            throw new BusinessRuleViolationException("Contact value cannot exceed 10 characters.");
        return value.Trim();
    }

    public override string ToString() =>
        $"{Value}" + (IsPrimary ? " (Primary)" : "");
}
