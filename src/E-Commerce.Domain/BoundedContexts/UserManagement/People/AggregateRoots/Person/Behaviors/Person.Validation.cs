using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;

public partial class Person
{
    // ==========================================================
    // Construction / Behavior Validation
    // ==========================================================
    private static FullName RequireName(FullName? name)
    {
        return name ?? throw new BusinessRuleViolationException("Person name is required.");
    }

    private static PhoneNumber RequirePhoneNumber(PhoneNumber? phoneNumber)
    {
        return phoneNumber ?? throw new BusinessRuleViolationException("Phone number is required.");
    }

    private static Email RequireEmail(Email? email)
    {
        return email ?? throw new BusinessRuleViolationException("Email is required.");
    }

    private static DateOnly RequireValidDateOfBirth(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (dateOfBirth > today)
            throw new BusinessRuleViolationException("Date of birth cannot be in the future.");

        return dateOfBirth;
    }

    private static Gender RequireValidGender(Gender gender)
    {
        if (!Enum.IsDefined(typeof(Gender), gender))
            throw new BusinessRuleViolationException("Invalid gender.");

        return gender;
    }

    private static Address RequireAddress(Address? address)
    {
        return address ?? throw new BusinessRuleViolationException("Address is required.");
    }

    private static Guid RequireFileId(Guid fileId)
    {
        if (fileId == Guid.Empty)
            throw new BusinessRuleViolationException("Invalid file identifier.");

        return fileId;
    }

    // ==========================================================
    // On Save Validation
    // ==========================================================

    public override void Validate()
    {
        ValidateRequiredState();
        ValidateBusinessRules();
    }

    private void ValidateRequiredState()
    {
        if (Name is null)
            throw new BusinessRuleViolationException("Person name is required.");

        if (PhoneNumber is null)
            throw new BusinessRuleViolationException("Phone number is required.");

        if (Email is null)
            throw new BusinessRuleViolationException("Email is required.");
    }

    private void ValidateBusinessRules()
    {
        if (DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleViolationException("Date of birth cannot be in the future.");

        if (!Enum.IsDefined(typeof(Gender), Gender))
            throw new BusinessRuleViolationException("Invalid gender.");

        if (PersonalImage is not null &&    
            PersonalImage.FileId == Guid.Empty)
        {
            throw new BusinessRuleViolationException("Invalid personal image.");
        }
    }

}