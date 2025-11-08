using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core configuration for FinancialTransactionEntity
/// </summary>
/// <remarks>
/// Configures database schema, indexes, relationships, and constraints for financial transactions.
/// Ensures optimal query performance and data integrity for financial operations.
/// </remarks>
public class FinancialTransactionConfiguration
    : IEntityTypeConfiguration<FinancialTransactionEntity>
{
    /// <summary>
    /// Configures the FinancialTransactionEntity for the database
    /// </summary>
    /// <param name="builder">Entity type builder for configuration</param>
    public void Configure(EntityTypeBuilder<FinancialTransactionEntity> builder)
    {
        // Table configuration
        builder.ToTable("financial_transactions");

        // Primary key
        builder.HasKey(ft => ft.Id);
        builder.Property(ft => ft.Id).HasColumnName("id").IsRequired();

        // Required fields
        builder
            .Property(ft => ft.TransactionNumber)
            .HasColumnName("transaction_number")
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(ft => ft.TransactionType)
            .HasColumnName("transaction_type")
            .HasConversion(
                v => v.ToString(),
                v => (FinancialTransactionType)Enum.Parse(typeof(FinancialTransactionType), v)
            )
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(ft => ft.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder
            .Property(ft => ft.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("USD");

        builder.Property(ft => ft.TransactionDate).HasColumnName("transaction_date").IsRequired();

        builder
            .Property(ft => ft.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder
            .Property(ft => ft.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("Pending");

        builder
            .Property(ft => ft.NetAmount)
            .HasColumnName("net_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ft => ft.CreatedBy).HasColumnName("created_by").IsRequired();

        builder.Property(ft => ft.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.Property(ft => ft.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Optional fields
        builder.Property(ft => ft.OrderId).HasColumnName("order_id");

        builder.Property(ft => ft.PaymentId).HasColumnName("payment_id");

        builder.Property(ft => ft.InventoryTransactionId).HasColumnName("inventory_transaction_id");

        builder.Property(ft => ft.JournalEntryId).HasColumnName("journal_entry_id");

        builder.Property(ft => ft.ProductId).HasColumnName("product_id");

        builder.Property(ft => ft.Counterparty).HasColumnName("counterparty").HasMaxLength(200);

        builder
            .Property(ft => ft.ReferenceNumber)
            .HasColumnName("reference_number")
            .HasMaxLength(100);

        builder
            .Property(ft => ft.IsReconciled)
            .HasColumnName("is_reconciled")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ft => ft.ReconciledAt).HasColumnName("reconciled_at");

        builder.Property(ft => ft.ReconciledBy).HasColumnName("reconciled_by");

        builder
            .Property(ft => ft.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v =>
                    string.IsNullOrEmpty(v)
                        ? null
                        : (PaymentMethod?)Enum.Parse(typeof(PaymentMethod), v)
            )
            .HasMaxLength(50);

        builder
            .Property(ft => ft.PaymentProvider)
            .HasColumnName("payment_provider")
            .HasMaxLength(100);

        builder.Property(ft => ft.Notes).HasColumnName("notes").HasMaxLength(1000);

        builder
            .Property(ft => ft.TaxAmount)
            .HasColumnName("tax_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder
            .Property(ft => ft.FeeAmount)
            .HasColumnName("fee_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes for performance
        builder
            .HasIndex(ft => ft.TransactionNumber)
            .HasDatabaseName("ix_financial_transactions_transaction_number")
            .IsUnique();

        builder
            .HasIndex(ft => ft.TransactionDate)
            .HasDatabaseName("ix_financial_transactions_transaction_date");

        builder
            .HasIndex(ft => ft.TransactionType)
            .HasDatabaseName("ix_financial_transactions_transaction_type");

        builder.HasIndex(ft => ft.OrderId).HasDatabaseName("ix_financial_transactions_order_id");

        builder
            .HasIndex(ft => ft.PaymentId)
            .HasDatabaseName("ix_financial_transactions_payment_id");

        builder
            .HasIndex(ft => ft.InventoryTransactionId)
            .HasDatabaseName("ix_financial_transactions_inventory_transaction_id");

        builder
            .HasIndex(ft => ft.IsReconciled)
            .HasDatabaseName("ix_financial_transactions_is_reconciled");

        builder
            .HasIndex(ft => new { ft.TransactionDate, ft.TransactionType })
            .HasDatabaseName("ix_financial_transactions_date_type");

        builder
            .HasIndex(ft => ft.Counterparty)
            .HasDatabaseName("ix_financial_transactions_counterparty");

        // Navigation properties configuration
        builder
            .HasOne(ft => ft.InventoryTransaction)
            .WithMany()
            .HasForeignKey(ft => ft.InventoryTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(ft => ft.JournalEntry)
            .WithMany()
            .HasForeignKey(ft => ft.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(ft => ft.Payment)
            .WithMany()
            .HasForeignKey(ft => ft.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
