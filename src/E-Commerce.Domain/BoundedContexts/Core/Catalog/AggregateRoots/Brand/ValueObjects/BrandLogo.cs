namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects
{
    public sealed record BrandLogo
    {
        // File ID is referenced in the storage system (e.g., blob storage)
        // where the logo is stored.
        // It can be used to retrieve or manage the file.
        public Guid FileId { get; init; } 
        public bool IsPrimary { get; init; }

        public BrandLogo(Guid fileId, bool isPrimary = false)
        {
            IsPrimary = isPrimary;
            if (fileId == Guid.Empty)
                throw new Exceptions.BrandException("Logo File ID cannot be empty.", nameof(fileId));
        }

        internal BrandLogo SetPrimary() => this with { IsPrimary = true };

        internal BrandLogo UnsetPrimary() => this with { IsPrimary = false };

        public BrandLogo ChangeFileId(Guid newFileId)
        {
            if (newFileId == Guid.Empty)
                throw new Exceptions.BrandException("Logo File ID cannot be empty.", nameof(newFileId));

            return this with { FileId = newFileId };
        }
    }
}
