using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.ValueObjects;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;

public partial class Person
{
    public void UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        phoneNumber = RequirePhoneNumber(phoneNumber);

        if (PhoneNumber == phoneNumber)
            return;

        PhoneNumber = phoneNumber;
    }

    public void UpdateEmail(Email email)
    {
        email = RequireEmail(email);

        if (Email == email)
            return;

        Email = email;
    }

    public void SetPersonalImage(Guid fileId)
    {
        fileId = RequireFileId(fileId);

        if (PersonalImage?.FileId == fileId)
            return;

        PersonalImage = new PersonalImage(fileId);
    }

    public void UpdateAddress(Address homeAddress)
    {
        homeAddress = RequireAddress(homeAddress);
        if (this.HomeAddress == homeAddress)
            return;
        HomeAddress = homeAddress;
    }
}
