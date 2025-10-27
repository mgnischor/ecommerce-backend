using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing CategoryEntity data access.
/// </summary>
public sealed class CategoryRepository : ICategoryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(PostgresqlContext context, ILogger<CategoryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CategoryEntity?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
    }

    public async Task<CategoryEntity?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

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

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Categories.AnyAsync(c => c.Name == name, cancellationToken);
    }

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

    public void Update(CategoryEntity category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _logger.LogDebug("Updating category: {CategoryId}", category.Id);
        _context.Categories.Update(category);
    }

    public void Remove(CategoryEntity category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _logger.LogDebug("Removing category: {CategoryId}", category.Id);
        _context.Categories.Remove(category);
    }

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
