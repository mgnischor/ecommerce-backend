using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class PostgresqlContext : DbContext
{
    public PostgresqlContext(DbContextOptions<PostgresqlContext> options)
        : base(options) { }

    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresqlContext).Assembly);
    }
}
