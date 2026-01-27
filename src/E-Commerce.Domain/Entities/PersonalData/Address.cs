using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Entities.PersonalData
{
    public class Address : BaseEntity
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        // Optional because not every country has "states"
        public string? State { get; set; }
        public string? LocationMapUrl { get; set; } = null;
        // Strongly typed instead of string
        public AddressType Type { get; set; } = AddressType.Generic;

        //Navigation
        public Person? Person { get; set; }

        public override void Validate()
        {
             base.Validate();
        }
    }

}
