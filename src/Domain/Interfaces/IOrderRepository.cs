namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for OrderEntity operations
/// </summary>
public interface IOrderRepository
{
    Task<Domain.Entities.OrderEntity?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.OrderEntity?> GetByIdWithItemsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.OrderEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.OrderEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.OrderEntity>> GetByStatusAsync(
        int status,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.OrderEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.OrderEntity order, CancellationToken cancellationToken = default);
    void Update(Domain.Entities.OrderEntity order);
    void Remove(Domain.Entities.OrderEntity order);
    Task<bool> RemoveByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
