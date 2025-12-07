using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing product category data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="CategoryEntity"/> including
/// hierarchical category support (parent/sub-category relationships), active category
/// filtering, name-based lookups, and existence checks. Supports building category trees
/// for navigation and product organization. All query operations use AsNoTracking for
/// optimal read performance. Categories are sorted alphabetically by name for consistent
/// display in navigation menus and category selectors.
/// </remarks>
public sealed class CategoryRepository : ICategoryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public CategoryRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetParentCategoriesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetSubCategoriesAsync(
        Guid parentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Categories.AnyAsync(c => c.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        CategoryEntity category,
        CancellationToken cancellationToken = default
    )
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _logger.LogDebug("Adding new category: {CategoryName}", category.Name);
        await _context.Categories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(CategoryEntity category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _logger.LogDebug("Updating category: {CategoryId}", category.Id);
        _context.Categories.Update(category);
    }

    /// <inheritdoc />
    public void Remove(CategoryEntity category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _logger.LogDebug("Removing category: {CategoryId}", category.Id);
        _context.Categories.Remove(category);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        var category = await _context.Categories.FirstOrDefaultAsync(
            c => c.Id == categoryId,
            cancellationToken
        );

        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
