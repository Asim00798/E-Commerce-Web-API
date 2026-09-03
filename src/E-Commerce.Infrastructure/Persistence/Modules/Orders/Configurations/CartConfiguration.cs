using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Configurations;

public sealed class CartConfiguration : BaseEntityConfiguration<Cart>
{
    public override void Configure(EntityTypeBuilder<Cart> builder)
    {
        base.Configure(builder);

        builder.ToTable("Carts", "ordering");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerId)
               .IsRequired();

        // One-to-many relationship with CartItem
        builder.HasMany(c => c.Items)
               .WithOne()
               .HasForeignKey(ci => ci.CartId)
               .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency token (shadow property)
        builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .HasColumnName("RowVersion");
    }
}