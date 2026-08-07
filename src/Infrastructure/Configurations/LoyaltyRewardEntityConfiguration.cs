using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the LoyaltyRewardEntity.
/// Maps the LoyaltyRewardEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class LoyaltyRewardEntityConfiguration : IEntityTypeConfiguration<LoyaltyRewardEntity>
{
    public void Configure(EntityTypeBuilder<LoyaltyRewardEntity> builder)
    {
        builder.ToTable("LoyaltyRewards", schema: "public");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder.Property(l => l.CustomerId).HasColumnName("customer_id").IsRequired();

        builder.Property(l => l.PointBalance).HasColumnName("point_balance").IsRequired();
        builder.Property(l => l.TotalPointsEarned).HasColumnName("total_points_earned").IsRequired();
        builder.Property(l => l.TotalPointsRedeemed).HasColumnName("total_points_redeemed").IsRequired();

        builder
            .Property(l => l.LastEarnedAt)
            .HasColumnName("last_earned_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(l => l.Tier)
            .HasColumnName("tier")
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Bronze");

        builder.Property(l => l.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .HasIndex(l => l.CustomerId)
            .IsUnique()
            .HasDatabaseName("ix_loyalty_rewards_customer_id");
    }
}
