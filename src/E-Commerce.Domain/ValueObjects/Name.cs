using E_Commerce.Domain.Exceptions;

public sealed record Name
{
    public string FirstName { get; }
    public string? SecondName { get; }
    public string? ThirdName { get; }        
    public string LastName { get; }

    public Name(string firstName,string? secondName,string? thirdName ,string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleViolationException("First name cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessRuleViolationException("Last name cannot be empty");

        FirstName = firstName;
        LastName = lastName;
        SecondName = secondName;
        ThirdName = thirdName;
    }
}
