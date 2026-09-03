using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products").HasCharSet("utf8mb4");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();

        builder.Property(product => product.Sku)
            .HasMaxLength(64)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique();

        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000);
        builder.Property(product => product.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(product => product.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(product => product.Version).IsConcurrencyToken().IsRequired();
        builder.Property(product => product.CreatedAtUtc)
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
            .HasColumnType("datetime(6)")
            .IsRequired();
        builder.Property(product => product.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(product => product.ModifiedAtUtc)
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
            .HasColumnType("datetime(6)")
            .IsRequired();
        builder.Property(product => product.ModifiedBy).HasMaxLength(200).IsRequired();
    }
}
