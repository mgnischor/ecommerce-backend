using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Chart of Accounts
/// </summary>
public class ChartOfAccountsConfiguration : IEntityTypeConfiguration<ChartOfAccountsEntity>
{
    public void Configure(EntityTypeBuilder<ChartOfAccountsEntity> builder)
    {
        builder.ToTable("ChartOfAccounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AccountCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.AccountCode)
            .IsUnique();

        builder.Property(e => e.AccountName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.AccountType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Balance)
            .HasPrecision(18, 2);

        builder.Property(e => e.IsAnalytic)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => e.AccountType);
        builder.HasIndex(e => e.IsActive);
    }
}
