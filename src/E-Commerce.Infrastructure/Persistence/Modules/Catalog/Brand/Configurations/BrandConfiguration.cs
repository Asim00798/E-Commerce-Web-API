using BrandAggregate = E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors.Brand;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Brand.Configurations;

public sealed class BrandConfiguration : BaseEntityConfiguration<BrandAggregate>
{
    public override void Configure(EntityTypeBuilder<BrandAggregate> builder)
    {
        base.Configure(builder);

        builder.ToTable("Brands", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.DescriptionText)
            .HasMaxLength(500);

        builder.OwnsOne(x => x.Logo, logo =>
        {
            logo.Property(x => x.FileId).HasColumnName("LogoFileId").IsRequired();
        });

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}