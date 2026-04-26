#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Entities
{
    public class SellerDocument : BaseEntity
    {
        public string DocumentName { get; private set; }
        public string DocumentUrl { get; private set; }
        public string DocumentType { get; private set; }
        public DateTime UploadedAt { get; private set; }

        public SellerDocument(string documentName, string documentUrl, string documentType)
        {
            DocumentName = documentName;
            DocumentUrl = documentUrl;
            DocumentType = documentType;
            UploadedAt = DateTime.UtcNow;
        }
    }
}

#endif