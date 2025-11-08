using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class PostgresqlContext : DbContext
{
    public PostgresqlContext(DbContextOptions<PostgresqlContext> options)
        : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }
    public DbSet<CartEntity> Carts { get; set; }
    public DbSet<CartItemEntity> CartItems { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<InventoryEntity> Inventories { get; set; }
    public DbSet<PaymentEntity> Payments { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<WishlistEntity> Wishlists { get; set; }
    public DbSet<WishlistItemEntity> WishlistItems { get; set; }
    public DbSet<CouponEntity> Coupons { get; set; }
    public DbSet<AddressEntity> Addresses { get; set; }

    // Accounting
    public DbSet<ChartOfAccountsEntity> ChartOfAccounts { get; set; }
    public DbSet<JournalEntryEntity> JournalEntries { get; set; }
    public DbSet<AccountingEntryEntity> AccountingEntries { get; set; }

    // Inventory
    public DbSet<InventoryTransactionEntity> InventoryTransactions { get; set; }

    // Financial
    public DbSet<FinancialTransactionEntity> FinancialTransactions { get; set; }

    // Shipping & Refunds
    public DbSet<ShipmentEntity> Shipments { get; set; }
    public DbSet<RefundEntity> Refunds { get; set; }
    public DbSet<ShippingZoneEntity> ShippingZones { get; set; }

    // Vendors & Suppliers
    public DbSet<VendorEntity> Vendors { get; set; }
    public DbSet<SupplierEntity> Suppliers { get; set; }

    // Products
    public DbSet<ProductVariantEntity> ProductVariants { get; set; }
    public DbSet<ProductAttributeEntity> ProductAttributes { get; set; }

    // Marketing & Notifications
    public DbSet<PromotionEntity> Promotions { get; set; }
    public DbSet<NotificationEntity> Notifications { get; set; }

    // Stores
    public DbSet<StoreEntity> Stores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresqlContext).Assembly);
    }
}
