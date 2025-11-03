using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.TestFixtures;

/// <summary>
/// Test-specific DbContext that configures entities for InMemory provider
/// </summary>
public class TestPostgresqlContext : PostgresqlContext
{
    public TestPostgresqlContext(DbContextOptions<PostgresqlContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities to ignore Dictionary properties for InMemory provider
        // InMemory provider doesn't support Dictionary<TKey, TValue> properties
        modelBuilder.Entity<NotificationEntity>().Ignore(e => e.Metadata);

        modelBuilder.Entity<ProductVariantEntity>().Ignore(e => e.Attributes);
    }
}
