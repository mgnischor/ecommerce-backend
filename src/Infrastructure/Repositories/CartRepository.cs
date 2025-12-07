using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing shopping cart data access operations.
/// </summary>
/// <remarks>
/// Provides data access methods for <see cref="CartEntity"/> including customer-based
/// cart retrieval and management. Typically, each customer has one active shopping cart.
/// All query operations use AsNoTracking for optimal read performance. Supports
/// cart lifecycle management from creation through checkout or abandonment.
/// </remarks>
public sealed class CartRepository : ICartRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CartRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public CartRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CartEntity?> GetByIdAsync(
        Guid cartId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Carts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CartEntity?> GetByIdWithItemsAsync(
        Guid cartId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(cartId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CartEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Carts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CartEntity?> GetByUserIdWithItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByUserIdAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CartEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Carts.AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Carts.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CartEntity cart, CancellationToken cancellationToken = default)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        _logger.LogDebug("Adding new cart: {CartId}", cart.Id);
        await _context.Carts.AddAsync(cart, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(CartEntity cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        _logger.LogDebug("Updating cart: {CartId}", cart.Id);
        _context.Carts.Update(cart);
    }

    /// <inheritdoc />
    public void Remove(CartEntity cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        _logger.LogDebug("Removing cart: {CartId}", cart.Id);
        _context.Carts.Remove(cart);
    }

    /// <inheritdoc />
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
