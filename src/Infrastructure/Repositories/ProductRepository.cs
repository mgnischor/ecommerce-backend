using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing ProductEntity data access.
/// Provides methods for CRUD operations and querying product data.
/// </summary>
public sealed class ProductRepository : IProductRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(PostgresqlContext context, ILogger<ProductRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    public async Task AddAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        if (product == null)
        {
            _logger.LogError("Attempt to add null product");
            throw new ArgumentNullException(nameof(product));
        }

        try
        {
            _logger.LogDebug("Adding new product: {Sku}", product.Sku);
            await _context.Products.AddAsync(product, cancellationToken);
            _logger.LogDebug("Product added successfully: {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product: {Sku}", product.Sku);
            throw;
        }
    }

    public void Update(ProductEntity product)
    {
        if (product == null)
        {
            _logger.LogError("Attempt to update null product");
            throw new ArgumentNullException(nameof(product));
        }

        try
        {
            _logger.LogDebug("Updating product: {ProductId}", product.Id);
            _context.Products.Update(product);
            _logger.LogDebug("Product updated successfully: {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", product.Id);
            throw;
        }
    }

    public void Remove(ProductEntity product)
    {
        if (product == null)
        {
            _logger.LogError("Attempt to remove null product");
            throw new ArgumentNullException(nameof(product));
        }

        try
        {
            _logger.LogDebug("Removing product: {ProductId}", product.Id);
            _context.Products.Remove(product);
            _logger.LogDebug("Product removed successfully: {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product: {ProductId}", product.Id);
            throw;
        }
    }

    public async Task<bool> RemoveByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogDebug("Removing product by ID: {ProductId}", productId);
            var product = await _context.Products.FirstOrDefaultAsync(
                p => p.Id == productId,
                cancellationToken
            );

            if (product == null)
            {
                _logger.LogDebug("Product not found for removal: {ProductId}", productId);
                return false;
            }

            _context.Products.Remove(product);
            _logger.LogDebug("Product removed successfully: {ProductId}", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product by ID: {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> SoftDeleteAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogDebug("Soft deleting product: {ProductId}", productId);
            var product = await _context.Products.FirstOrDefaultAsync(
                p => p.Id == productId,
                cancellationToken
            );

            if (product == null)
            {
                _logger.LogDebug("Product not found for soft delete: {ProductId}", productId);
                return false;
            }

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            _logger.LogDebug("Product soft deleted successfully: {ProductId}", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting product: {ProductId}", productId);
            throw;
        }
    }
}
