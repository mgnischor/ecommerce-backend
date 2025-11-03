using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

/// <summary>
/// Configuration for the NotificationEntity.
/// Maps the NotificationEntity properties to the corresponding database columns and sets up constraints and indexes.
/// </summary>
internal sealed class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("notifications", schema: "public");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .Property(n => n.CreatedBy)
            .HasColumnName("created_by")
            .HasDefaultValue(Guid.Parse("ce06e1a8-f688-44b6-b616-4badf09d9153"));

        builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();

        builder
            .Property(n => n.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ECommerce.Domain.Enums.NotificationType.System);

        builder
            .Property(n => n.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder
            .Property(n => n.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode(true);

        builder
            .Property(n => n.ActionUrl)
            .HasColumnName("action_url")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(n => n.Icon).HasColumnName("icon").HasMaxLength(100).IsUnicode(false);

        builder.Property(n => n.RelatedEntityId).HasColumnName("related_entity_id");

        builder
            .Property(n => n.RelatedEntityType)
            .HasColumnName("related_entity_type")
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(n => n.IsRead).HasColumnName("is_read").HasDefaultValue(false);

        builder
            .Property(n => n.ReadAt)
            .HasColumnName("read_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(n => n.SendEmail).HasColumnName("send_email").HasDefaultValue(false);

        builder.Property(n => n.EmailSent).HasColumnName("email_sent").HasDefaultValue(false);

        builder
            .Property(n => n.EmailSentAt)
            .HasColumnName("email_sent_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(n => n.SendPush).HasColumnName("send_push").HasDefaultValue(false);

        builder.Property(n => n.PushSent).HasColumnName("push_sent").HasDefaultValue(false);

        builder
            .Property(n => n.PushSentAt)
            .HasColumnName("push_sent_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(n => n.Priority).HasColumnName("priority").HasDefaultValue(5);

        builder
            .Property(n => n.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(n => n.Metadata).HasColumnName("metadata").HasColumnType("jsonb");

        builder.Property(n => n.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder
            .Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(n => n.UserId).HasDatabaseName("ix_notifications_user_id");
        builder.HasIndex(n => n.Type).HasDatabaseName("ix_notifications_type");
        builder.HasIndex(n => n.IsRead).HasDatabaseName("ix_notifications_is_read");
        builder.HasIndex(n => n.CreatedAt).HasDatabaseName("ix_notifications_created_at");
        builder.HasIndex(n => n.Priority).HasDatabaseName("ix_notifications_priority");
        builder
            .HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("ix_notifications_user_id_is_read");
    }
}
