using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors
{
    public sealed partial class Person : BaseEntity, IAggregateRoot
    {
        public Guid? IdentityUserId { get; private set; }
        public FullName Name { get; private set; } = null!;
        public DateOnly DateOfBirth { get; private set; }
        public Gender Gender { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public Address? HomeAddress { get; private set; }
        public PersonalImage? PersonalImage { get; private set; } 

        private Person() { }

        //DDD Constructor
        public Person(
            PhoneNumber phoneNumber,
            Email email,
            Address? address,
            PersonalImage? personalImage,
            FullName name,
            DateOnly dateOfBirth,
            Gender gender)
        {
            Name = RequireName(name);
            PhoneNumber = RequirePhoneNumber(phoneNumber);
            Email = RequireEmail(email);
            DateOfBirth = RequireValidDateOfBirth(dateOfBirth);
            Gender = RequireValidGender(gender);

            HomeAddress = address;
            PersonalImage = personalImage;
        }

        public void LinkIdentityUser(Guid identityUserId)
        {
            if (identityUserId == Guid.Empty)
                throw new BusinessRuleViolationException(
                    "Invalid identity user id.");

            if (IdentityUserId.HasValue)
                throw new BusinessRuleViolationException(
                    "Person is already linked to an identity account.");

            IdentityUserId = identityUserId;
        }
    }
}