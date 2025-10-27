using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing InventoryEntity data access.
/// </summary>
public sealed class InventoryRepository : IInventoryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<InventoryRepository> _logger;

    public InventoryRepository(PostgresqlContext context, ILogger<InventoryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InventoryEntity?> GetByIdAsync(
        Guid inventoryId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == inventoryId, cancellationToken);
    }

    public async Task<InventoryEntity?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .OrderBy(i => i.ProductId)
            .ToListAsync(cancellationToken);
    }

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

    public async Task<IReadOnlyList<InventoryEntity>> GetOutOfStockAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Inventories.AsNoTracking()
            .Where(i => i.QuantityInStock == 0)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Inventories.CountAsync(cancellationToken);
    }

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

    public void Update(InventoryEntity inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory));

        _logger.LogDebug("Updating inventory: {InventoryId}", inventory.Id);
        _context.Inventories.Update(inventory);
    }

    public void Remove(InventoryEntity inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory));

        _logger.LogDebug("Removing inventory: {InventoryId}", inventory.Id);
        _context.Inventories.Remove(inventory);
    }

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
