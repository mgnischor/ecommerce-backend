namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for ReviewEntity operations
/// </summary>
public interface IReviewRepository
{
    Task<Domain.Entities.ReviewEntity?> GetByIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ReviewEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ReviewEntity>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ReviewEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ReviewEntity>> GetApprovedByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ReviewEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<double> GetAverageRatingByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(
        Domain.Entities.ReviewEntity review,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.ReviewEntity review);
    void Remove(Domain.Entities.ReviewEntity review);
    Task<bool> RemoveByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);
}
