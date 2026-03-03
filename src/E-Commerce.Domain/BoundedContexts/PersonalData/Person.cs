using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.PersonalData
{
    public class Person : BaseEntity
    {
        public PersonIdentity Identity { get; private set; }
        public ICollection<Contact> Contacts { get; private set; }
        public Address? HomeAddress { get; private set; }
        public string? PersonalImageUrl { get; private set; }

        //DDD Constructor
        public Person(PersonIdentity identity, ICollection<Contact> contacts, Address? address)
        {
            Identity = identity;
            Contacts = contacts;
            HomeAddress = address;
        }

        public void ChangeProfileImage(string? newUrl)
        {
            if (PersonalImageUrl == newUrl) return;

            if (newUrl != null && !Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
                throw new BusinessRuleViolationException("Invalid image URL.");

            PersonalImageUrl = newUrl;
        }

    }

}
