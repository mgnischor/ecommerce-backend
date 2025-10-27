namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for CouponEntity operations
/// </summary>
public interface ICouponRepository
{
    Task<Domain.Entities.CouponEntity?> GetByIdAsync(
        Guid couponId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.CouponEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CouponEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CouponEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.CouponEntity>> GetValidAsync(
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.CouponEntity coupon,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.CouponEntity coupon);
    void Remove(Domain.Entities.CouponEntity coupon);
    Task<bool> RemoveByIdAsync(Guid couponId, CancellationToken cancellationToken = default);
}
