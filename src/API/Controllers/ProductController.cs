using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public sealed class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly PostgresqlContext _context;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductRepository productRepository,
        PostgresqlContext context,
        ILogger<ProductController> logger
    )
    {
        _productRepository =
            productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all products with pagination support
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> GetAllProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Getting all products - Page: {PageNumber}, PageSize: {PageSize}",
            pageNumber,
            pageSize
        );

        if (pageNumber < 1)
        {
            _logger.LogWarning("Invalid page number: {PageNumber}", pageNumber);
            return BadRequest("Page number must be greater than 0");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            _logger.LogWarning("Invalid page size: {PageSize}", pageSize);
            return BadRequest("Page size must be between 1 and 100");
        }

        var products = await _productRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            cancellationToken
        );
        var totalCount = await _productRepository.GetCountAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} products out of {TotalCount} total",
            products.Count,
            totalCount
        );

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page-Number", pageNumber.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(products);
    }

    /// <summary>
    /// Retrieves a product by ID
    /// </summary>
    /// <param name="id">Product unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product entity</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductEntity>> GetProductById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
            return NotFound(new { Message = $"Product with ID '{id}' not found" });

        return Ok(product);
    }

    /// <summary>
    /// Retrieves a product by SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product entity</returns>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductEntity>> GetProductBySku(
        string sku,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _productRepository.GetBySkuAsync(sku, cancellationToken);

        if (product == null)
            return NotFound(new { Message = $"Product with SKU '{sku}' not found" });

        return Ok(product);
    }

    /// <summary>
    /// Retrieves products by category
    /// </summary>
    /// <param name="category">Product category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products in the category</returns>
    [HttpGet("category/{category:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> GetProductsByCategory(
        int category,
        CancellationToken cancellationToken = default
    )
    {
        var products = await _productRepository.GetByCategoryAsync(category, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Retrieves featured products
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of featured products</returns>
    [HttpGet("featured")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> GetFeaturedProducts(
        CancellationToken cancellationToken = default
    )
    {
        var products = await _productRepository.GetFeaturedAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Retrieves products on sale
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products on sale</returns>
    [HttpGet("on-sale")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> GetOnSaleProducts(
        CancellationToken cancellationToken = default
    )
    {
        var products = await _productRepository.GetOnSaleAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Searches products by name
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching products</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> SearchProducts(
        [FromQuery] string searchTerm,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        var products = await _productRepository.SearchByNameAsync(searchTerm, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="newProduct">Product data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductEntity>> CreateProduct(
        [FromBody] ProductEntity newProduct,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Creating new product with SKU: {Sku}", newProduct?.Sku ?? "null");

        if (newProduct == null)
        {
            _logger.LogWarning("Attempt to create product with null data");
            return BadRequest("Product data is required");
        }

        if (await _productRepository.ExistsBySkuAsync(newProduct.Sku, cancellationToken))
        {
            _logger.LogWarning(
                "Attempt to create product with duplicate SKU: {Sku}",
                newProduct.Sku
            );
            return Conflict(
                new { Message = $"Product with SKU '{newProduct.Sku}' already exists" }
            );
        }

        newProduct.CreatedAt = DateTime.UtcNow;
        newProduct.UpdatedAt = DateTime.UtcNow;

        await _productRepository.AddAsync(newProduct, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully created product: {ProductId}, SKU: {Sku}",
            newProduct.Id,
            newProduct.Sku
        );

        return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product unique identifier</param>
    /// <param name="updatedProduct">Updated product data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] ProductEntity updatedProduct,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Updating product: {ProductId}", id);

        if (updatedProduct == null)
        {
            _logger.LogWarning("Attempt to update product with null data for ID: {ProductId}", id);
            return BadRequest("Product data is required");
        }

        if (id != updatedProduct.Id)
        {
            _logger.LogWarning(
                "Product ID mismatch in update request. URL ID: {UrlId}, Body ID: {BodyId}",
                id,
                updatedProduct.Id
            );
            return BadRequest("ID mismatch");
        }

        var existingProduct = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (existingProduct == null)
        {
            _logger.LogWarning("Attempt to update non-existent product: {ProductId}", id);
            return NotFound(new { Message = $"Product with ID '{id}' not found" });
        }

        var skuExists = await _productRepository.ExistsBySkuAsync(
            updatedProduct.Sku,
            cancellationToken
        );
        if (skuExists && existingProduct.Sku != updatedProduct.Sku)
        {
            _logger.LogWarning(
                "Attempt to update product {ProductId} with duplicate SKU: {Sku}",
                id,
                updatedProduct.Sku
            );
            return Conflict(
                new { Message = $"Product with SKU '{updatedProduct.Sku}' already exists" }
            );
        }

        updatedProduct.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(updatedProduct);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated product: {ProductId}", id);

        return NoContent();
    }

    /// <summary>
    /// Deletes a product by ID (hard delete)
    /// </summary>
    /// <param name="id">Product unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Hard deleting product: {ProductId}", id);

        var deleted = await _productRepository.RemoveByIdAsync(id, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("Attempt to delete non-existent product: {ProductId}", id);
            return NotFound(new { Message = $"Product with ID '{id}' not found" });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted product: {ProductId}", id);

        return NoContent();
    }

    /// <summary>
    /// Soft deletes a product by ID
    /// </summary>
    /// <param name="id">Product unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPatch("{id:guid}/soft-delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Soft deleting product: {ProductId}", id);

        var deleted = await _productRepository.SoftDeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("Attempt to soft delete non-existent product: {ProductId}", id);
            return NotFound(new { Message = $"Product with ID '{id}' not found" });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully soft deleted product: {ProductId}", id);

        return NoContent();
    }

    /// <summary>
    /// Returns available endpoints
    /// </summary>
    /// <returns>
    /// List of available endpoints
    /// </returns>
    [HttpGet("endpoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEndpoints()
    {
        var endpoints = new[]
        {
            new { Method = "GET", Path = "/api/v1/products" },
            new { Method = "POST", Path = "/api/v1/products" },
            new { Method = "GET", Path = "/api/v1/products/{id}" },
            new { Method = "PUT", Path = "/api/v1/products/{id}" },
            new { Method = "DELETE", Path = "/api/v1/products/{id}" },
            new { Method = "PATCH", Path = "/api/v1/products/{id}/soft-delete" },
            new { Method = "GET", Path = "/api/v1/products/sku/{sku}" },
            new { Method = "GET", Path = "/api/v1/products/category/{category}" },
            new { Method = "GET", Path = "/api/v1/products/featured" },
            new { Method = "GET", Path = "/api/v1/products/on-sale" },
            new { Method = "GET", Path = "/api/v1/products/search?searchTerm={searchTerm}" },
        };

        return Ok(endpoints);
    }

    /// <summary>
    /// Returns allowed HTTP methods for this endpoint
    /// </summary>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOptions()
    {
        Response.Headers.Append("Allow", "GET,POST,PUT,PATCH,DELETE,OPTIONS");
        return NoContent();
    }
}
