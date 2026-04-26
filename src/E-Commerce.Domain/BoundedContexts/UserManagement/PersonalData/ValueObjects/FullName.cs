#if false
using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.ValueObjects
{
    public sealed record FullName
    {
        public string FirstName { get; init; }
        public string? SecondName { get; init; }
        public string? ThirdName { get; init; }
        public string LastName { get; init; }

        public FullName(string firstName, string? secondName, string? thirdName, string lastName)
        {
            FirstName = ValidateName(firstName, "First name");
            LastName = ValidateName(lastName, "Last name");
            SecondName = secondName;
            ThirdName = thirdName;
        }

        // ======================
        // With methods for immutability + validation
        // ======================
        public FullName WithFirstName(string firstName) =>
            this with { FirstName = ValidateName(firstName, "First name") };

        public FullName WithSecondName(string? secondName) =>
            this with { SecondName = secondName };

        public FullName WithThirdName(string? thirdName) =>
            this with { ThirdName = thirdName };

        public FullName WithLastName(string lastName) =>
            this with { LastName = ValidateName(lastName, "Last name") };

        // ======================
        // Validation helper
        // ======================
        private static string ValidateName(string? name, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException($"{fieldName} cannot be empty");
            return name.Trim();
        }

        public override string ToString()
        {
            var names = new[] { FirstName, SecondName, ThirdName, LastName };
            return string.Join(" ", names.Where(n => !string.IsNullOrWhiteSpace(n)));
        }
    }
}

#endif