using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the StoreEntity.
/// Maps the StoreEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class StoreEntityConfiguration : IEntityTypeConfiguration<StoreEntity>
{
    public void Configure(EntityTypeBuilder<StoreEntity> builder)
    {
        builder.ToTable("Stores", schema: "public");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");

        builder
            .Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(s => s.StoreCode)
            .HasColumnName("store_code")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsUnicode(true);

        builder
            .Property(s => s.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(256)
            .IsUnicode(false);

        builder
            .Property(s => s.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.Address)
            .HasColumnName("address")
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(true);

        builder
            .Property(s => s.City)
            .HasColumnName("city")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(s => s.State)
            .HasColumnName("state")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(s => s.PostalCode)
            .HasColumnName("postal_code")
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder
            .Property(s => s.Country)
            .HasColumnName("country")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder.Property(s => s.Latitude).HasColumnName("latitude").HasColumnType("decimal(10,7)");

        builder
            .Property(s => s.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("decimal(10,7)");

        builder.Property(s => s.ManagerId).HasColumnName("manager_id");

        builder.Property(s => s.OpeningHours).HasColumnName("opening_hours").HasColumnType("jsonb");

        builder
            .Property(s => s.Timezone)
            .HasColumnName("timezone")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasDefaultValue("UTC");

        builder
            .Property(s => s.Currency)
            .HasColumnName("currency")
            .IsRequired()
            .HasMaxLength(3)
            .IsUnicode(false)
            .HasDefaultValue("USD");

        builder
            .Property(s => s.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder
            .Property(s => s.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(s => s.IsDefault).HasColumnName("is_default").HasDefaultValue(false);

        builder
            .Property(s => s.SupportsPickup)
            .HasColumnName("supports_pickup")
            .HasDefaultValue(true);

        builder
            .Property(s => s.SupportsDelivery)
            .HasColumnName("supports_delivery")
            .HasDefaultValue(true);

        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(s => s.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);

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

        builder.HasIndex(s => s.StoreCode).IsUnique().HasDatabaseName("ix_stores_store_code");
        builder.HasIndex(s => s.Name).HasDatabaseName("ix_stores_name");
        builder.HasIndex(s => s.IsDefault).HasDatabaseName("ix_stores_is_default");
        builder.HasIndex(s => s.IsActive).HasDatabaseName("ix_stores_is_active");
        builder.HasIndex(s => s.DisplayOrder).HasDatabaseName("ix_stores_display_order");
        builder.HasIndex(s => s.City).HasDatabaseName("ix_stores_city");
        builder.HasIndex(s => s.Country).HasDatabaseName("ix_stores_country");
    }
}
