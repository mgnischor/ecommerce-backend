using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Journal Entries
/// </summary>
public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntryEntity>
{
    public void Configure(EntityTypeBuilder<JournalEntryEntity> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntryNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.EntryNumber)
            .IsUnique();

        builder.Property(e => e.EntryDate)
            .IsRequired();

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(e => e.History)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.IsPosted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => e.EntryDate);
        builder.HasIndex(e => e.DocumentType);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.InventoryTransactionId);
        builder.HasIndex(e => e.IsPosted);
    }
}
