using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Accounting Entries
/// </summary>
public class AccountingEntryConfiguration : IEntityTypeConfiguration<AccountingEntryEntity>
{
    public void Configure(EntityTypeBuilder<AccountingEntryEntity> builder)
    {
        builder.ToTable("AccountingEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.JournalEntryId)
            .IsRequired();

        builder.Property(e => e.AccountId)
            .IsRequired();

        builder.Property(e => e.EntryType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.CostCenter)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.JournalEntry)
            .WithMany()
            .HasForeignKey(e => e.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.JournalEntryId);
        builder.HasIndex(e => e.AccountId);
        builder.HasIndex(e => e.EntryType);
    }
}
