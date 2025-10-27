namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for WishlistEntity operations
/// </summary>
public interface IWishlistRepository
{
    Task<Domain.Entities.WishlistEntity?> GetByIdAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.WishlistEntity?> GetByIdWithItemsAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.WishlistEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.WishlistEntity?> GetByUserIdWithItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.WishlistEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.WishlistEntity wishlist,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.WishlistEntity wishlist);
    void Remove(Domain.Entities.WishlistEntity wishlist);
    Task<bool> RemoveByIdAsync(Guid wishlistId, CancellationToken cancellationToken = default);
}
