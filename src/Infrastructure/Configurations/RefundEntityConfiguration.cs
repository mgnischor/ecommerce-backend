using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the RefundEntity.
/// Maps the RefundEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class RefundEntityConfiguration : IEntityTypeConfiguration<RefundEntity>
{
    public void Configure(EntityTypeBuilder<RefundEntity> builder)
    {
        builder.ToTable("refunds", schema: "public");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(r => r.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder
            .Property(r => r.UpdatedBy)
            .HasColumnName("updated_by");

        builder
            .Property(r => r.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder
            .Property(r => r.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder
            .Property(r => r.PaymentId)
            .HasColumnName("payment_id");

        builder
            .Property(r => r.RefundNumber)
            .HasColumnName("refund_number")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(r => r.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(0);

        builder
            .Property(r => r.RefundAmount)
            .HasColumnName("refund_amount")
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder
            .Property(r => r.Reason)
            .HasColumnName("reason")
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(true);

        builder
            .Property(r => r.CustomerNotes)
            .HasColumnName("customer_notes")
            .HasMaxLength(2000)
            .IsUnicode(true);

        builder
            .Property(r => r.AdminNotes)
            .HasColumnName("admin_notes")
            .HasMaxLength(2000)
            .IsUnicode(true);

        builder
            .Property(r => r.OrderItemIds)
            .HasColumnName("order_item_ids")
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(r => r.RequiresReturn)
            .HasColumnName("requires_return")
            .HasDefaultValue(true);

        builder
            .Property(r => r.ReturnTrackingNumber)
            .HasColumnName("return_tracking_number")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder
            .Property(r => r.ReturnedAt)
            .HasColumnName("returned_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(r => r.ApprovedAt)
            .HasColumnName("approved_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(r => r.ApprovedBy)
            .HasColumnName("approved_by");

        builder
            .Property(r => r.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(r => r.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(r => r.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(500)
            .IsUnicode(true);

        builder
            .Property(r => r.TransactionId)
            .HasColumnName("transaction_id")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder
            .Property(r => r.RestockingFee)
            .HasColumnName("restocking_fee")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(r => r.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder
            .Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(r => r.RefundNumber).IsUnique().HasDatabaseName("ix_refunds_refund_number");
        builder.HasIndex(r => r.OrderId).HasDatabaseName("ix_refunds_order_id");
        builder.HasIndex(r => r.CustomerId).HasDatabaseName("ix_refunds_customer_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_refunds_status");
        builder.HasIndex(r => r.CreatedAt).HasDatabaseName("ix_refunds_created_at");
    }
}
