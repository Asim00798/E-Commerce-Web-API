using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.PersonalData
{
    public class Country : BaseEntity
    {

        // Country Name (required)
        public string Name { get; set; } = string.Empty;

        // ISO code (required, 2 or 3 letters)
        public string ISOCode { get; set; } = string.Empty;

        // -----------------
        // Validation / Business Rules
        // -----------------
        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Country Name is required.");

            if (string.IsNullOrWhiteSpace(ISOCode))
                throw new InvalidOperationException("Country ISOCode is required.");
        }
    }

}
