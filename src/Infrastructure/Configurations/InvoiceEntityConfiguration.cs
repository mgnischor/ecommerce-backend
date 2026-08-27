using Comex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comex.Infrastructure.Configurations;

/// <summary>
/// Configuration for the InvoiceEntity.
/// Maps the InvoiceEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class InvoiceEntityConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        builder.ToTable("Invoices", schema: "public");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(i => i.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(i => i.UpdatedBy).HasColumnName("updated_by");

        builder
            .Property(i => i.InvoiceNumber)
            .HasColumnName("invoice_number")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(i => i.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(i => i.CustomerId).HasColumnName("customer_id").IsRequired();

        builder
            .Property(i => i.Subtotal)
            .HasColumnName("subtotal")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(i => i.TaxAmount)
            .HasColumnName("tax_amount")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(i => i.ShippingCost)
            .HasColumnName("shipping_cost")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(i => i.Total)
            .HasColumnName("total")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(i => i.PaidAmount)
            .HasColumnName("paid_amount")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(i => i.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(i => i.IssuedAt)
            .HasColumnName("issued_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(i => i.IsPaid).HasColumnName("is_paid").HasDefaultValue(false);
        builder.Property(i => i.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("ix_invoices_invoice_number");
        builder.HasIndex(i => i.OrderId).HasDatabaseName("ix_invoices_order_id");
        builder.HasIndex(i => i.CustomerId).HasDatabaseName("ix_invoices_customer_id");
        builder.HasIndex(i => i.IsPaid).HasDatabaseName("ix_invoices_is_paid");
    }
}
