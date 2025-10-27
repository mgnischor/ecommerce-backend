using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing ProductEntity data access.
/// Provides methods for CRUD operations and querying product data.
/// </summary>
public sealed class ProductRepository
{
    private readonly PostgresqlContext _context;

    public ProductRepository(PostgresqlContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ProductEntity?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<ProductEntity?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetByCategoryAsync(
        int category,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => !p.IsDeleted && (int)p.Category == category)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetFeaturedAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsFeatured && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetOnSaleAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsOnSale && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name.Contains(searchTerm))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products.CountAsync(p => !p.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Products.AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task AddAsync(
        ProductEntity product,
        CancellationToken cancellationToken = default
    )
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(ProductEntity product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        _context.Products.Update(product);
    }

    public void Remove(ProductEntity product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        _context.Products.Remove(product);
    }

    public async Task<bool> RemoveByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _context.Products.FirstOrDefaultAsync(
            p => p.Id == productId,
            cancellationToken
        );

        if (product == null)
            return false;

        _context.Products.Remove(product);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _context.Products.FirstOrDefaultAsync(
            p => p.Id == productId,
            cancellationToken
        );

        if (product == null)
            return false;

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        return true;
    }
}
