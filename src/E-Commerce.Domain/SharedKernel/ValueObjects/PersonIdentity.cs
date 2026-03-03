using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.SharedKernel.Enums;

namespace E_Commerce.Domain.SharedKernel.ValueObjects
{
    public sealed record PersonIdentity
    {
        public FullName Name { get; init; }
        public DateOnly DateOfBirth { get; init; }
        public Gender Gender { get; init; }

        public PersonIdentity(FullName name, DateOnly dateOfBirth, Gender gender)
        {
            Name = name ?? throw new BusinessRuleViolationException("Name is required.");
            DateOfBirth = ValidateDateOfBirth(dateOfBirth);
            Gender = gender;
        }

        // ======================
        // Immutable "With" methods
        // ======================
        public PersonIdentity WithName(FullName newName)
        {
            if (newName == null) throw new BusinessRuleViolationException("Name is required.");
            if (newName == Name) return this;
            return this with { Name = newName };
        }

        public PersonIdentity WithGender(Gender newGender)
        {
            if (newGender == Gender) return this;
            return this with { Gender = newGender };
        }

        public PersonIdentity WithDateOfBirth(DateOnly newDateOfBirth)
        {
            newDateOfBirth = ValidateDateOfBirth(newDateOfBirth);
            if (newDateOfBirth == DateOfBirth) return this;
            return this with { DateOfBirth = newDateOfBirth };
        }

        // ======================
        // Validation helpers
        // ======================
        private static DateOnly ValidateDateOfBirth(DateOnly dob)
        {
            if (dob > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BusinessRuleViolationException("Date of birth cannot be in the future.");
            return dob;
        }

        public override string ToString() =>
            $"{Name.FirstName} {Name.LastName}, {Gender}, {DateOfBirth:yyyy-MM-dd}";
    }
}
