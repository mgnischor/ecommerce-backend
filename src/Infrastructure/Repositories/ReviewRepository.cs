using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing product review data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="ReviewEntity"/> including
/// product-based queries, customer review history, approval filtering, rating calculations,
/// and review counting. Supports review moderation workflows with approval status tracking.
/// Calculates average product ratings for display on product pages. All query operations
/// use AsNoTracking for optimal read performance. Reviews are sorted by creation date in
/// descending order to show most recent feedback first. Essential for customer feedback
/// management and product quality insights.
/// </remarks>
public sealed class ReviewRepository : IReviewRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public ReviewRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ReviewEntity?> GetByIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReviewEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reviews.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reviews.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Reviews.CountAsync(r => r.ProductId == productId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(ReviewEntity review, CancellationToken cancellationToken = default)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        _logger.LogDebug("Adding new review: {ReviewId}", review.Id);
        await _context.Reviews.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(ReviewEntity review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        _logger.LogDebug("Updating review: {ReviewId}", review.Id);
        _context.Reviews.Update(review);
    }

    /// <inheritdoc />
    public void Remove(ReviewEntity review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        _logger.LogDebug("Removing review: {ReviewId}", review.Id);
        _context.Reviews.Remove(review);
    }

    /// <inheritdoc />
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
