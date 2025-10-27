using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing CouponEntity data access.
/// </summary>
public sealed class CouponRepository : ICouponRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public CouponRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CouponEntity?> GetByIdAsync(
        Guid couponId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);
    }

    public async Task<CouponEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<CouponEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Coupons.AsNoTracking()
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

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

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Coupons.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Coupons.AnyAsync(c => c.Code == code, cancellationToken);
    }

    public async Task AddAsync(CouponEntity coupon, CancellationToken cancellationToken = default)
    {
        if (coupon == null)
            throw new ArgumentNullException(nameof(coupon));

        _logger.LogDebug("Adding new coupon: {CouponCode}", coupon.Code);
        await _context.Coupons.AddAsync(coupon, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(CouponEntity coupon)
    {
        if (coupon == null)
            throw new ArgumentNullException(nameof(coupon));

        _logger.LogDebug("Updating coupon: {CouponId}", coupon.Id);
        _context.Coupons.Update(coupon);
    }

    public void Remove(CouponEntity coupon)
    {
        if (coupon == null)
            throw new ArgumentNullException(nameof(coupon));

        _logger.LogDebug("Removing coupon: {CouponId}", coupon.Id);
        _context.Coupons.Remove(coupon);
    }

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
