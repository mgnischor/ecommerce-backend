using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing OrderEntity data access.
/// </summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public OrderRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderEntity?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<OrderEntity?> GetByIdWithItemsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

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

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.CountAsync(cancellationToken);
    }

    public async Task<int> GetCountByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Orders.CountAsync(o => o.CustomerId == userId, cancellationToken);
    }

    public async Task AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogDebug("Adding new order: {OrderId}", order.Id);
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(OrderEntity order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogDebug("Updating order: {OrderId}", order.Id);
        _context.Orders.Update(order);
    }

    public void Remove(OrderEntity order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogDebug("Removing order: {OrderId}", order.Id);
        _context.Orders.Remove(order);
    }

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
