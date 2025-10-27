namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for CartEntity operations
/// </summary>
public interface ICartRepository
{
    Task<Domain.Entities.CartEntity?> GetByIdAsync(
        Guid cartId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.CartEntity?> GetByIdWithItemsAsync(
        Guid cartId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.CartEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.CartEntity?> GetByUserIdWithItemsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CartEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.CartEntity cart, CancellationToken cancellationToken = default);
    void Update(Domain.Entities.CartEntity cart);
    void Remove(Domain.Entities.CartEntity cart);
    Task<bool> RemoveByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
}
