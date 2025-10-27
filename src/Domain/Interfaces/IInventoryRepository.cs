namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for InventoryEntity operations
/// </summary>
public interface IInventoryRepository
{
    Task<Domain.Entities.InventoryEntity?> GetByIdAsync(
        Guid inventoryId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.InventoryEntity?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.InventoryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.InventoryEntity>> GetLowStockAsync(
        int threshold,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.InventoryEntity>> GetOutOfStockAsync(
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(
        Domain.Entities.InventoryEntity inventory,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.InventoryEntity inventory);
    void Remove(Domain.Entities.InventoryEntity inventory);
    Task<bool> RemoveByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);
}
