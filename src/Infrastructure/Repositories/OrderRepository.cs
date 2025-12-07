using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing order data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="OrderEntity"/> including
/// customer-based queries, status filtering, pagination support, and order counting.
/// Supports order lifecycle management from creation through fulfillment. All query
/// operations use AsNoTracking for optimal read performance. Orders are sorted by
/// creation date in descending order by default to show most recent orders first.
/// </remarks>
public sealed class OrderRepository : IOrderRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public OrderRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<OrderEntity?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OrderEntity?> GetByIdWithItemsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(orderId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .Where(o => o.CustomerId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderEntity>> GetByStatusAsync(
        int status,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .Where(o => (int)o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Orders.CountAsync(o => o.CustomerId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogDebug("Adding new order: {OrderId}", order.Id);
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(OrderEntity order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogDebug("Updating order: {OrderId}", order.Id);
        _context.Orders.Update(order);
    }

    /// <inheritdoc />
    public void Remove(OrderEntity order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogDebug("Removing order: {OrderId}", order.Id);
        _context.Orders.Remove(order);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        var order = await _context.Orders.FirstOrDefaultAsync(
            o => o.Id == orderId,
            cancellationToken
        );

        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
