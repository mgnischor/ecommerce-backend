using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing WishlistEntity data access.
/// </summary>
public sealed class WishlistRepository : IWishlistRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public WishlistRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WishlistEntity?> GetByIdAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Wishlists.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == wishlistId, cancellationToken);
    }

    public async Task<WishlistEntity?> GetByIdWithItemsAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(wishlistId, cancellationToken);
    }

    public async Task<WishlistEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Wishlists.AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerId == userId, cancellationToken);
    }

    public async Task<WishlistEntity?> GetByUserIdWithItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<WishlistEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Wishlists.AsNoTracking()
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Wishlists.CountAsync(cancellationToken);
    }

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

    public void Update(WishlistEntity wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        _logger.LogDebug("Updating wishlist: {WishlistId}", wishlist.Id);
        _context.Wishlists.Update(wishlist);
    }

    public void Remove(WishlistEntity wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        _logger.LogDebug("Removing wishlist: {WishlistId}", wishlist.Id);
        _context.Wishlists.Remove(wishlist);
    }

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
