using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;

public sealed record BrandLogo
{
    public Guid FileId { get; init; }

    public BrandLogo(Guid fileId)
    {
        FileId = ValidateFileId(fileId);
    }

    public BrandLogo WithFileId(Guid fileId) =>
        this with { FileId = ValidateFileId(fileId) };

    private static Guid ValidateFileId(Guid fileId)
    {
        if (fileId == Guid.Empty)
            throw new BrandException("Logo File ID cannot be empty.", nameof(fileId));

        return fileId;
    }
}