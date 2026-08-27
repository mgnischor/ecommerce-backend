using Comex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comex.Infrastructure.Configurations;

/// <summary>
/// Configuration for the GiftCardEntity.
/// Maps the GiftCardEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class GiftCardEntityConfiguration : IEntityTypeConfiguration<GiftCardEntity>
{
    public void Configure(EntityTypeBuilder<GiftCardEntity> builder)
    {
        builder.ToTable("GiftCards", schema: "public");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(g => g.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(g => g.UpdatedBy).HasColumnName("updated_by");

        builder
            .Property(g => g.CardNumber)
            .HasColumnName("card_number")
            .IsRequired()
            .HasMaxLength(19)
            .IsUnicode(false);

        builder
            .Property(g => g.Balance)
            .HasColumnName("balance")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(g => g.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(g => g.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
        builder.Property(g => g.AllowsReload).HasColumnName("allows_reload").HasDefaultValue(true);

        builder
            .Property(g => g.IssuedAt)
            .HasColumnName("issued_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(g => g.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(g => g.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(g => g.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(g => g.CardNumber).IsUnique().HasDatabaseName("ix_gift_cards_card_number");
        builder.HasIndex(g => g.IsActive).HasDatabaseName("ix_gift_cards_is_active");
    }
}
