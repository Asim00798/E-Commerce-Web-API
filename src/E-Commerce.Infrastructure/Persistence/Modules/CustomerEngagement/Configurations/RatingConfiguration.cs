using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.CustomerEngagement.Configurations;

public sealed class RatingConfiguration : BaseEntityConfiguration<Rating>
{
    public override void Configure(EntityTypeBuilder<Rating> builder)
    {
        base.Configure(builder);

        builder.ToTable("Ratings", "engagement");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CustomerId)
               .IsRequired();

        builder.Property(r => r.ProductId)
               .IsRequired();

        // StarRating value object
        builder.OwnsOne(r => r.StarRating, star =>
        {
            star.Property(s => s.Value)
                .HasColumnName("StarRatingValue")
                .IsRequired();
        });

        builder.Property(r => r.CreatedAtUtc)
               .IsRequired();

        builder.Property(r => r.UpdatedAtUtc)
               .IsRequired(false);

        // Unique constraint: one rating per customer per product
        builder.HasIndex(r => new { r.CustomerId, r.ProductId })
               .IsUnique()
               .HasDatabaseName("UX_Ratings_CustomerId_ProductId");

        // Optimistic concurrency token
        builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .HasColumnName("RowVersion");
    }
}