using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Product catalog management endpoints
/// </summary>
/// <remarks>
/// Provides comprehensive product management including CRUD operations, search, filtering, and categorization.
/// Public endpoints are available for product browsing, while create/update/delete operations require authentication.
/// </remarks>
[Tags("Products")]
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
    /// <remarks>
    /// Returns a paginated list of all active products. Pagination headers are included in the response.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products?pageNumber=1&amp;pageSize=20
    ///
    /// Response headers:
    /// - X-Total-Count: Total number of products available
    /// - X-Page-Number: Current page number
    /// - X-Page-Size: Number of items per page
    ///
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based, default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, range: 1-100)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of products for the requested page</returns>
    /// <response code="200">Successfully retrieved the product list. Check response headers for pagination details.</response>
    /// <response code="400">Invalid pagination parameters. Page number must be >= 1, page size must be between 1 and 100.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
    /// Retrieves a specific product by its unique identifier
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single product including pricing, stock, and categorization.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    /// <param name="id">Product unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The requested product with all details</returns>
    /// <response code="200">Successfully retrieved the product.</response>
    /// <response code="404">Product not found with the specified ID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Retrieves a specific product by its SKU (Stock Keeping Unit)
    /// </summary>
    /// <remarks>
    /// Returns product details using the SKU identifier, which is unique across the catalog.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products/sku/PROD-12345
    ///
    /// </remarks>
    /// <param name="sku">Product SKU identifier (unique alphanumeric code)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The product matching the specified SKU</returns>
    /// <response code="200">Successfully retrieved the product.</response>
    /// <response code="404">Product not found with the specified SKU.</response>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Retrieves all products belonging to a specific category
    /// </summary>
    /// <remarks>
    /// Returns all products filtered by category ID. Returns an empty list if no products are found.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products/category/5
    ///
    /// </remarks>
    /// <param name="category">Product category identifier (integer)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of products in the specified category</returns>
    /// <response code="200">Successfully retrieved the product list. May be empty if category has no products.</response>
    [HttpGet("category/{category:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> GetProductsByCategory(
        int category,
        CancellationToken cancellationToken = default
    )
    {
        var products = await _productRepository.GetByCategoryAsync(category, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Retrieves all products marked as featured
    /// </summary>
    /// <remarks>
    /// Returns products that have been marked as featured for homepage or promotional display.
    /// Useful for showcasing special or highlighted products.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products/featured
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of featured products</returns>
    /// <response code="200">Successfully retrieved the featured products list.</response>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> GetFeaturedProducts(
        CancellationToken cancellationToken = default
    )
    {
        var products = await _productRepository.GetFeaturedAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Retrieves all products currently on sale
    /// </summary>
    /// <remarks>
    /// Returns products that have active sale prices or promotional discounts.
    /// Useful for displaying sale pages and promotional campaigns.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products/on-sale
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of products with active sales</returns>
    /// <response code="200">Successfully retrieved the on-sale products list.</response>
    [HttpGet("on-sale")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductEntity>>> GetOnSaleProducts(
        CancellationToken cancellationToken = default
    )
    {
        var products = await _productRepository.GetOnSaleAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Searches products by name using a search term
    /// </summary>
    /// <remarks>
    /// Performs a case-insensitive search across product names. Returns all matching products.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products/search?searchTerm=laptop
    ///
    /// </remarks>
    /// <param name="searchTerm">Search term to match against product names (required, minimum 1 character)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of products matching the search criteria</returns>
    /// <response code="200">Successfully retrieved matching products. May be empty if no matches found.</response>
    /// <response code="400">Invalid request. Search term is required and cannot be empty.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
    /// Creates a new product in the catalog
    /// </summary>
    /// <remarks>
    /// Creates a new product with the provided data. The SKU must be unique across the catalog.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/products
    ///     {
    ///        "name": "Gaming Laptop",
    ///        "sku": "LAPTOP-001",
    ///        "description": "High-performance gaming laptop",
    ///        "price": 1299.99,
    ///        "stockQuantity": 50,
    ///        "category": 1,
    ///        "isFeatured": true,
    ///        "isOnSale": false
    ///     }
    ///
    /// **Required permissions:** Admin, Manager
    ///
    /// The response includes a Location header with the URI of the newly created product.
    ///
    /// </remarks>
    /// <param name="newProduct">Product data to create (SKU must be unique)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The created product with generated ID and timestamps</returns>
    /// <response code="201">Successfully created the product. Location header contains the URI of the new resource.</response>
    /// <response code="400">Invalid request. Product data is required and must pass validation.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="403">Forbidden. Requires Admin or Manager role.</response>
    /// <response code="409">Conflict. A product with the same SKU already exists.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
    /// <remarks>
    /// Replaces all properties of an existing product with the provided data.
    /// The product ID in the URL must match the ID in the request body.
    ///
    /// Sample request:
    ///
    ///     PUT /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "name": "Updated Gaming Laptop",
    ///        "sku": "LAPTOP-001",
    ///        "description": "Updated description",
    ///        "price": 1199.99,
    ///        "stockQuantity": 45,
    ///        "category": 1,
    ///        "isFeatured": false,
    ///        "isOnSale": true
    ///     }
    ///
    /// **Required permissions:** Admin, Manager
    ///
    /// **Note:** If changing the SKU, ensure the new SKU is not already in use.
    ///
    /// </remarks>
    /// <param name="id">Product unique identifier (must match the ID in request body)</param>
    /// <param name="updatedProduct">Complete updated product data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Successfully updated the product. No content returned.</response>
    /// <response code="400">Invalid request. ID mismatch or validation errors.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="403">Forbidden. Requires Admin or Manager role.</response>
    /// <response code="404">Product not found with the specified ID.</response>
    /// <response code="409">Conflict. The new SKU is already in use by another product.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
    /// Permanently deletes a product from the catalog
    /// </summary>
    /// <remarks>
    /// Permanently removes a product from the database. This action cannot be undone.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Admin only
    ///
    /// **Warning:** This is a destructive operation. Consider using the soft delete endpoint instead
    /// to preserve historical data and maintain referential integrity.
    ///
    /// </remarks>
    /// <param name="id">Product unique identifier</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Successfully deleted the product permanently.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="403">Forbidden. Only administrators can perform hard deletes.</response>
    /// <response code="404">Product not found with the specified ID.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Soft deletes a product (marks as deleted without removing from database)
    /// </summary>
    /// <remarks>
    /// Marks a product as deleted without permanently removing it from the database.
    /// The product will be excluded from normal queries but can be restored if needed.
    /// This preserves historical data and maintains referential integrity.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6/soft-delete
    ///
    /// **Required permissions:** Admin, Manager
    ///
    /// </remarks>
    /// <param name="id">Product unique identifier</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Successfully soft deleted the product.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="403">Forbidden. Requires Admin or Manager role.</response>
    /// <response code="404">Product not found with the specified ID.</response>
    [HttpPatch("{id:guid}/soft-delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Returns a list of all available endpoints for the Products API
    /// </summary>
    /// <remarks>
    /// Provides API discovery by listing all available endpoints with their HTTP methods and paths.
    /// Useful for client applications to dynamically discover API capabilities.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/products/endpoints
    ///
    /// </remarks>
    /// <returns>List of available endpoints with methods and paths</returns>
    /// <response code="200">Successfully retrieved the endpoints list.</response>
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
    /// Returns allowed HTTP methods for the Products API
    /// </summary>
    /// <remarks>
    /// Returns the allowed HTTP methods in the Allow response header.
    /// Useful for CORS preflight requests and API capability discovery.
    ///
    /// Sample request:
    ///
    ///     OPTIONS /api/v1/products
    ///
    /// </remarks>
    /// <returns>No content with Allow header containing supported methods</returns>
    /// <response code="204">No content. Check the Allow header for supported HTTP methods.</response>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOptions()
    {
        Response.Headers.Append("Allow", "GET,POST,PUT,PATCH,DELETE,OPTIONS");
        return NoContent();
    }
}
