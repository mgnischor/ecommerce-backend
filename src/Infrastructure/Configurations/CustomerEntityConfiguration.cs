using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the CustomerEntity.
/// Maps the CustomerEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class CustomerEntityConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("Customers", schema: "public");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();

        builder
            .Property(c => c.TotalSpending)
            .HasColumnName("total_spending")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(c => c.TotalOrders)
            .HasColumnName("total_orders")
            .IsRequired()
            .HasDefaultValue(0);

        builder
            .Property(c => c.LastOrderDate)
            .HasColumnName("last_order_date")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(c => c.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(c => c.HistoricalAverageOrderDays)
            .HasColumnName("historical_average_order_days");

        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.UserId).IsUnique().HasDatabaseName("ix_customers_user_id");
        builder.HasIndex(c => c.IsActive).HasDatabaseName("ix_customers_is_active");
    }
}
