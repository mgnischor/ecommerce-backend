using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing UserEntity data access.
/// Provides methods for CRUD operations and querying user data.
/// </summary>
public sealed class UserRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(PostgresqlContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserEntity?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogDebug("Retrieving user by ID: {UserId}", userId);
            var user = await _context
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                _logger.LogDebug("User not found with ID: {UserId}", userId);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserEntity?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<UserEntity?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<IReadOnlyList<UserEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Users.AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Users.AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            _logger.LogError("Attempt to add null user");
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            _logger.LogDebug("Adding new user: {Email}", user.Email);
            await _context.Users.AddAsync(user, cancellationToken);
            _logger.LogDebug("User added successfully: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user: {Email}", user.Email);
            throw;
        }
    }

    public void Update(UserEntity user)
    {
        if (user == null)
        {
            _logger.LogError("Attempt to update null user");
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            _logger.LogDebug("Updating user: {UserId}", user.Id);
            _context.Users.Update(user);
            _logger.LogDebug("User updated successfully: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public void Remove(UserEntity user)
    {
        if (user == null)
        {
            _logger.LogError("Attempt to remove null user");
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            _logger.LogDebug("Removing user: {UserId}", user.Id);
            _context.Users.Remove(user);
            _logger.LogDebug("User removed successfully: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> RemoveByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return false;

        _context.Users.Remove(user);
        return true;
    }
}
