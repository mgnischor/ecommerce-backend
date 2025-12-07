using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing user data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="UserEntity"/> including
/// authentication-related queries (email, username lookups), pagination support,
/// and existence checks. Implements secure user data retrieval with proper tracking
/// management: AsNoTracking for read-only operations, and change tracking for
/// authentication scenarios where password verification requires entity updates.
/// All operations include comprehensive error logging for security auditing.
/// </remarks>
public sealed class UserRepository : IUserRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic and security audit information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public UserRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<UserEntity?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        // Not using AsNoTracking for authentication scenarios where we need to track changes
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Users.AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }
}
