using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the ProductAttributeEntity.
/// Maps the ProductAttributeEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class ProductAttributeEntityConfiguration
    : IEntityTypeConfiguration<ProductAttributeEntity>
{
    public void Configure(EntityTypeBuilder<ProductAttributeEntity> builder)
    {
        builder.ToTable("ProductAttributes", schema: "public");

        builder.HasKey(pa => pa.Id);
        builder.Property(pa => pa.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(pa => pa.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(pa => pa.UpdatedBy).HasColumnName("updated_by");

        builder
            .Property(pa => pa.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(pa => pa.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(pa => pa.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsUnicode(true);

        builder
            .Property(pa => pa.DataType)
            .HasColumnName("data_type")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasDefaultValue("Text");

        builder
            .Property(pa => pa.PossibleValues)
            .HasColumnName("possible_values")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(pa => pa.DefaultValue)
            .HasColumnName("default_value")
            .HasMaxLength(500)
            .IsUnicode(true);

        builder.Property(pa => pa.IsRequired).HasColumnName("is_required").HasDefaultValue(false);

        builder
            .Property(pa => pa.IsVariantAttribute)
            .HasColumnName("is_variant_attribute")
            .HasDefaultValue(false);

        builder
            .Property(pa => pa.IsFilterable)
            .HasColumnName("is_filterable")
            .HasDefaultValue(true);

        builder
            .Property(pa => pa.IsSearchable)
            .HasColumnName("is_searchable")
            .HasDefaultValue(true);

        builder
            .Property(pa => pa.IsVisibleOnProductPage)
            .HasColumnName("is_visible_on_product_page")
            .HasDefaultValue(true);

        builder.Property(pa => pa.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);

        builder.Property(pa => pa.Unit).HasColumnName("unit").HasMaxLength(20).IsUnicode(false);

        builder
            .Property(pa => pa.ValidationPattern)
            .HasColumnName("validation_pattern")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder
            .Property(pa => pa.ApplicableCategoryIds)
            .HasColumnName("applicable_category_ids")
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'");

        builder.Property(pa => pa.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(pa => pa.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(pa => pa.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(pa => pa.Code).IsUnique().HasDatabaseName("ix_product_attributes_code");
        builder.HasIndex(pa => pa.Name).HasDatabaseName("ix_product_attributes_name");
        builder
            .HasIndex(pa => pa.IsVariantAttribute)
            .HasDatabaseName("ix_product_attributes_is_variant_attribute");
        builder
            .HasIndex(pa => pa.IsFilterable)
            .HasDatabaseName("ix_product_attributes_is_filterable");
        builder
            .HasIndex(pa => pa.DisplayOrder)
            .HasDatabaseName("ix_product_attributes_display_order");
    }
}
