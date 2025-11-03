using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the ShipmentEntity.
/// Maps the ShipmentEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class ShipmentEntityConfiguration : IEntityTypeConfiguration<ShipmentEntity>
{
    public void Configure(EntityTypeBuilder<ShipmentEntity> builder)
    {
        builder.ToTable("shipments", schema: "public");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");

        builder.Property(s => s.OrderId).HasColumnName("order_id").IsRequired();

        builder
            .Property(s => s.ShippingAddressId)
            .HasColumnName("shipping_address_id")
            .IsRequired();

        builder
            .Property(s => s.TrackingNumber)
            .HasColumnName("tracking_number")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false);

        builder
            .Property(s => s.Carrier)
            .HasColumnName("carrier")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(s => s.ServiceType)
            .HasColumnName("service_type")
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(s => s.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ECommerce.Domain.Enums.ShipmentStatus.Preparing);

        builder
            .Property(s => s.ShippingCost)
            .HasColumnName("shipping_cost")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(s => s.Weight)
            .HasColumnName("weight")
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(s => s.Length).HasColumnName("length").HasColumnType("decimal(10,2)");

        builder.Property(s => s.Width).HasColumnName("width").HasColumnType("decimal(10,2)");

        builder.Property(s => s.Height).HasColumnName("height").HasColumnType("decimal(10,2)");

        builder
            .Property(s => s.ExpectedDeliveryDate)
            .HasColumnName("expected_delivery_date")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(s => s.DeliveredAt)
            .HasColumnName("delivered_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(s => s.ShippedAt)
            .HasColumnName("shipped_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(s => s.TrackingUrl)
            .HasColumnName("tracking_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(s => s.Notes).HasColumnName("notes").HasMaxLength(1000).IsUnicode(true);

        builder.Property(s => s.IsInsured).HasColumnName("is_insured").HasDefaultValue(false);

        builder
            .Property(s => s.InsuranceAmount)
            .HasColumnName("insurance_amount")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(s => s.RequiresSignature)
            .HasColumnName("requires_signature")
            .HasDefaultValue(false);

        builder
            .Property(s => s.ReceivedBy)
            .HasColumnName("received_by")
            .HasMaxLength(200)
            .IsUnicode(true);

        builder.Property(s => s.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(s => s.OrderId).HasDatabaseName("ix_shipments_order_id");
        builder.HasIndex(s => s.TrackingNumber).HasDatabaseName("ix_shipments_tracking_number");
        builder.HasIndex(s => s.Status).HasDatabaseName("ix_shipments_status");
        builder.HasIndex(s => s.CreatedAt).HasDatabaseName("ix_shipments_created_at");
        builder.HasIndex(s => s.DeliveredAt).HasDatabaseName("ix_shipments_delivered_at");
    }
}
