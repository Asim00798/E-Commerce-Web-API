using ProductAggregate = E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors.Product;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Product.Configurations;

public sealed class ProductConfiguration : BaseEntityConfiguration<ProductAggregate>
{
    public override void Configure(EntityTypeBuilder<ProductAggregate> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BrandId)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.OwnsOne(x => x.Description, description =>
        {
            description.Property(d => d.Name).HasColumnName("Name").IsRequired().HasMaxLength(200);
            description.Property(d => d.ShortDescription).HasColumnName("ShortDescription").HasMaxLength(500);
            description.Property(d => d.LongDescription).HasColumnName("LongDescription").HasMaxLength(4000);

            description.OwnsOne(d => d.Dimensions, dim =>
            {
                dim.Property(dd => dd.Length).HasColumnName("Length").HasPrecision(18, 2);
                dim.Property(dd => dd.Width).HasColumnName("Width").HasPrecision(18, 2);
                dim.Property(dd => dd.Height).HasColumnName("Height").HasPrecision(18, 2);
            });

            description.OwnsOne(d => d.Weight, weight =>
            {
                weight.Property(w => w.Kilograms).HasColumnName("WeightKg").HasPrecision(18, 2);
            });

            description.Property(d => d.DateOfManufacture).HasColumnName("DateOfManufacture");
            description.Property(d => d.DateOfExpiry).HasColumnName("DateOfExpiry");
            description.Property(d => d.Material).HasColumnName("Material").HasMaxLength(100);
            description.Property(d => d.Color).HasColumnName("Color").HasMaxLength(50);
        });

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Variants)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Variants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("Tags");

        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.Status);
    }
}