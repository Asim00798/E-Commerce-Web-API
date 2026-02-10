using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Exceptions;

public sealed record Contact
{
    public ContactType Type { get; }
    public string Value { get; }
    public bool IsPrimary { get; set; } = false;

    public Contact(ContactType type, string value)
    {
        if (!Enum.IsDefined(typeof(ContactType), type))
            throw new BusinessRuleViolationException("Invalid contact type");
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleViolationException("Contact value cannot be empty");
        if (value.Length > 200)
            throw new InvalidOperationException("Contact value cannot exceed 200 characters.");

        // Optional stricter checks
        if (Type == ContactType.Email && !value.Contains("@"))
            throw new InvalidOperationException("Email contact must contain '@' symbol.");
        Type = type;
        Value = value;
    }
}
