using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the PromotionEntity.
/// Maps the PromotionEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class PromotionEntityConfiguration : IEntityTypeConfiguration<PromotionEntity>
{
    public void Configure(EntityTypeBuilder<PromotionEntity> builder)
    {
        builder.ToTable("promotions", schema: "public");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder
            .Property(p => p.UpdatedBy)
            .HasColumnName("updated_by");

        builder
            .Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode(true);

        builder
            .Property(p => p.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(0);

        builder
            .Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(p => p.DiscountPercentage)
            .HasColumnName("discount_percentage")
            .HasColumnType("decimal(5,2)");

        builder
            .Property(p => p.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(p => p.MinimumOrderAmount)
            .HasColumnName("minimum_order_amount")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(p => p.MaximumDiscountAmount)
            .HasColumnName("maximum_discount_amount")
            .HasColumnType("decimal(18,2)");

        builder
            .Property(p => p.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(p => p.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(p => p.MaxUsageCount)
            .HasColumnName("max_usage_count");

        builder
            .Property(p => p.UsageCount)
            .HasColumnName("usage_count")
            .HasDefaultValue(0);

        builder
            .Property(p => p.MaxUsagePerUser)
            .HasColumnName("max_usage_per_user");

        builder
            .Property(p => p.EligibleProductIds)
            .HasColumnName("eligible_product_ids")
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(p => p.EligibleCategoryIds)
            .HasColumnName("eligible_category_ids")
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(p => p.EligibleUserIds)
            .HasColumnName("eligible_user_ids")
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'");

        builder
            .Property(p => p.Priority)
            .HasColumnName("priority")
            .HasDefaultValue(0);

        builder
            .Property(p => p.IsCombinable)
            .HasColumnName("is_combinable")
            .HasDefaultValue(true);

        builder
            .Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder
            .Property(p => p.IsFeatured)
            .HasColumnName("is_featured")
            .HasDefaultValue(false);

        builder
            .Property(p => p.BannerUrl)
            .HasColumnName("banner_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder
            .Property(p => p.TermsAndConditions)
            .HasColumnName("terms_and_conditions")
            .HasMaxLength(5000)
            .IsUnicode(true);

        builder
            .Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

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

        builder.HasIndex(p => p.Code).HasDatabaseName("ix_promotions_code");
        builder.HasIndex(p => p.Type).HasDatabaseName("ix_promotions_type");
        builder.HasIndex(p => p.IsActive).HasDatabaseName("ix_promotions_is_active");
        builder.HasIndex(p => p.IsFeatured).HasDatabaseName("ix_promotions_is_featured");
        builder.HasIndex(p => p.StartDate).HasDatabaseName("ix_promotions_start_date");
        builder.HasIndex(p => p.EndDate).HasDatabaseName("ix_promotions_end_date");
        builder.HasIndex(p => new { p.StartDate, p.EndDate, p.IsActive }).HasDatabaseName("ix_promotions_active_dates");
    }
}
