namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductEntity operations
/// </summary>
public interface IProductRepository
{
    Task<Domain.Entities.ProductEntity?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.ProductEntity?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ProductEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ProductEntity>> GetByCategoryAsync(
        int category,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ProductEntity>> GetFeaturedAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ProductEntity>> GetOnSaleAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ProductEntity>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.ProductEntity product,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.ProductEntity product);
    void Remove(Domain.Entities.ProductEntity product);
    Task<bool> RemoveByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(Guid productId, CancellationToken cancellationToken = default);
}
