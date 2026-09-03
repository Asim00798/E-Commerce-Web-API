using CategoryAggregate = E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors.Category;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Category.Configurations;

public sealed class CategoryConfiguration : BaseEntityConfiguration<CategoryAggregate>
{
    public override void Configure(EntityTypeBuilder<CategoryAggregate> builder)
    {
        base.Configure(builder);

        builder.ToTable("Categories", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.ParentCategoryId)
            .IsRequired(false);

        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}