namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for UserEntity operations
/// </summary>
public interface IUserRepository
{
    Task<Domain.Entities.UserEntity?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.UserEntity?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.UserEntity?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.UserEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.UserEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Domain.Entities.UserEntity user, CancellationToken cancellationToken = default);
    void Update(Domain.Entities.UserEntity user);
    void Remove(Domain.Entities.UserEntity user);
    Task<bool> RemoveByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
