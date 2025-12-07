using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing promotional coupon data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="CouponEntity"/> including
/// code-based lookups, active coupon filtering, validity checking (date ranges and usage limits),
/// and existence verification. Supports promotional campaigns, discount management, and
/// marketing initiatives. Validates coupon applicability based on activation status, date
/// ranges, and usage count constraints. All query operations use AsNoTracking for optimal
/// read performance. Coupons are sorted alphabetically by code for easy reference in
/// administrative interfaces.
/// </remarks>
public sealed class CouponRepository : ICouponRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CouponRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public CouponRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CouponEntity?> GetByIdAsync(
        Guid couponId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CouponEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CouponEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Coupons.AsNoTracking()
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CouponEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Coupons.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CouponEntity>> GetValidAsync(
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow;
        return await _context
            .Coupons.AsNoTracking()
            .Where(c =>
                c.IsActive
                && c.ValidFrom <= now
                && c.ValidUntil >= now
                && (c.MaxUsageCount == null || c.UsageCount < c.MaxUsageCount)
            )
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Coupons.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Coupons.AnyAsync(c => c.Code == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CouponEntity coupon, CancellationToken cancellationToken = default)
    {
        if (coupon == null)
            throw new ArgumentNullException(nameof(coupon));

        _logger.LogDebug("Adding new coupon: {CouponCode}", coupon.Code);
        await _context.Coupons.AddAsync(coupon, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(CouponEntity coupon)
    {
        if (coupon == null)
            throw new ArgumentNullException(nameof(coupon));

        _logger.LogDebug("Updating coupon: {CouponId}", coupon.Id);
        _context.Coupons.Update(coupon);
    }

    /// <inheritdoc />
    public void Remove(CouponEntity coupon)
    {
        if (coupon == null)
            throw new ArgumentNullException(nameof(coupon));

        _logger.LogDebug("Removing coupon: {CouponId}", coupon.Id);
        _context.Coupons.Remove(coupon);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveByIdAsync(
        Guid couponId,
        CancellationToken cancellationToken = default
    )
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(
            c => c.Id == couponId,
            cancellationToken
        );

        if (coupon == null)
            return false;

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
