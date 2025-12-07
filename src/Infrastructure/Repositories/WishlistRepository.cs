using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing customer wishlist data access operations.
/// </summary>
/// <remarks>
/// Provides data access methods for <see cref="WishlistEntity"/> including customer-based
/// wishlist retrieval and management. Typically, each customer has one primary wishlist
/// for tracking desired products. Supports wishlist features for later purchase, gift
/// registries, and product interest tracking. All query operations use AsNoTracking for
/// optimal read performance. Enables customer engagement through saved product lists
/// and purchase intent signals.
/// </remarks>
public sealed class WishlistRepository : IWishlistRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WishlistRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public WishlistRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<WishlistEntity?> GetByIdAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Wishlists.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == wishlistId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WishlistEntity?> GetByIdWithItemsAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(wishlistId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WishlistEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Wishlists.AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WishlistEntity?> GetByUserIdWithItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByUserIdAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WishlistEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Wishlists.AsNoTracking()
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Wishlists.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        WishlistEntity wishlist,
        CancellationToken cancellationToken = default
    )
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        _logger.LogDebug("Adding new wishlist: {WishlistId}", wishlist.Id);
        await _context.Wishlists.AddAsync(wishlist, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(WishlistEntity wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        _logger.LogDebug("Updating wishlist: {WishlistId}", wishlist.Id);
        _context.Wishlists.Update(wishlist);
    }

    /// <inheritdoc />
    public void Remove(WishlistEntity wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        _logger.LogDebug("Removing wishlist: {WishlistId}", wishlist.Id);
        _context.Wishlists.Remove(wishlist);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveByIdAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default
    )
    {
        var wishlist = await _context.Wishlists.FirstOrDefaultAsync(
            w => w.Id == wishlistId,
            cancellationToken
        );

        if (wishlist == null)
            return false;

        _context.Wishlists.Remove(wishlist);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
