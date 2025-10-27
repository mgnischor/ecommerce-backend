using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing ReviewEntity data access.
/// </summary>
public sealed class ReviewRepository : IReviewRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<ReviewRepository> _logger;

    public ReviewRepository(PostgresqlContext context, ILogger<ReviewRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReviewEntity?> GetByIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewEntity>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .Where(r => r.CustomerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewEntity>> GetApprovedByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<double> GetAverageRatingByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        var reviews = await _context
            .Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync(cancellationToken);

        return reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reviews.CountAsync(cancellationToken);
    }

    public async Task<int> GetCountByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Reviews.CountAsync(r => r.ProductId == productId, cancellationToken);
    }

    public async Task AddAsync(ReviewEntity review, CancellationToken cancellationToken = default)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        _logger.LogDebug("Adding new review: {ReviewId}", review.Id);
        await _context.Reviews.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(ReviewEntity review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        _logger.LogDebug("Updating review: {ReviewId}", review.Id);
        _context.Reviews.Update(review);
    }

    public void Remove(ReviewEntity review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        _logger.LogDebug("Removing review: {ReviewId}", review.Id);
        _context.Reviews.Remove(review);
    }

    public async Task<bool> RemoveByIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default
    )
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(
            r => r.Id == reviewId,
            cancellationToken
        );

        if (review == null)
            return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
