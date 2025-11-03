using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the VendorEntity.
/// Maps the VendorEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class VendorEntityConfiguration : IEntityTypeConfiguration<VendorEntity>
{
    public void Configure(EntityTypeBuilder<VendorEntity> builder)
    {
        builder.ToTable("vendors", schema: "public");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(v => v.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder
            .Property(v => v.UpdatedBy)
            .HasColumnName("updated_by");

        builder
            .Property(v => v.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder
            .Property(v => v.BusinessName)
            .HasColumnName("business_name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(v => v.StoreName)
            .HasColumnName("store_name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(v => v.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(256)
            .IsUnicode(false);

        builder
            .Property(v => v.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(v => v.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsUnicode(true);

        builder
            .Property(v => v.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder
            .Property(v => v.BannerUrl)
            .HasColumnName("banner_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder
            .Property(v => v.Address)
            .HasColumnName("address")
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(true);

        builder
            .Property(v => v.City)
            .HasColumnName("city")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(v => v.State)
            .HasColumnName("state")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(v => v.PostalCode)
            .HasColumnName("postal_code")
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder
            .Property(v => v.Country)
            .HasColumnName("country")
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(v => v.TaxId)
            .HasColumnName("tax_id")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(v => v.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(v => v.CommissionRate)
            .HasColumnName("commission_rate")
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(10.0m);

        builder
            .Property(v => v.Rating)
            .HasColumnName("rating")
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0m);

        builder
            .Property(v => v.TotalRatings)
            .HasColumnName("total_ratings")
            .HasDefaultValue(0);

        builder
            .Property(v => v.TotalSales)
            .HasColumnName("total_sales")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder
            .Property(v => v.TotalOrders)
            .HasColumnName("total_orders")
            .HasDefaultValue(0);

        builder
            .Property(v => v.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(0);

        builder
            .Property(v => v.BankAccountNumber)
            .HasColumnName("bank_account_number")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(v => v.BankName)
            .HasColumnName("bank_name")
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(v => v.BankRoutingNumber)
            .HasColumnName("bank_routing_number")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(v => v.PayPalEmail)
            .HasColumnName("paypal_email")
            .HasMaxLength(256)
            .IsUnicode(false);

        builder
            .Property(v => v.IsVerified)
            .HasColumnName("is_verified")
            .HasDefaultValue(false);

        builder
            .Property(v => v.VerifiedAt)
            .HasColumnName("verified_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(v => v.IsFeatured)
            .HasColumnName("is_featured")
            .HasDefaultValue(false);

        builder
            .Property(v => v.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder
            .Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(v => v.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(v => v.UserId).IsUnique().HasDatabaseName("ix_vendors_user_id");
        builder.HasIndex(v => v.Email).HasDatabaseName("ix_vendors_email");
        builder.HasIndex(v => v.StoreName).HasDatabaseName("ix_vendors_store_name");
        builder.HasIndex(v => v.Status).HasDatabaseName("ix_vendors_status");
        builder.HasIndex(v => v.Rating).HasDatabaseName("ix_vendors_rating");
        builder.HasIndex(v => v.IsFeatured).HasDatabaseName("ix_vendors_is_featured");
        builder.HasIndex(v => v.CreatedAt).HasDatabaseName("ix_vendors_created_at");
    }
}
