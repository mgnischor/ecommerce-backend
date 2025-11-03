using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the ProductVariantEntity.
/// Maps the ProductVariantEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class ProductVariantEntityConfiguration : IEntityTypeConfiguration<ProductVariantEntity>
{
    public void Configure(EntityTypeBuilder<ProductVariantEntity> builder)
    {
        builder.ToTable("product_variants", schema: "public");

        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(pv => pv.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder
            .Property(pv => pv.UpdatedBy)
            .HasColumnName("updated_by");

        builder
            .Property(pv => pv.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder
            .Property(pv => pv.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(pv => pv.Sku)
            .HasColumnName("sku")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(pv => pv.Barcode)
            .HasColumnName("barcode")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder
            .Property(pv => pv.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder
            .Property(pv => pv.DiscountPrice)
            .HasColumnName("discount_price")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(pv => pv.StockQuantity)
            .HasColumnName("stock_quantity")
            .IsRequired()
            .HasDefaultValue(0);

        builder
            .Property(pv => pv.Weight)
            .HasColumnName("weight")
            .HasColumnType("decimal(10,2)");

        builder
            .Property(pv => pv.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder
            .Property(pv => pv.Images)
            .HasColumnName("images")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(pv => pv.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(pv => pv.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        builder
            .Property(pv => pv.IsAvailable)
            .HasColumnName("is_available")
            .HasDefaultValue(true);

        builder
            .Property(pv => pv.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder
            .Property(pv => pv.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder
            .Property(pv => pv.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(pv => pv.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(pv => pv.ProductId).HasDatabaseName("ix_product_variants_product_id");
        builder.HasIndex(pv => pv.Sku).IsUnique().HasDatabaseName("ix_product_variants_sku");
        builder.HasIndex(pv => pv.IsDefault).HasDatabaseName("ix_product_variants_is_default");
        builder.HasIndex(pv => pv.IsAvailable).HasDatabaseName("ix_product_variants_is_available");
        builder.HasIndex(pv => pv.DisplayOrder).HasDatabaseName("ix_product_variants_display_order");
    }
}
