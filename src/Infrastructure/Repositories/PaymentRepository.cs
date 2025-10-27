using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing PaymentEntity data access.
/// </summary>
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(PostgresqlContext context, ILogger<PaymentRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentEntity?> GetByIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }

    public async Task<PaymentEntity?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        // Note: PaymentEntity doesn't have UserId directly, need to join with Orders
        // For now, returning empty list - should be implemented with proper join
        return await _context
            .Payments.AsNoTracking()
            .Where(p => false) // Placeholder - needs proper implementation with order join
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetByStatusAsync(
        int status,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.AsNoTracking()
            .Where(p => (int)p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments.CountAsync(cancellationToken);
    }

    public async Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken = default)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _logger.LogDebug("Adding new payment: {PaymentId}", payment.Id);
        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(PaymentEntity payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _logger.LogDebug("Updating payment: {PaymentId}", payment.Id);
        _context.Payments.Update(payment);
    }

    public void Remove(PaymentEntity payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _logger.LogDebug("Removing payment: {PaymentId}", payment.Id);
        _context.Payments.Remove(payment);
    }

    public async Task<bool> RemoveByIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    )
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(
            p => p.Id == paymentId,
            cancellationToken
        );

        if (payment == null)
            return false;

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
