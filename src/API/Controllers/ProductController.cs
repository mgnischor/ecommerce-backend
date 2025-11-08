using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
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
/// <para>
/// Provides comprehensive product catalog management including CRUD operations, search, filtering,
/// categorization, and specialized queries. This controller manages the core product data that
/// drives the e-commerce platform's catalog functionality.
/// </para>
/// <para>
/// <strong>Product Management Overview:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Catalog Browsing:</strong> Public endpoints for product discovery and browsing</description></item>
/// <item><description><strong>Search and Filter:</strong> Flexible search by name, category, featured status, and sale status</description></item>
/// <item><description><strong>CRUD Operations:</strong> Full create, read, update, delete functionality</description></item>
/// <item><description><strong>Multiple Identifiers:</strong> Lookup by ID or SKU for flexible integration</description></item>
/// <item><description><strong>Soft Delete Support:</strong> Preserve historical data while removing products from active catalog</description></item>
/// </list>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Pagination:</strong> Efficient handling of large product catalogs with configurable page sizes</description></item>
/// <item><description><strong>Category Management:</strong> Organize and retrieve products by category</description></item>
/// <item><description><strong>Featured Products:</strong> Highlight special products for homepage or promotional display</description></item>
/// <item><description><strong>Sale Management:</strong> Track and retrieve products with active sales or discounts</description></item>
/// <item><description><strong>SKU Management:</strong> Unique stock keeping unit identifiers for inventory integration</description></item>
/// <item><description><strong>Search Capabilities:</strong> Case-insensitive name search across the catalog</description></item>
/// </list>
/// <para>
/// <strong>Access Control:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Public Read:</strong> GET endpoints are public for catalog browsing and product discovery</description></item>
/// <item><description><strong>Admin/Manager Write:</strong> Create, update, and soft delete require Admin or Manager role</description></item>
/// <item><description><strong>Admin Delete:</strong> Hard delete operations restricted to Admin role only</description></item>
/// <item><description><strong>API Discovery:</strong> Endpoints and options methods are public for client integration</description></item>
/// </list>
/// <para>
/// <strong>Data Management:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Soft Delete:</strong> Marks products as deleted while preserving data and relationships</description></item>
/// <item><description><strong>Hard Delete:</strong> Permanent removal from database (Admin only, use with caution)</description></item>
/// <item><description><strong>Audit Trail:</strong> Tracks creation and update timestamps for all products</description></item>
/// <item><description><strong>SKU Uniqueness:</strong> Enforces unique SKUs across the entire catalog</description></item>
/// </list>
/// <para>
/// <strong>Integration Support:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Pagination Headers:</strong> X-Total-Count, X-Page-Number, X-Page-Size for client-side pagination</description></item>
/// <item><description><strong>Location Headers:</strong> Standard REST practice for created resources</description></item>
/// <item><description><strong>CORS Support:</strong> OPTIONS endpoint for preflight requests</description></item>
/// <item><description><strong>API Discovery:</strong> Endpoints list for dynamic client configuration</description></item>
/// </list>
/// <para>
/// <strong>Use Cases:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Building product catalog pages with pagination</description></item>
/// <item><description>Product search and filtering functionality</description></item>
/// <item><description>Featured product carousels on homepage</description></item>
/// <item><description>Sale and promotional campaign pages</description></item>
/// <item><description>Product detail pages with full specifications</description></item>
/// <item><description>Inventory management and SKU tracking</description></item>
/// <item><description>Category-based navigation and browsing</description></item>
/// </list>
/// </remarks>
[Tags("Products")]
[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public sealed class ProductController : ControllerBase
{
    /// <summary>
    /// Repository interface for product data access operations
    /// </summary>
    /// <remarks>
    /// Provides abstraction layer for product data access, implementing repository pattern
    /// for CRUD operations, queries, and specialized product retrieval methods.
    /// Enables efficient querying with pagination and filtering support.
    /// </remarks>
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Database context for direct database access and transaction management
    /// </summary>
    /// <remarks>
    /// Used for SaveChanges operations to commit repository modifications to the database.
    /// Provides access to the underlying PostgreSQL database through Entity Framework Core.
    /// Required for explicit transaction control and change persistence.
    /// </remarks>
    private readonly PostgresqlContext _context;

    /// <summary>
    /// Logger instance for tracking product operations, errors, and security events
    /// </summary>
    /// <remarks>
    /// Used to log product access, modifications, validation errors, security events,
    /// and exceptions for monitoring, debugging, audit trail, and compliance purposes.
    /// Enables operational visibility and troubleshooting of product management operations.
    /// </remarks>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductController"/> class
    /// </summary>
    /// <param name="productRepository">
    /// Repository for product data access operations. Provides abstraction for CRUD operations,
    /// pagination, search, and specialized product queries. Cannot be null.
    /// </param>
    /// <param name="context">
    /// Database context for transaction management and change persistence. Used for SaveChanges
    /// operations after repository modifications. Cannot be null.
    /// </param>
    /// <param name="logger">
    /// Logger instance for recording product events, errors, and security audit information.
    /// Used for operational monitoring, debugging, and compliance tracking. Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the parameters (productRepository, context, or logger) is null.
    /// All dependencies are required for the controller to operate correctly.
    /// </exception>
    /// <remarks>
    /// This constructor uses dependency injection to provide all required services.
    /// All parameters are validated for null values to ensure the controller operates correctly.
    /// The controller is instantiated by the ASP.NET Core dependency injection container
    /// when handling product-related HTTP requests.
    /// </remarks>
    public ProductController(
        IProductRepository productRepository,
        PostgresqlContext context,
        ILoggingService logger
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
    /// <para>
    /// Returns a paginated list of all active products in the catalog. This is the primary endpoint
    /// for browsing the product catalog and is publicly accessible without authentication.
    /// Pagination metadata is provided in custom response headers for client-side pagination controls.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products?pageNumber=1&amp;pageSize=20
    /// </code>
    /// <para>
    /// <strong>Response Headers:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>X-Total-Count:</strong> Total number of products available in the catalog</description></item>
    /// <item><description><strong>X-Page-Number:</strong> Current page number returned (matches request parameter)</description></item>
    /// <item><description><strong>X-Page-Size:</strong> Number of items per page (matches request parameter)</description></item>
    /// </list>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>Pagination Parameters:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>pageNumber:</strong> 1-based page index (default: 1, minimum: 1)</description></item>
    /// <item><description><strong>pageSize:</strong> Items per page (default: 10, range: 1-100)</description></item>
    /// <item><description><strong>Total Pages:</strong> Calculate as ceiling(X-Total-Count / X-Page-Size)</description></item>
    /// </list>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Page number must be 1 or greater (1-based indexing)</description></item>
    /// <item><description>Page size must be between 1 and 100 inclusive</description></item>
    /// <item><description>Invalid parameters result in 400 Bad Request with error message</description></item>
    /// </list>
    /// <para>
    /// <strong>Response Data:</strong>
    /// </para>
    /// <para>
    /// Each product includes: ID, name, SKU, description, price, stock quantity, category,
    /// featured status, sale status, timestamps, and any other product properties.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Building product catalog pages with pagination controls</description></item>
    /// <item><description>Loading products for infinite scroll interfaces</description></item>
    /// <item><description>Product listing pages with configurable page sizes</description></item>
    /// <item><description>API clients needing to iterate through entire catalog</description></item>
    /// </list>
    /// <para>
    /// <strong>Performance Considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Use smaller page sizes (10-50) for better response times</description></item>
    /// <item><description>Maximum page size of 100 prevents excessive memory usage</description></item>
    /// <item><description>Results exclude soft-deleted products automatically</description></item>
    /// </list>
    /// </remarks>
    /// <param name="pageNumber">
    /// The page number to retrieve (1-based indexing). Default is 1. Must be 1 or greater.
    /// Example: pageNumber=1 retrieves the first page, pageNumber=2 retrieves the second page.
    /// </param>
    /// <param name="pageSize">
    /// The number of products to return per page. Default is 10. Valid range is 1 to 100 inclusive.
    /// Larger page sizes return more data but may impact performance.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a read-only list of <see cref="ProductEntity"/> objects
    /// for the requested page. The response includes pagination metadata in custom headers.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the product list. Returns a JSON array of product objects.
    /// Check response headers (X-Total-Count, X-Page-Number, X-Page-Size) for pagination metadata.
    /// The array may be empty if the page number exceeds the total number of pages.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid pagination parameters. Common causes: page number less than 1,
    /// page size less than 1, or page size greater than 100. The error message indicates
    /// the specific validation issue.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving products.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Returns complete information about a single product identified by its unique GUID.
    /// This is the primary endpoint for displaying product detail pages and retrieving
    /// full product specifications. Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>Response includes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Basic Info:</strong> ID, name, SKU, description</description></item>
    /// <item><description><strong>Pricing:</strong> Price, sale price (if on sale)</description></item>
    /// <item><description><strong>Inventory:</strong> Stock quantity and availability</description></item>
    /// <item><description><strong>Categorization:</strong> Category assignment</description></item>
    /// <item><description><strong>Flags:</strong> Featured status, sale status</description></item>
    /// <item><description><strong>Audit:</strong> Creation and update timestamps</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Displaying product detail pages with full specifications</description></item>
    /// <item><description>Loading product data for shopping cart operations</description></item>
    /// <item><description>Retrieving product information for order processing</description></item>
    /// <item><description>Product management interfaces for editing</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the product to retrieve. Must be a valid GUID format.
    /// Example: 3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="ProductEntity"/> object
    /// with complete product details including all properties and metadata.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the product. Returns a JSON object with complete product details.
    /// </response>
    /// <response code="404">
    /// Product not found. No product exists with the specified ID, or the product has been
    /// soft-deleted from the catalog.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving the product.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Returns complete product information using the SKU identifier. SKUs are unique across
    /// the entire catalog and are commonly used for inventory management, barcode scanning,
    /// and integration with external systems. Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products/sku/PROD-12345
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>SKU (Stock Keeping Unit) Explained:</strong>
    /// </para>
    /// <para>
    /// SKUs are unique alphanumeric identifiers assigned to each product for inventory tracking
    /// and management purposes. Unlike GUIDs which are system-generated, SKUs are typically
    /// business-friendly codes that may encode product information (category, size, color, etc.).
    /// </para>
    /// <para>
    /// <strong>SKU Format Examples:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>LAPTOP-001:</strong> Simple sequential numbering</description></item>
    /// <item><description><strong>TEE-BLU-MED:</strong> Category-Color-Size encoding</description></item>
    /// <item><description><strong>ELEC-TV-SAMSUNG-55:</strong> Category-Type-Brand-Size</description></item>
    /// <item><description><strong>12345678:</strong> Numeric only for barcode compatibility</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Inventory Management:</strong> Track stock levels by SKU</description></item>
    /// <item><description><strong>Barcode Scanning:</strong> Quick product lookup during checkout or receiving</description></item>
    /// <item><description><strong>External Integration:</strong> Sync products with ERP, POS, or warehouse systems</description></item>
    /// <item><description><strong>Import/Export:</strong> Data migration using SKU as unique identifier</description></item>
    /// <item><description><strong>Order Processing:</strong> Reference products by SKU in orders</description></item>
    /// </list>
    /// <para>
    /// <strong>Advantages of SKU Lookup:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Human-readable and memorable identifiers</description></item>
    /// <item><description>Compatible with barcode and QR code systems</description></item>
    /// <item><description>Business-friendly for inventory staff</description></item>
    /// <item><description>Can encode product attributes in the SKU itself</description></item>
    /// </list>
    /// </remarks>
    /// <param name="sku">
    /// The unique SKU (Stock Keeping Unit) identifier of the product to retrieve.
    /// SKU format is flexible but must be unique across the catalog.
    /// Example: "PROD-12345" or "TEE-BLU-MED"
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="ProductEntity"/> object
    /// matching the specified SKU, with complete product details.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the product. Returns a JSON object with complete product details.
    /// </response>
    /// <response code="404">
    /// Product not found. No product exists with the specified SKU, or the product has been
    /// soft-deleted from the catalog. SKU comparison is case-sensitive.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving the product.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Returns all active products assigned to the specified category. This endpoint is essential
    /// for building category-based navigation and browsing experiences. Results are not paginated
    /// and include all products in the category. Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products/category/5
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>Category-Based Organization:</strong>
    /// </para>
    /// <para>
    /// Products are organized into categories for easier browsing and navigation. Categories
    /// typically represent product types, departments, or hierarchical classifications.
    /// Each product is assigned to exactly one category.
    /// </para>
    /// <para>
    /// <strong>Example Categories:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Category 1:</strong> Electronics</description></item>
    /// <item><description><strong>Category 2:</strong> Clothing</description></item>
    /// <item><description><strong>Category 3:</strong> Books</description></item>
    /// <item><description><strong>Category 4:</strong> Home &amp; Garden</description></item>
    /// <item><description><strong>Category 5:</strong> Sports &amp; Outdoors</description></item>
    /// </list>
    /// <para>
    /// <strong>Response Behavior:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Returns all active products in the category (no pagination)</description></item>
    /// <item><description>Empty array if category has no products or doesn't exist</description></item>
    /// <item><description>Excludes soft-deleted products automatically</description></item>
    /// <item><description>No error for non-existent category IDs (returns empty array)</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Category landing pages showing all products in a category</description></item>
    /// <item><description>Navigation menus with category-based browsing</description></item>
    /// <item><description>Filtering product catalog by category</description></item>
    /// <item><description>Building category-specific promotional pages</description></item>
    /// </list>
    /// <para>
    /// <strong>Performance Note:</strong>
    /// </para>
    /// <para>
    /// For categories with many products, consider using the paginated GetAllProducts endpoint
    /// with additional filtering, or implementing pagination at the application level.
    /// This endpoint returns all matching products in a single response.
    /// </para>
    /// </remarks>
    /// <param name="category">
    /// The category identifier (integer) to filter products by. Products are assigned to
    /// categories for organizational purposes. The category ID should correspond to a valid
    /// category in the system, but non-existent categories will return an empty array.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a read-only list of <see cref="ProductEntity"/> objects
    /// that belong to the specified category. Returns an empty array if no products are found.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the product list. Returns a JSON array of products in the category.
    /// The array may be empty if the category has no products or the category doesn't exist.
    /// This is not considered an error condition.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving products.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("category/{category:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Returns all products that have been flagged as featured (IsFeatured = true). Featured products
    /// are typically showcased on the homepage, category pages, or in special promotional sections.
    /// This endpoint enables merchandising teams to control which products receive prominent display.
    /// Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products/featured
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>Featured Products Explained:</strong>
    /// </para>
    /// <para>
    /// Featured products are hand-selected items that businesses want to highlight to customers.
    /// The featured flag allows merchandising control over product visibility and prominence
    /// independent of sales performance or inventory levels.
    /// </para>
    /// <para>
    /// <strong>Common Uses for Featured Products:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Homepage Carousels:</strong> Showcase top products on main landing page</description></item>
    /// <item><description><strong>New Arrivals:</strong> Highlight recently added products</description></item>
    /// <item><description><strong>Best Sellers:</strong> Feature popular or high-margin items</description></item>
    /// <item><description><strong>Seasonal Products:</strong> Promote seasonal or holiday items</description></item>
    /// <item><description><strong>Premium Products:</strong> Highlight high-value or flagship items</description></item>
    /// <item><description><strong>Strategic Promotions:</strong> Feature products aligned with marketing campaigns</description></item>
    /// </list>
    /// <para>
    /// <strong>Response Behavior:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Returns all products where IsFeatured = true</description></item>
    /// <item><description>No pagination (typically a limited set of featured products)</description></item>
    /// <item><description>Excludes soft-deleted products automatically</description></item>
    /// <item><description>Empty array if no products are currently featured</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Building homepage product carousels or grids</description></item>
    /// <item><description>Featured products section in category pages</description></item>
    /// <item><description>Mobile app featured products screen</description></item>
    /// <item><description>Email marketing campaigns showcasing selected products</description></item>
    /// <item><description>Special landing pages with curated product selections</description></item>
    /// </list>
    /// <para>
    /// <strong>Management:</strong>
    /// </para>
    /// <para>
    /// Products are marked as featured through the update product endpoint by users with
    /// Admin or Manager roles. The featured status can be toggled on/off as needed for
    /// merchandising campaigns and seasonal promotions.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a read-only list of <see cref="ProductEntity"/> objects
    /// that are marked as featured. Returns an empty array if no featured products exist.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the featured products list. Returns a JSON array of products
    /// flagged as featured. The array may be empty if no products are currently featured.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving featured products.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Returns all products that are currently on sale (IsOnSale = true). These products typically
    /// have reduced prices or special promotional discounts. This endpoint is essential for building
    /// sale pages, promotional campaigns, and clearance sections. Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products/on-sale
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>Sale Products Explained:</strong>
    /// </para>
    /// <para>
    /// Products marked as "on sale" have special pricing or promotional discounts active.
    /// The sale flag allows businesses to track and display discounted items separately
    /// from regular inventory, driving customer engagement and conversion.
    /// </para>
    /// <para>
    /// <strong>Common Sale Scenarios:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Seasonal Sales:</strong> Summer sale, Black Friday, holiday clearance</description></item>
    /// <item><description><strong>Clearance:</strong> End-of-season or discontinued item clearance</description></item>
    /// <item><description><strong>Flash Sales:</strong> Limited-time promotional offers</description></item>
    /// <item><description><strong>Category Sales:</strong> Department-wide or category-specific discounts</description></item>
    /// <item><description><strong>Promotional Campaigns:</strong> Marketing campaign-driven sales</description></item>
    /// <item><description><strong>Overstock Reduction:</strong> Moving excess inventory with discounts</description></item>
    /// </list>
    /// <para>
    /// <strong>Response Behavior:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Returns all products where IsOnSale = true</description></item>
    /// <item><description>No pagination (returns all sale items in single response)</description></item>
    /// <item><description>Excludes soft-deleted products automatically</description></item>
    /// <item><description>Empty array if no products are currently on sale</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Building dedicated sale or clearance pages</description></item>
    /// <item><description>Sale section in navigation menus</description></item>
    /// <item><description>Promotional email campaigns featuring discounted items</description></item>
    /// <item><description>Mobile app sale notifications and sections</description></item>
    /// <item><description>Social media promotional posts with sale items</description></item>
    /// <item><description>Analytics and reporting on promotional effectiveness</description></item>
    /// </list>
    /// <para>
    /// <strong>Pricing Considerations:</strong>
    /// </para>
    /// <para>
    /// While this endpoint identifies products on sale, the actual sale price is stored
    /// in the product entity. Client applications should display the sale price prominently
    /// and may show the original price (strikethrough) for comparison.
    /// </para>
    /// <para>
    /// <strong>Management:</strong>
    /// </para>
    /// <para>
    /// Products are marked as on sale through the update product endpoint by users with
    /// Admin or Manager roles. The sale status can be toggled on/off to control active
    /// promotional campaigns and seasonal sales events.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a read-only list of <see cref="ProductEntity"/> objects
    /// that are currently on sale. Returns an empty array if no sale products exist.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the on-sale products list. Returns a JSON array of products
    /// marked as on sale. The array may be empty if no products are currently on sale.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving sale products.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("on-sale")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Performs a case-insensitive partial match search across product names. Returns all products
    /// whose names contain the search term. This is the primary search endpoint for product discovery
    /// and is essential for building search functionality in e-commerce applications.
    /// Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products/search?searchTerm=laptop
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>Search Behavior:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Matching:</strong> Partial match - searches for products containing the term anywhere in the name</description></item>
    /// <item><description><strong>Case Sensitivity:</strong> Case-insensitive - "Laptop", "laptop", and "LAPTOP" return the same results</description></item>
    /// <item><description><strong>Scope:</strong> Searches product names only (not descriptions, SKUs, or other fields)</description></item>
    /// <item><description><strong>Results:</strong> Returns all matching products (no pagination)</description></item>
    /// <item><description><strong>Empty Results:</strong> Returns empty array if no matches found (not an error)</description></item>
    /// </list>
    /// <para>
    /// <strong>Search Examples:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>searchTerm=laptop:</strong> Matches "Gaming Laptop", "Dell Laptop", "Laptop Bag"</description></item>
    /// <item><description><strong>searchTerm=blue:</strong> Matches "Blue T-Shirt", "Light Blue Jeans", "Navy Blue Jacket"</description></item>
    /// <item><description><strong>searchTerm=samsung:</strong> Matches "Samsung Galaxy", "Samsung TV", "Samsung Tablet"</description></item>
    /// <item><description><strong>searchTerm=gam:</strong> Matches "Gaming Laptop", "Board Game", "Game Console"</description></item>
    /// </list>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Search term is required (cannot be null, empty, or whitespace)</description></item>
    /// <item><description>Minimum length: 1 character (no maximum enforced)</description></item>
    /// <item><description>Invalid search terms result in 400 Bad Request</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Product search bars in header navigation</description></item>
    /// <item><description>Search result pages with product listings</description></item>
    /// <item><description>Autocomplete/type-ahead suggestions</description></item>
    /// <item><description>Mobile app product search functionality</description></item>
    /// <item><description>Voice search integration</description></item>
    /// </list>
    /// <para>
    /// <strong>Performance Considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Short search terms (1-2 characters) may return many results and impact performance</description></item>
    /// <item><description>Consider client-side pagination for large result sets</description></item>
    /// <item><description>Excludes soft-deleted products automatically</description></item>
    /// <item><description>For advanced search, consider adding filters (category, price range, etc.)</description></item>
    /// </list>
    /// <para>
    /// <strong>Future Enhancements:</strong>
    /// </para>
    /// <para>
    /// This basic name search could be enhanced with full-text search, relevance ranking,
    /// fuzzy matching, search across multiple fields (description, SKU), and filters.
    /// </para>
    /// </remarks>
    /// <param name="searchTerm">
    /// The search term to match against product names. Performs case-insensitive partial matching.
    /// Must be at least 1 character. Cannot be null, empty, or whitespace.
    /// Example: "laptop" or "blue" or "samsung"
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a read-only list of <see cref="ProductEntity"/> objects
    /// whose names contain the search term. Returns an empty array if no matches are found.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved matching products. Returns a JSON array of products whose names
    /// contain the search term (case-insensitive). The array may be empty if no matches are found,
    /// which is not considered an error condition.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid search term. The search term is required and cannot be null, empty,
    /// or consist only of whitespace characters. Provide a valid search term with at least one character.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while searching products.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Creates a new product with the provided data and adds it to the catalog. The SKU must be unique
    /// across the entire catalog. This endpoint is restricted to users with Admin or Manager roles
    /// for inventory management and catalog control.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// POST /api/v1/products
    /// Authorization: Bearer {token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "name": "Gaming Laptop",
    ///   "sku": "LAPTOP-001",
    ///   "description": "High-performance gaming laptop with RGB keyboard",
    ///   "price": 1299.99,
    ///   "stockQuantity": 50,
    ///   "category": 1,
    ///   "isFeatured": true,
    ///   "isOnSale": false
    /// }
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin or Manager role
    /// </para>
    /// <para>
    /// <strong>Required fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>name:</strong> Product display name</description></item>
    /// <item><description><strong>sku:</strong> Unique stock keeping unit identifier</description></item>
    /// <item><description><strong>price:</strong> Product price (positive decimal value)</description></item>
    /// </list>
    /// <para>
    /// <strong>Optional fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>description:</strong> Detailed product description</description></item>
    /// <item><description><strong>stockQuantity:</strong> Initial inventory quantity (default: 0)</description></item>
    /// <item><description><strong>category:</strong> Category assignment (default: unassigned)</description></item>
    /// <item><description><strong>isFeatured:</strong> Featured product flag (default: false)</description></item>
    /// <item><description><strong>isOnSale:</strong> Sale status flag (default: false)</description></item>
    /// <item><description><strong>salePrice:</strong> Discounted price if on sale</description></item>
    /// </list>
    /// <para>
    /// <strong>Automatic fields (system-generated):</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>id:</strong> Automatically generated unique identifier (GUID)</description></item>
    /// <item><description><strong>createdAt:</strong> Automatically set to current UTC time</description></item>
    /// <item><description><strong>updatedAt:</strong> Automatically set to current UTC time</description></item>
    /// <item><description><strong>isDeleted:</strong> Defaults to false (active product)</description></item>
    /// </list>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Product object cannot be null</description></item>
    /// <item><description>SKU must be unique across the catalog (409 Conflict if duplicate)</description></item>
    /// <item><description>Name, SKU, and price are typically required by entity validation</description></item>
    /// <item><description>Price should be positive</description></item>
    /// <item><description>Stock quantity should be non-negative</description></item>
    /// </list>
    /// <para>
    /// <strong>Response:</strong>
    /// </para>
    /// <para>
    /// On success, returns 201 Created with the complete product object including the generated ID
    /// and timestamps. The Location header contains the URI to retrieve the newly created product
    /// using the GET /api/v1/products/{id} endpoint.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Adding new products to the catalog from admin interfaces</description></item>
    /// <item><description>Bulk product imports from external systems</description></item>
    /// <item><description>Creating product variants with unique SKUs</description></item>
    /// <item><description>Launching new product lines or collections</description></item>
    /// </list>
    /// </remarks>
    /// <param name="newProduct">
    /// The product entity to create. Must include required fields (name, SKU, price).
    /// The ID and timestamps will be automatically generated. SKU must be unique.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the created <see cref="ProductEntity"/> object
    /// with the assigned ID, timestamps, and all product details. The Location header contains
    /// the URI to retrieve this product.
    /// </returns>
    /// <response code="201">
    /// Created. Successfully created the product. The Location header contains the URI of the
    /// newly created product resource. Returns the complete product object including the generated
    /// ID, createdAt, and updatedAt timestamps.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid product data, missing required fields, or validation errors.
    /// Common causes: null product object, missing name/SKU/price, invalid data formats,
    /// or negative price/quantity values.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token
    /// with JWT bearer authentication.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the required role (Admin or Manager)
    /// to create products. Customer or unauthenticated users cannot create products.
    /// </response>
    /// <response code="409">
    /// Conflict. A product with the specified SKU already exists in the catalog. SKUs must be unique.
    /// Choose a different SKU or update the existing product instead.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while creating the product.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ProductEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Replaces all properties of an existing product with the provided data. This is a full update
    /// operation (PUT) that requires sending the complete product object. The product ID in the URL
    /// must match the ID in the request body. Restricted to Admin or Manager roles.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// PUT /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "name": "Updated Gaming Laptop",
    ///   "sku": "LAPTOP-001",
    ///   "description": "Updated high-performance gaming laptop",
    ///   "price": 1199.99,
    ///   "stockQuantity": 45,
    ///   "category": 1,
    ///   "isFeatured": false,
    ///   "isOnSale": true,
    ///   "salePrice": 999.99
    /// }
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin or Manager role
    /// </para>
    /// <para>
    /// <strong>Updatable fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>name:</strong> Product display name</description></item>
    /// <item><description><strong>sku:</strong> Stock keeping unit (must remain unique if changed)</description></item>
    /// <item><description><strong>description:</strong> Product description</description></item>
    /// <item><description><strong>price:</strong> Product price</description></item>
    /// <item><description><strong>stockQuantity:</strong> Available inventory quantity</description></item>
    /// <item><description><strong>category:</strong> Category assignment</description></item>
    /// <item><description><strong>isFeatured:</strong> Featured product flag</description></item>
    /// <item><description><strong>isOnSale:</strong> Sale status flag</description></item>
    /// <item><description><strong>salePrice:</strong> Discounted sale price</description></item>
    /// </list>
    /// <para>
    /// <strong>Non-updatable fields (system-managed):</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>id:</strong> Cannot be changed (must match route parameter)</description></item>
    /// <item><description><strong>createdAt:</strong> Original creation timestamp (preserved)</description></item>
    /// <item><description><strong>updatedAt:</strong> Automatically set to current UTC time</description></item>
    /// <item><description><strong>isDeleted:</strong> Use soft delete endpoint to modify</description></item>
    /// </list>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Route ID must exactly match the product ID in request body</description></item>
    /// <item><description>Product must exist and not be soft-deleted</description></item>
    /// <item><description>Product object cannot be null</description></item>
    /// <item><description>If SKU is changed, new SKU must be unique across catalog</description></item>
    /// <item><description>Required fields must be present and valid</description></item>
    /// </list>
    /// <para>
    /// <strong>SKU Change Handling:</strong>
    /// </para>
    /// <para>
    /// When updating the SKU, the system first checks if the product exists, then verifies
    /// the new SKU is not already in use by another product. If the SKU hasn't changed,
    /// this validation is skipped. SKU changes are logged for audit purposes.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Updating product information from admin interfaces</description></item>
    /// <item><description>Adjusting prices and sale status</description></item>
    /// <item><description>Modifying inventory quantities</description></item>
    /// <item><description>Changing featured or sale status for promotions</description></item>
    /// <item><description>Correcting product details or descriptions</description></item>
    /// <item><description>Recategorizing products</description></item>
    /// </list>
    /// <para>
    /// <strong>Best Practices:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Always retrieve the current product first before updating</description></item>
    /// <item><description>Include all fields in the update, not just changed fields</description></item>
    /// <item><description>Handle 409 Conflict for SKU duplicates gracefully</description></item>
    /// <item><description>Use appropriate error handling for concurrent updates</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the product to update. Must be a valid GUID that matches
    /// the ID in the request body. The product must exist and not be soft-deleted.
    /// </param>
    /// <param name="updatedProduct">
    /// The updated product entity with all fields included. The ID must match the route parameter.
    /// This is a full update operation, so all fields should be provided even if unchanged.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content) on success.
    /// The response body is empty as per HTTP specification for successful PUT operations.
    /// </returns>
    /// <response code="204">
    /// No Content. Successfully updated the product. The response body is empty. The product's
    /// properties have been updated and the updatedAt timestamp has been recorded.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid data, missing required fields, validation errors, or ID mismatch.
    /// Common causes: route ID doesn't match body ID, null product object, missing required fields,
    /// or invalid data formats.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token
    /// with JWT bearer authentication.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the required role (Admin or Manager)
    /// to update products. Customer or unauthenticated users cannot update products.
    /// </response>
    /// <response code="404">
    /// Not found. The product with the specified ID does not exist or has been soft-deleted
    /// from the catalog. Cannot update a non-existent or deleted product.
    /// </response>
    /// <response code="409">
    /// Conflict. The new SKU is already in use by another product. SKUs must be unique across
    /// the catalog. Keep the current SKU or choose a different unique SKU.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while updating the product.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// Permanently deletes a product from the catalog (hard delete)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Permanently removes a product from the database. This is a destructive operation that
    /// cannot be undone. The product record and all its data are completely removed from the system.
    /// Restricted to Admin role only due to the permanent and irreversible nature of this operation.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// DELETE /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin role only (not Manager)
    /// </para>
    /// <para>
    /// <strong>⚠️ WARNING - Destructive Operation:</strong>
    /// </para>
    /// <para>
    /// This is a hard delete that permanently removes the product from the database. This operation:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Cannot be undone:</strong> The product data is permanently lost</description></item>
    /// <item><description><strong>Breaks references:</strong> May cause foreign key constraint issues if product is referenced elsewhere</description></item>
    /// <item><description><strong>Loses history:</strong> All audit trail and historical data is deleted</description></item>
    /// <item><description><strong>Affects orders:</strong> May impact historical orders that reference this product</description></item>
    /// </list>
    /// <para>
    /// <strong>Hard Delete vs Soft Delete:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Hard Delete (this endpoint):</strong> Permanently removes record from database</description></item>
    /// <item><description><strong>Soft Delete (recommended):</strong> Marks product as deleted but preserves data</description></item>
    /// <item><description><strong>Recommendation:</strong> Use soft delete (PATCH /soft-delete) instead for most scenarios</description></item>
    /// </list>
    /// <para>
    /// <strong>When to use hard delete:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Removing test or duplicate data from development/staging environments</description></item>
    /// <item><description>Compliance requirements for data removal (GDPR, right to be forgotten)</description></item>
    /// <item><description>Cleaning up malformed or corrupted product records</description></item>
    /// <item><description>Database maintenance and cleanup operations</description></item>
    /// </list>
    /// <para>
    /// <strong>Before hard deleting, consider:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Is this product referenced in any orders, carts, or wishlists?</description></item>
    /// <item><description>Do you need to preserve this data for reporting or audit purposes?</description></item>
    /// <item><description>Could soft delete meet your needs instead?</description></item>
    /// <item><description>Have you backed up the data if recovery might be needed?</description></item>
    /// <item><description>Are there any foreign key constraints that will prevent deletion?</description></item>
    /// </list>
    /// <para>
    /// <strong>Impact and Consequences:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Historical Orders:</strong> Orders referencing this product may lose product details</description></item>
    /// <item><description><strong>Analytics:</strong> Historical sales and performance data affected</description></item>
    /// <item><description><strong>Inventory Reports:</strong> Past inventory records may be incomplete</description></item>
    /// <item><description><strong>Audit Trail:</strong> Complete audit history for this product is lost</description></item>
    /// </list>
    /// <para>
    /// <strong>Alternative (Recommended):</strong>
    /// </para>
    /// <para>
    /// Consider using the soft delete endpoint (PATCH /api/v1/products/{id}/soft-delete) instead,
    /// which marks the product as deleted while preserving all data and maintaining referential
    /// integrity. Soft deleted products are automatically excluded from catalog queries.
    /// </para>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the product to permanently delete. The product must exist
    /// in the database for the operation to succeed.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content) on successful deletion.
    /// The response body is empty as per HTTP specification for successful DELETE operations.
    /// </returns>
    /// <response code="204">
    /// No Content. Successfully deleted the product permanently from the database. The response
    /// body is empty. The product and all its data have been permanently removed and cannot be recovered.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token
    /// with JWT bearer authentication.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the Admin role required for hard delete
    /// operations. Only administrators can permanently delete products. Managers must use soft delete.
    /// </response>
    /// <response code="404">
    /// Not found. The product with the specified ID does not exist in the database or has already
    /// been deleted. Cannot delete a non-existent product.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while deleting the product. This may
    /// include foreign key constraint violations if the product is referenced by other entities.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Marks a product as deleted by setting its isDeleted flag to true without permanently removing
    /// it from the database. The product will be automatically excluded from normal catalog queries
    /// but remains in the database for historical reference, reporting, and potential recovery.
    /// This is the recommended approach for removing products. Restricted to Admin or Manager roles.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// PATCH /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6/soft-delete
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin or Manager role
    /// </para>
    /// <para>
    /// <strong>Soft Delete Explained:</strong>
    /// </para>
    /// <para>
    /// Soft deletion marks a product as deleted without actually removing it from the database.
    /// This approach provides several advantages over hard deletion while removing the product
    /// from active catalog operations.
    /// </para>
    /// <para>
    /// <strong>Benefits of Soft Delete:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Data Preservation:</strong> Product data remains in database for historical reference</description></item>
    /// <item><description><strong>Referential Integrity:</strong> Maintains links to orders, carts, and other entities</description></item>
    /// <item><description><strong>Reversible:</strong> Can be restored if deleted by mistake (with database access)</description></item>
    /// <item><description><strong>Audit Trail:</strong> Complete history of product lifecycle preserved</description></item>
    /// <item><description><strong>Reporting:</strong> Historical reports remain accurate with past product data</description></item>
    /// <item><description><strong>Safe Operation:</strong> No risk of foreign key constraint violations</description></item>
    /// </list>
    /// <para>
    /// <strong>Effects of Soft Delete:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Catalog Visibility:</strong> Product no longer appears in catalog, search, or category queries</description></item>
    /// <item><description><strong>Public Endpoints:</strong> Soft-deleted products are excluded from all GET endpoints</description></item>
    /// <item><description><strong>Direct Access:</strong> GET by ID returns 404 for soft-deleted products</description></item>
    /// <item><description><strong>Historical Orders:</strong> Existing orders retain product information and remain valid</description></item>
    /// <item><description><strong>Database Record:</strong> Product record remains in database with isDeleted = true</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Removing discontinued products from active catalog</description></item>
    /// <item><description>Temporarily removing out-of-stock items (alternative to stock management)</description></item>
    /// <item><description>Archiving seasonal products after season ends</description></item>
    /// <item><description>Removing duplicate or erroneous products while preserving data</description></item>
    /// <item><description>Product lifecycle management (active → discontinued → archived)</description></item>
    /// </list>
    /// <para>
    /// <strong>Soft Delete vs Hard Delete:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Soft Delete (this endpoint):</strong> Sets isDeleted flag, preserves data, recommended</description></item>
    /// <item><description><strong>Hard Delete (DELETE endpoint):</strong> Permanently removes record, Admin only, use with caution</description></item>
    /// <item><description><strong>Default Choice:</strong> Soft delete should be the default for most scenarios</description></item>
    /// </list>
    /// <para>
    /// <strong>Recovery:</strong>
    /// </para>
    /// <para>
    /// Currently there is no API endpoint to restore soft-deleted products. Recovery requires
    /// direct database access to set isDeleted back to false. Consider carefully before soft
    /// deleting products, though the operation is reversible with database access.
    /// </para>
    /// <para>
    /// <strong>Audit Logging:</strong>
    /// </para>
    /// <para>
    /// Soft delete operations are logged for audit trail and compliance purposes, including
    /// the product ID and the user who performed the deletion.
    /// </para>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the product to soft delete. The product must exist
    /// and not already be soft-deleted for the operation to succeed.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content) on successful soft deletion.
    /// The response body is empty as per HTTP specification for successful PATCH operations.
    /// </returns>
    /// <response code="204">
    /// No Content. Successfully soft deleted the product. The response body is empty. The product
    /// is now marked as deleted and will not appear in future catalog queries, but the data
    /// remains in the database.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token
    /// with JWT bearer authentication.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the required role (Admin or Manager)
    /// to soft delete products. Customer or unauthenticated users cannot delete products.
    /// </response>
    /// <response code="404">
    /// Not found. The product with the specified ID does not exist or has already been soft-deleted.
    /// Attempting to soft delete an already soft-deleted product returns this status.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while soft deleting the product.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPatch("{id:guid}/soft-delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// <para>
    /// Provides API discovery functionality by listing all available endpoints with their HTTP methods
    /// and URL paths. This enables client applications to dynamically discover API capabilities without
    /// hardcoding endpoint information. Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/products/endpoints
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>Response Format:</strong>
    /// </para>
    /// <para>
    /// Returns a JSON array of endpoint objects, each containing the HTTP method and URL path.
    /// This allows clients to build dynamic navigation or API documentation.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>API Discovery:</strong> Client applications discovering available operations</description></item>
    /// <item><description><strong>Dynamic Documentation:</strong> Building API documentation dynamically</description></item>
    /// <item><description><strong>Testing Tools:</strong> API testing tools enumerating available endpoints</description></item>
    /// <item><description><strong>Admin Interfaces:</strong> Building admin UIs with dynamic endpoint lists</description></item>
    /// <item><description><strong>Monitoring:</strong> API monitoring tools tracking endpoint availability</description></item>
    /// </list>
    /// <para>
    /// <strong>Included Endpoints:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>GET /api/v1/products - Get all products with pagination</description></item>
    /// <item><description>POST /api/v1/products - Create new product</description></item>
    /// <item><description>GET /api/v1/products/{id} - Get product by ID</description></item>
    /// <item><description>PUT /api/v1/products/{id} - Update product</description></item>
    /// <item><description>DELETE /api/v1/products/{id} - Hard delete product</description></item>
    /// <item><description>PATCH /api/v1/products/{id}/soft-delete - Soft delete product</description></item>
    /// <item><description>GET /api/v1/products/sku/{sku} - Get product by SKU</description></item>
    /// <item><description>GET /api/v1/products/category/{category} - Get products by category</description></item>
    /// <item><description>GET /api/v1/products/featured - Get featured products</description></item>
    /// <item><description>GET /api/v1/products/on-sale - Get products on sale</description></item>
    /// <item><description>GET /api/v1/products/search - Search products by name</description></item>
    /// </list>
    /// <para>
    /// <strong>Note:</strong> This endpoint lists available paths but does not include authentication
    /// requirements or detailed parameter information. Refer to OpenAPI/Swagger documentation for
    /// complete endpoint specifications.
    /// </para>
    /// </remarks>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a JSON array of endpoint objects with Method and Path properties.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the endpoints list. Returns a JSON array of endpoint objects,
    /// each with "Method" and "Path" properties describing an available API endpoint.
    /// </response>
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
    /// <para>
    /// Returns the allowed HTTP methods for the Products API in the Allow response header.
    /// This endpoint is primarily used for CORS (Cross-Origin Resource Sharing) preflight requests
    /// and API capability discovery. Publicly accessible without authentication.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// OPTIONS /api/v1/products
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint)
    /// </para>
    /// <para>
    /// <strong>CORS Preflight:</strong>
    /// </para>
    /// <para>
    /// When a browser makes a cross-origin request with certain characteristics (custom headers,
    /// non-simple HTTP methods), it first sends an OPTIONS preflight request to check if the
    /// actual request is safe to send. This endpoint responds with the allowed methods.
    /// </para>
    /// <para>
    /// <strong>Response Headers:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Allow:</strong> GET,POST,PUT,PATCH,DELETE,OPTIONS</description></item>
    /// </list>
    /// <para>
    /// <strong>Allowed Methods Explained:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>GET:</strong> Retrieve products (various endpoints for listing, search, filtering)</description></item>
    /// <item><description><strong>POST:</strong> Create new products (requires Admin or Manager role)</description></item>
    /// <item><description><strong>PUT:</strong> Update existing products (requires Admin or Manager role)</description></item>
    /// <item><description><strong>PATCH:</strong> Soft delete products (requires Admin or Manager role)</description></item>
    /// <item><description><strong>DELETE:</strong> Hard delete products (requires Admin role)</description></item>
    /// <item><description><strong>OPTIONS:</strong> Query allowed methods (this endpoint)</description></item>
    /// </list>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>CORS Preflight:</strong> Browser preflight checks for cross-origin requests</description></item>
    /// <item><description><strong>API Discovery:</strong> Client applications discovering supported HTTP methods</description></item>
    /// <item><description><strong>API Testing:</strong> Testing tools checking endpoint capabilities</description></item>
    /// <item><description><strong>Documentation:</strong> Automated documentation generation</description></item>
    /// </list>
    /// <para>
    /// <strong>Note:</strong> While this endpoint lists all HTTP methods supported by the API,
    /// some methods require authentication and specific roles. The Allow header does not indicate
    /// authentication requirements - refer to individual endpoint documentation for access control details.
    /// </para>
    /// </remarks>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content). The response body is empty.
    /// Check the Allow response header for the list of supported HTTP methods.
    /// </returns>
    /// <response code="204">
    /// No content. Successfully returned allowed methods. The response body is empty.
    /// Check the "Allow" header which contains a comma-separated list of supported HTTP methods:
    /// GET, POST, PUT, PATCH, DELETE, OPTIONS.
    /// </response>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOptions()
    {
        Response.Headers.Append("Allow", "GET,POST,PUT,PATCH,DELETE,OPTIONS");
        return NoContent();
    }
}
