using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the ShippingZoneEntity.
/// Maps the ShippingZoneEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class ShippingZoneEntityConfiguration : IEntityTypeConfiguration<ShippingZoneEntity>
{
    public void Configure(EntityTypeBuilder<ShippingZoneEntity> builder)
    {
        builder.ToTable("ShippingZones", schema: "public");

        builder.HasKey(sz => sz.Id);
        builder.Property(sz => sz.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(sz => sz.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(sz => sz.UpdatedBy).HasColumnName("updated_by");

        builder
            .Property(sz => sz.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(sz => sz.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsUnicode(true);

        builder
            .Property(sz => sz.Countries)
            .HasColumnName("countries")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(sz => sz.States)
            .HasColumnName("states")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(sz => sz.PostalCodes)
            .HasColumnName("postal_codes")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(sz => sz.BaseRate)
            .HasColumnName("base_rate")
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder
            .Property(sz => sz.RatePerKg)
            .HasColumnName("rate_per_kg")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(sz => sz.RatePerItem)
            .HasColumnName("rate_per_item")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(sz => sz.FreeShippingThreshold)
            .HasColumnName("free_shipping_threshold")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(sz => sz.MinimumOrderAmount)
            .HasColumnName("minimum_order_amount")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(sz => sz.MaximumOrderAmount)
            .HasColumnName("maximum_order_amount")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(sz => sz.EstimatedDeliveryDaysMin)
            .HasColumnName("estimated_delivery_days_min")
            .IsRequired()
            .HasDefaultValue(1);

        builder
            .Property(sz => sz.EstimatedDeliveryDaysMax)
            .HasColumnName("estimated_delivery_days_max")
            .IsRequired()
            .HasDefaultValue(7);

        builder
            .Property(sz => sz.AvailableShippingMethods)
            .HasColumnName("available_shipping_methods")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder.Property(sz => sz.TaxRate).HasColumnName("tax_rate").HasColumnType("decimal(5,2)");

        builder.Property(sz => sz.Priority).HasColumnName("priority").HasDefaultValue(0);

        builder.Property(sz => sz.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(sz => sz.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(sz => sz.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(sz => sz.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(sz => sz.Name).HasDatabaseName("ix_shipping_zones_name");
        builder.HasIndex(sz => sz.IsActive).HasDatabaseName("ix_shipping_zones_is_active");
        builder.HasIndex(sz => sz.Priority).HasDatabaseName("ix_shipping_zones_priority");
    }
}
