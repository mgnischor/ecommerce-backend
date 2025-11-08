using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the SupplierEntity.
/// Maps the SupplierEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class SupplierEntityConfiguration : IEntityTypeConfiguration<SupplierEntity>
{
    public void Configure(EntityTypeBuilder<SupplierEntity> builder)
    {
        builder.ToTable("Suppliers", schema: "public");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");

        builder
            .Property(s => s.CompanyName)
            .HasColumnName("company_name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(s => s.SupplierCode)
            .HasColumnName("supplier_code")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.ContactName)
            .HasColumnName("contact_name")
            .IsRequired()
            .HasMaxLength(200)
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
            .Property(s => s.AlternatePhone)
            .HasColumnName("alternate_phone")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.FaxNumber)
            .HasColumnName("fax_number")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.Website)
            .HasColumnName("website")
            .HasMaxLength(500)
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

        builder.Property(s => s.TaxId).HasColumnName("tax_id").HasMaxLength(50).IsUnicode(false);

        builder
            .Property(s => s.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.BankAccountNumber)
            .HasColumnName("bank_account_number")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.BankName)
            .HasColumnName("bank_name")
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(s => s.BankRoutingNumber)
            .HasColumnName("bank_routing_number")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(s => s.PaymentTerms)
            .HasColumnName("payment_terms")
            .HasMaxLength(100)
            .IsUnicode(true);

        builder
            .Property(s => s.CreditLimit)
            .HasColumnName("credit_limit")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(s => s.CurrentBalance)
            .HasColumnName("current_balance")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder
            .Property(s => s.Rating)
            .HasColumnName("rating")
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0m);

        builder.Property(s => s.LeadTimeDays).HasColumnName("lead_time_days").HasDefaultValue(0);

        builder
            .Property(s => s.MinimumOrderAmount)
            .HasColumnName("minimum_order_amount")
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.Notes).HasColumnName("notes").HasMaxLength(2000).IsUnicode(true);

        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(s => s.IsPreferred).HasColumnName("is_preferred").HasDefaultValue(false);

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

        builder
            .HasIndex(s => s.SupplierCode)
            .IsUnique()
            .HasDatabaseName("ix_suppliers_supplier_code");
        builder.HasIndex(s => s.CompanyName).HasDatabaseName("ix_suppliers_company_name");
        builder.HasIndex(s => s.Email).HasDatabaseName("ix_suppliers_email");
        builder.HasIndex(s => s.IsActive).HasDatabaseName("ix_suppliers_is_active");
        builder.HasIndex(s => s.IsPreferred).HasDatabaseName("ix_suppliers_is_preferred");
        builder.HasIndex(s => s.Rating).HasDatabaseName("ix_suppliers_rating");
    }
}
