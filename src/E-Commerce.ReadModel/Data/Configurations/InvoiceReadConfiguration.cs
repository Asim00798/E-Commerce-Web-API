using E_Commerce.ReadModel.Features.Invoices.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.ReadModel.Data.Configurations;

public class InvoiceReadConfiguration : IEntityTypeConfiguration<InvoiceProjection>
{
    public void Configure(EntityTypeBuilder<InvoiceProjection> builder)
    {
        builder.ToTable("Invoices", "Read");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
        
        // Optimize for read performance
        builder.HasIndex(x => x.InvoiceNumber);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.IssuedDate);
    }
}
