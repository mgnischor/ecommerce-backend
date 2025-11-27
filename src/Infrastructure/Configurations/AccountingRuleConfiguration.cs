using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Accounting Rules
/// </summary>
public class AccountingRuleConfiguration : IEntityTypeConfiguration<AccountingRuleEntity>
{
    public void Configure(EntityTypeBuilder<AccountingRuleEntity> builder)
    {
        builder.ToTable("AccountingRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RuleCode).IsRequired().HasMaxLength(50);

        builder.HasIndex(e => e.RuleCode).IsUnique();

        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);

        builder.Property(e => e.TransactionType).IsRequired().HasConversion<int>();

        builder.Property(e => e.DebitAccountCode).IsRequired().HasMaxLength(20);

        builder.Property(e => e.CreditAccountCode).IsRequired().HasMaxLength(20);

        builder.Property(e => e.Condition).HasMaxLength(200);

        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(e => e.CreatedAt).IsRequired();

        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.TransactionType);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.TransactionType, e.IsActive });
    }
}
