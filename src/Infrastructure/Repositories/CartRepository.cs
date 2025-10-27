using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing CartEntity data access.
/// </summary>
public sealed class CartRepository : ICartRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<CartRepository> _logger;

    public CartRepository(PostgresqlContext context, ILogger<CartRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CartEntity?> GetByIdAsync(
        Guid cartId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Carts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
    }

    public async Task<CartEntity?> GetByIdWithItemsAsync(
        Guid cartId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(cartId, cancellationToken);
    }

    public async Task<CartEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Carts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == userId, cancellationToken);
    }

    public async Task<CartEntity?> GetByUserIdWithItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<CartEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Carts.AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Carts.CountAsync(cancellationToken);
    }

    public async Task AddAsync(CartEntity cart, CancellationToken cancellationToken = default)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        _logger.LogDebug("Adding new cart: {CartId}", cart.Id);
        await _context.Carts.AddAsync(cart, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(CartEntity cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        _logger.LogDebug("Updating cart: {CartId}", cart.Id);
        _context.Carts.Update(cart);
    }

    public void Remove(CartEntity cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        _logger.LogDebug("Removing cart: {CartId}", cart.Id);
        _context.Carts.Remove(cart);
    }

    public async Task<bool> RemoveByIdAsync(
        Guid cartId,
        CancellationToken cancellationToken = default
    )
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

        if (cart == null)
            return false;

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
