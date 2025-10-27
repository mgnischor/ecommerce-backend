using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class PostgresqlContext : DbContext
{
    public PostgresqlContext(DbContextOptions<PostgresqlContext> options)
        : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    
    // Contabilidade
    public DbSet<ChartOfAccountsEntity> ChartOfAccounts { get; set; }
    public DbSet<JournalEntryEntity> JournalEntries { get; set; }
    public DbSet<AccountingEntryEntity> AccountingEntries { get; set; }
    
    // Estoque
    public DbSet<InventoryTransactionEntity> InventoryTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresqlContext).Assembly);
    }
}
