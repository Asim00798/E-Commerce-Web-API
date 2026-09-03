using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Behaviors;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.CustomerEngagement.Configurations;

public sealed class WishlistConfiguration : BaseEntityConfiguration<Wishlist>
{
    public override void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        base.Configure(builder);

        builder.ToTable("Wishlists", "engagement");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.CustomerId)
               .IsRequired();

        builder.Property(w => w.CreatedAtUtc)
               .IsRequired();

        // Unique constraint: one wishlist per customer
        builder.HasIndex(w => w.CustomerId)
               .IsUnique()
               .HasDatabaseName("UX_Wishlists_CustomerId");

        // Owned collection of WishlistItem
        builder.OwnsMany(w => w.Items, item =>
        {
            item.ToTable("WishlistItems", "engagement");
            item.WithOwner().HasForeignKey(wi => wi.WishlistId);
            item.HasKey(wi => wi.Id);

            item.Property(wi => wi.ProductId)
                .IsRequired();

            item.Property(wi => wi.AddedAtUtc)
                .IsRequired();

            // Unique constraint: no duplicate product in a wishlist
            item.HasIndex(wi => new { wi.WishlistId, wi.ProductId })
                .IsUnique()
                .HasDatabaseName("UX_WishlistItems_WishlistId_ProductId");
        });

        // Optimistic concurrency token
        builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .HasColumnName("RowVersion");
    }
}