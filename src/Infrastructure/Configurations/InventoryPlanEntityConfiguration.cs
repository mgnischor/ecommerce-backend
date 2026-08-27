using Comex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comex.Infrastructure.Configurations;

/// <summary>
/// Configuration for the InventoryPlanEntity.
/// Maps the InventoryPlanEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class InventoryPlanEntityConfiguration
    : IEntityTypeConfiguration<InventoryPlanEntity>
{
    public void Configure(EntityTypeBuilder<InventoryPlanEntity> builder)
    {
        builder.ToTable("InventoryPlans", schema: "public");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder.Property(p => p.ProductId).HasColumnName("product_id").IsRequired();

        builder
            .Property(p => p.CurrentStock)
            .HasColumnName("current_stock")
            .IsRequired()
            .HasDefaultValue(0);
        builder
            .Property(p => p.ReorderPoint)
            .HasColumnName("reorder_point")
            .IsRequired()
            .HasDefaultValue(0);
        builder
            .Property(p => p.AverageDailySales)
            .HasColumnName("average_daily_sales")
            .IsRequired()
            .HasDefaultValue(0);
        builder.Property(p => p.LeadTimeDays).HasColumnName("lead_time_days");
        builder.Property(p => p.ServiceLevelPercent).HasColumnName("service_level_percent");

        builder
            .Property(p => p.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(p => p.CostOfGoodsSold)
            .HasColumnName("cost_of_goods_sold")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(p => p.AverageInventoryValue)
            .HasColumnName("average_inventory_value")
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder
            .Property(p => p.SalesInLast90Days)
            .HasColumnName("sales_in_last_90_days")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(p => p.ProductId).HasDatabaseName("ix_inventory_plans_product_id");
        builder.HasIndex(p => p.IsDeleted).HasDatabaseName("ix_inventory_plans_is_deleted");
    }
}
