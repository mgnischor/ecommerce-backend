using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the ProductEntity.
/// Maps the ProductEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("products", schema: "public");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder
            .Property(p => p.UpdatedBy)
            .HasColumnName("updated_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder
            .Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsUnicode(true);

        builder
            .Property(p => p.Sku)
            .HasColumnName("sku")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(p => p.Brand).HasColumnName("brand").HasMaxLength(100).IsUnicode(true);

        builder
            .Property(p => p.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder
            .Property(p => p.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder
            .Property(p => p.DiscountPrice)
            .HasColumnName("discount_price")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(p => p.Weight)
            .HasColumnName("weight")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder
            .Property(p => p.StockQuantity)
            .HasColumnName("stock_quantity")
            .IsRequired()
            .HasDefaultValue(0);

        builder
            .Property(p => p.MinStockLevel)
            .HasColumnName("min_stock_level")
            .HasDefaultValue(0);

        builder
            .Property(p => p.MaxOrderQuantity)
            .HasColumnName("max_order_quantity")
            .HasDefaultValue(100);

        builder
            .Property(p => p.Category)
            .HasColumnName("category")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValueSql("0");

        builder
            .Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValueSql("1");

        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(p => p.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder.Property(p => p.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);

        builder.Property(p => p.IsOnSale).HasColumnName("is_on_sale").HasDefaultValue(false);

        builder
            .Property(p => p.Tags)
            .HasColumnName("tags")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(p => p.Images)
            .HasColumnName("images")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("ix_products_sku");
        builder.HasIndex(p => p.Name).HasDatabaseName("ix_products_name");
        builder.HasIndex(p => p.Category).HasDatabaseName("ix_products_category");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_products_status");
        builder.HasIndex(p => p.CreatedAt).HasDatabaseName("ix_products_created_at");
        builder.HasIndex(p => p.IsFeatured).HasDatabaseName("ix_products_is_featured");
        builder.HasIndex(p => p.IsOnSale).HasDatabaseName("ix_products_is_on_sale");
    }
}
