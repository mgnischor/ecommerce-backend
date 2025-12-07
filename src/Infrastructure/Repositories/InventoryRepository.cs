using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing inventory stock level data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="InventoryEntity"/> including
/// product-based queries, low stock alerts, out-of-stock tracking, and existence checks.
/// Manages current stock levels, reorder points, and inventory locations. Supports
/// inventory monitoring for automated restocking alerts and stock availability checks
/// during order processing. All query operations use AsNoTracking for optimal read
/// performance. Critical for maintaining accurate product availability information.
/// </remarks>
public sealed class InventoryRepository : IInventoryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public InventoryRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<InventoryEntity?> GetByIdAsync(
        Guid inventoryId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == inventoryId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InventoryEntity?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InventoryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .OrderBy(i => i.ProductId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InventoryEntity>> GetLowStockAsync(
        int threshold,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .Where(i => i.QuantityInStock <= threshold && i.QuantityInStock > 0)
            .OrderBy(i => i.QuantityInStock)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InventoryEntity>> GetOutOfStockAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .Where(i => i.QuantityInStock == 0)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Inventories.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Inventories.AnyAsync(
            i => i.ProductId == productId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddAsync(
        InventoryEntity inventory,
        CancellationToken cancellationToken = default
    )
    {
        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory));

        _logger.LogDebug("Adding new inventory: {InventoryId}", inventory.Id);
        await _context.Inventories.AddAsync(inventory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(InventoryEntity inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory));

        _logger.LogDebug("Updating inventory: {InventoryId}", inventory.Id);
        _context.Inventories.Update(inventory);
    }

    /// <inheritdoc />
    public void Remove(InventoryEntity inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory));

        _logger.LogDebug("Removing inventory: {InventoryId}", inventory.Id);
        _context.Inventories.Remove(inventory);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveByIdAsync(
        Guid inventoryId,
        CancellationToken cancellationToken = default
    )
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(
            i => i.Id == inventoryId,
            cancellationToken
        );

        if (inventory == null)
            return false;

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
