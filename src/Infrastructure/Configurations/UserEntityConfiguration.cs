using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the UserEntity.
/// Maps the UserEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users", schema: "public");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder
            .Property(u => u.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("ce06e1a8-f688-44b6-b616-4badf09d9153");

        builder
            .Property(u => u.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder
            .Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false);

        builder
            .Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Address).HasColumnName("address").HasMaxLength(500).IsUnicode(true);

        builder.Property(u => u.City).HasColumnName("city").HasMaxLength(100).IsUnicode(true);

        builder.Property(u => u.Country).HasColumnName("country").HasMaxLength(100).IsUnicode(true);

        builder.Property(u => u.BirthDate).HasColumnName("birth_date").HasColumnType("date");

        builder
            .Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ix_users_email");
        builder.HasIndex(u => u.Username).IsUnique().HasDatabaseName("ix_users_username");
        builder.HasIndex(u => u.CreatedAt).HasDatabaseName("ix_users_created_at");
    }
}
