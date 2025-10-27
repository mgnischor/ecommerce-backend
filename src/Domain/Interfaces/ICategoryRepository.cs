namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for CategoryEntity operations
/// </summary>
public interface ICategoryRepository
{
    Task<Domain.Entities.CategoryEntity?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.CategoryEntity?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CategoryEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CategoryEntity>> GetParentCategoriesAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CategoryEntity>> GetSubCategoriesAsync(
        Guid parentId,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.CategoryEntity category,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.CategoryEntity category);
    void Remove(Domain.Entities.CategoryEntity category);
    Task<bool> RemoveByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
