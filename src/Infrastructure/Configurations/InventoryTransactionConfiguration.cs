using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Inventory Transactions
/// </summary>
public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransactionEntity>
{
    public void Configure(EntityTypeBuilder<InventoryTransactionEntity> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TransactionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.TransactionNumber)
            .IsUnique();

        builder.Property(e => e.TransactionDate)
            .IsRequired();

        builder.Property(e => e.TransactionType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ProductId)
            .IsRequired();

        builder.Property(e => e.ProductSku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.FromLocation)
            .HasMaxLength(100);

        builder.Property(e => e.ToLocation)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.UnitCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.JournalEntry)
            .WithMany()
            .HasForeignKey(e => e.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.TransactionDate);
        builder.HasIndex(e => e.TransactionType);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.CreatedBy);
    }
}
