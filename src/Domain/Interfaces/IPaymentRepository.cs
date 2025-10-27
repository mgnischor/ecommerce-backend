namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for PaymentEntity operations
/// </summary>
public interface IPaymentRepository
{
    Task<Domain.Entities.PaymentEntity?> GetByIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.PaymentEntity?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.PaymentEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.PaymentEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.PaymentEntity>> GetByStatusAsync(
        int status,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.PaymentEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.PaymentEntity payment,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.PaymentEntity payment);
    void Remove(Domain.Entities.PaymentEntity payment);
    Task<bool> RemoveByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
