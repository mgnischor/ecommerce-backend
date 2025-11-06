using System.Security.Claims;
using System.Text.RegularExpressions;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Product attribute management endpoints
/// </summary>
/// <remarks>
/// <para>
/// Provides comprehensive management of product attributes that define the characteristics and
/// specifications of products in the e-commerce catalog. Product attributes enable flexible
/// product configuration, variant management, filtering, and search capabilities.
/// </para>
/// <para>
/// <strong>Product Attributes Overview:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Standard Attributes:</strong> Basic product properties like brand, material, color (non-variant)</description></item>
/// <item><description><strong>Variant Attributes:</strong> Properties that create product variants (size, color, style)</description></item>
/// <item><description><strong>Searchable Attributes:</strong> Attributes indexed for product search functionality</description></item>
/// <item><description><strong>Filterable Attributes:</strong> Attributes used in product filtering and faceted navigation</description></item>
/// </list>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Attribute Definition:</strong> Create and manage custom product attributes with validation rules</description></item>
/// <item><description><strong>Data Type Support:</strong> Multiple data types (text, number, boolean, date, list)</description></item>
/// <item><description><strong>Variant Support:</strong> Flag attributes as variant-defining for product variations</description></item>
/// <item><description><strong>Validation Patterns:</strong> Custom regex patterns for attribute value validation</description></item>
/// <item><description><strong>Display Ordering:</strong> Control attribute display order in UI</description></item>
/// <item><description><strong>Code-Based Lookup:</strong> Retrieve attributes by unique code for system integration</description></item>
/// </list>
/// <para>
/// <strong>Access Control:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Public Read:</strong> GET endpoints are public (AllowAnonymous) for catalog display</description></item>
/// <item><description><strong>Admin/Manager Write:</strong> Create and update operations require Admin or Manager role</description></item>
/// <item><description><strong>Admin Delete:</strong> Soft delete operations restricted to Admin role only</description></item>
/// </list>
/// <para>
/// <strong>Data Management:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Soft Delete:</strong> Attributes are soft-deleted to maintain referential integrity</description></item>
/// <item><description><strong>Audit Trail:</strong> Tracks creation and modification with user IDs and timestamps</description></item>
/// <item><description><strong>Unique Codes:</strong> Each attribute has a unique code for system-level identification</description></item>
/// <item><description><strong>Display Control:</strong> Attributes ordered by display order for consistent UI presentation</description></item>
/// </list>
/// <para>
/// <strong>Use Cases:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Defining product specifications (dimensions, weight, capacity)</description></item>
/// <item><description>Creating product variants (size, color, material combinations)</description></item>
/// <item><description>Enabling product filtering in catalog pages</description></item>
/// <item><description>Supporting advanced product search</description></item>
/// <item><description>Managing product taxonomy and categorization</description></item>
/// </list>
/// </remarks>
[Tags("ProductAttributes")]
[ApiController]
[Route("api/v1/product-attributes")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Manager")]
public sealed class ProductAttributeController : ControllerBase
{
    /// <summary>
    /// Database context for product attribute data access operations
    /// </summary>
    /// <remarks>
    /// Provides direct access to the product attribute entities in the PostgreSQL database.
    /// Used for querying, creating, updating, and soft-deleting attribute records.
    /// </remarks>
    private readonly PostgresqlContext _context;

    /// <summary>
    /// Logger instance for tracking product attribute operations and errors
    /// </summary>
    /// <remarks>
    /// Used to log attribute access, modifications, validation errors, security events,
    /// and exceptions for monitoring, debugging, and audit trail purposes.
    /// </remarks>
    private readonly ILogger<ProductAttributeController> _logger;

    /// <summary>
    /// Maximum number of product attributes that can be retrieved in a single request
    /// </summary>
    /// <remarks>
    /// This limit prevents performance issues and excessive memory usage when retrieving attributes.
    /// Set to 500 attributes to balance functionality with performance. Attributes are ordered
    /// by display order and name for consistent presentation.
    /// </remarks>
    private const int MaxAttributes = 500;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductAttributeController"/> class
    /// </summary>
    /// <param name="context">
    /// Database context for product attribute operations. Provides access to attribute entities
    /// and database operations. Cannot be null.
    /// </param>
    /// <param name="logger">
    /// Logger instance for recording attribute events and errors. Used for operational
    /// monitoring, security auditing, and debugging. Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either context or logger parameter is null.
    /// </exception>
    /// <remarks>
    /// This constructor uses dependency injection to provide all required services.
    /// All parameters are validated for null values to ensure the controller operates correctly.
    /// The controller is instantiated by the ASP.NET Core dependency injection container
    /// when handling product attribute requests.
    /// </remarks>
    public ProductAttributeController(
        PostgresqlContext context,
        ILogger<ProductAttributeController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>
    /// A string containing the user ID from the JWT token's NameIdentifier claim,
    /// or null if the claim is not found or the user is not authenticated.
    /// </returns>
    /// <remarks>
    /// This helper method extracts the user identifier from the JWT token claims
    /// to track which user created or modified product attributes for audit trail purposes.
    /// </remarks>
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves all product attributes
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns a list of all active product attributes ordered by display order and name.
    /// This is a public endpoint accessible without authentication, commonly used for
    /// catalog display, filtering interfaces, and product detail pages.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/product-attributes
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint, AllowAnonymous)
    /// </para>
    /// <para>
    /// <strong>Ordering:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Primary:</strong> By display order (ascending) - for UI presentation order</description></item>
    /// <item><description><strong>Secondary:</strong> By name (ascending) - alphabetical within same display order</description></item>
    /// </list>
    /// <para>
    /// <strong>Response includes for each attribute:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Identification:</strong> ID, unique code, and name</description></item>
    /// <item><description><strong>Configuration:</strong> Data type, validation pattern, default value</description></item>
    /// <item><description><strong>Flags:</strong> IsRequired, IsVariantAttribute, IsSearchable, IsFilterable</description></item>
    /// <item><description><strong>Display:</strong> Description and display order</description></item>
    /// <item><description><strong>Audit:</strong> Creation and update timestamps, creator/updater IDs</description></item>
    /// </list>
    /// <para>
    /// <strong>Common attribute examples:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Brand:</strong> Text type, searchable, filterable (non-variant)</description></item>
    /// <item><description><strong>Size:</strong> Text/list type, variant attribute, filterable</description></item>
    /// <item><description><strong>Color:</strong> Text/list type, variant attribute, searchable, filterable</description></item>
    /// <item><description><strong>Weight:</strong> Number type, searchable (non-variant)</description></item>
    /// <item><description><strong>Material:</strong> Text type, searchable, filterable (non-variant)</description></item>
    /// </list>
    /// <para>
    /// <strong>Soft Delete Handling:</strong> Deleted attributes are automatically excluded from results.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Results are limited to 500 attributes maximum to ensure
    /// optimal performance. Results use no-tracking queries for better read performance.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="ProductAttributeEntity"/> objects.
    /// Attributes are ordered by display order then name. Returns an empty array if no attributes exist.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved product attributes. Returns a JSON array of attribute objects
    /// ordered by display order and name. The array may be empty if no attributes have been defined.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving attributes.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductAttributeEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductAttributeEntity>>> GetAllAttributes(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var attributes = await _context
                .ProductAttributes.Where(a => !a.IsDeleted)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .Take(MaxAttributes)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} product attributes", attributes.Count);
            return Ok(attributes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product attributes");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a specific product attribute by its unique identifier
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns detailed information about a single product attribute identified by its unique ID.
    /// This is a public endpoint used for displaying attribute details in product pages and management interfaces.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/product-attributes/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint, AllowAnonymous)
    /// </para>
    /// <para>
    /// <strong>Response includes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Attribute ID, unique code, and name</description></item>
    /// <item><description>Data type and validation pattern</description></item>
    /// <item><description>Configuration flags (required, variant, searchable, filterable)</description></item>
    /// <item><description>Default value and display order</description></item>
    /// <item><description>Description and audit information</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Displaying attribute details in admin interfaces</description></item>
    /// <item><description>Loading attribute configuration for product forms</description></item>
    /// <item><description>Validating attribute usage in product specifications</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the product attribute to retrieve.
    /// Must be a valid, non-empty GUID format (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6).
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="ProductAttributeEntity"/> object
    /// with complete attribute details including configuration, validation rules, and audit information.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the product attribute. Returns a JSON object with complete attribute details.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid attribute ID format (empty GUID or malformed GUID).
    /// </response>
    /// <response code="404">
    /// Product attribute not found. The specified ID does not exist in the system,
    /// the attribute has been soft-deleted, or the ID format is invalid.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving the attribute.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductAttributeEntity>> GetAttributeById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid product attribute GUID provided");
            return BadRequest(new { Message = "Invalid attribute ID" });
        }

        try
        {
            var attribute = await _context
                .ProductAttributes.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                _logger.LogInformation("Product attribute not found: {AttributeId}", id);
                return NotFound(new { Message = "Product attribute not found" });
            }

            return Ok(attribute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product attribute: {AttributeId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a product attribute by its unique code
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns a product attribute identified by its unique code. This endpoint is useful for
    /// system-level integrations where attributes are referenced by code rather than ID.
    /// Attribute codes are unique identifiers used for programmatic access.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/product-attributes/code/product-color
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint, AllowAnonymous)
    /// </para>
    /// <para>
    /// <strong>Code Format Requirements:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Pattern:</strong> Alphanumeric characters, hyphens, and underscores only</description></item>
    /// <item><description><strong>Regex:</strong> ^[a-zA-Z0-9\-_]+$</description></item>
    /// <item><description><strong>Length:</strong> 1 to 50 characters</description></item>
    /// <item><description><strong>Case-Sensitive:</strong> Codes are case-sensitive</description></item>
    /// <item><description><strong>Examples:</strong> product-color, size_variant, brand-name, weight_kg</description></item>
    /// </list>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Code cannot be null, empty, or whitespace</description></item>
    /// <item><description>Code length must not exceed 50 characters</description></item>
    /// <item><description>Code must match the alphanumeric pattern</description></item>
    /// <item><description>Invalid characters result in 400 Bad Request</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>API Integration:</strong> External systems reference attributes by code</description></item>
    /// <item><description><strong>Import/Export:</strong> Data migrations using attribute codes</description></item>
    /// <item><description><strong>Template Systems:</strong> Product templates reference standard attribute codes</description></item>
    /// <item><description><strong>Programmatic Access:</strong> Application logic using predefined attribute codes</description></item>
    /// </list>
    /// </remarks>
    /// <param name="code">
    /// The unique code of the product attribute to retrieve. Must be a valid attribute code
    /// containing only alphanumeric characters, hyphens, and underscores (1-50 characters).
    /// Case-sensitive. Example: "product-color" or "size_variant"
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="ProductAttributeEntity"/> object
    /// matching the specified code, with complete attribute configuration and validation rules.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the product attribute. Returns a JSON object with complete attribute details.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid code format, empty code, code exceeds 50 characters,
    /// or code contains invalid characters. The error message indicates the specific validation issue.
    /// </response>
    /// <response code="404">
    /// Product attribute not found. No attribute exists with the specified code,
    /// or the attribute has been soft-deleted.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving the attribute.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductAttributeEntity>> GetAttributeByCode(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Empty attribute code provided");
            return BadRequest(new { Message = "Attribute code is required" });
        }

        if (code.Length > 50)
        {
            _logger.LogWarning("Attribute code too long: {Length}", code.Length);
            return BadRequest(new { Message = "Attribute code must not exceed 50 characters" });
        }

        // Validate code format
        if (!Regex.IsMatch(code, @"^[a-zA-Z0-9\-_]+$"))
        {
            _logger.LogWarning("Invalid attribute code format: {Code}", code);
            return BadRequest(new { Message = "Invalid attribute code format" });
        }

        try
        {
            var attribute = await _context
                .ProductAttributes.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Code == code && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                _logger.LogInformation("Product attribute code not found: {Code}", code);
                return NotFound(new { Message = "Product attribute not found" });
            }

            return Ok(attribute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product attribute by code");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves all variant-defining product attributes
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns only attributes flagged as variant attributes (IsVariantAttribute = true).
    /// Variant attributes are properties that define different versions of a product,
    /// such as size, color, or style. This endpoint is essential for product variant management.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/product-attributes/variant
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> None (public endpoint, AllowAnonymous)
    /// </para>
    /// <para>
    /// <strong>Variant Attributes Explained:</strong>
    /// </para>
    /// <para>
    /// Variant attributes create different versions of the same base product. For example,
    /// a T-shirt product might have variants based on Size (S, M, L, XL) and Color (Red, Blue, Green).
    /// Each combination of variant attribute values creates a unique product variant with
    /// its own SKU, price, and inventory.
    /// </para>
    /// <para>
    /// <strong>Common variant attributes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Size:</strong> Clothing sizes (XS, S, M, L, XL, XXL), shoe sizes, dimensions</description></item>
    /// <item><description><strong>Color:</strong> Product colors (Red, Blue, Black, White, etc.)</description></item>
    /// <item><description><strong>Style:</strong> Different styles of the same product (Classic, Modern, Vintage)</description></item>
    /// <item><description><strong>Material:</strong> Fabric or material types (Cotton, Polyester, Leather)</description></item>
    /// <item><description><strong>Capacity:</strong> Storage capacity (128GB, 256GB, 512GB)</description></item>
    /// </list>
    /// <para>
    /// <strong>Non-variant attributes (excluded from this endpoint):</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Brand:</strong> Manufacturer or brand name (applies to all variants)</description></item>
    /// <item><description><strong>Weight:</strong> Product weight (specification, not variant-defining)</description></item>
    /// <item><description><strong>Warranty:</strong> Warranty period (same for all variants)</description></item>
    /// <item><description><strong>Country of Origin:</strong> Manufacturing location</description></item>
    /// </list>
    /// <para>
    /// <strong>Ordering:</strong> Results are ordered by display order (ascending) for consistent
    /// UI presentation in variant selection interfaces.
    /// </para>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Variant Creation:</strong> Building product variant matrices</description></item>
    /// <item><description><strong>Product Forms:</strong> Displaying variant selection dropdowns</description></item>
    /// <item><description><strong>Inventory Management:</strong> Tracking stock by variant combinations</description></item>
    /// <item><description><strong>Pricing:</strong> Setting prices for different variant combinations</description></item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong> Results are limited to 500 attributes maximum and use
    /// no-tracking queries for optimal read performance.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="ProductAttributeEntity"/> objects
    /// where IsVariantAttribute is true. Ordered by display order. Returns an empty array if no variant attributes exist.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved variant attributes. Returns a JSON array of attribute objects
    /// flagged as variant attributes, ordered by display order. The array may be empty if
    /// no variant attributes have been defined in the system.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving variant attributes.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("variant")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductAttributeEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductAttributeEntity>>> GetVariantAttributes(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var attributes = await _context
                .ProductAttributes.Where(a => a.IsVariantAttribute && !a.IsDeleted)
                .OrderBy(a => a.DisplayOrder)
                .Take(MaxAttributes)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} variant attributes", attributes.Count);
            return Ok(attributes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variant attributes");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new product attribute
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a new product attribute with the specified configuration, validation rules, and flags.
    /// This endpoint is restricted to users with Admin or Manager roles.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// POST /api/v1/product-attributes
    /// Authorization: Bearer {token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "name": "Product Color",
    ///   "code": "product-color",
    ///   "description": "Available colors for the product",
    ///   "dataType": "List",
    ///   "isRequired": false,
    ///   "isVariantAttribute": true,
    ///   "isSearchable": true,
    ///   "isFilterable": true,
    ///   "validationPattern": null,
    ///   "defaultValue": null,
    ///   "displayOrder": 10
    /// }
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin or Manager role
    /// </para>
    /// <para>
    /// <strong>Required fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>name:</strong> Attribute display name (1-100 characters)</description></item>
    /// <item><description><strong>code:</strong> Unique attribute code (1-50 characters, alphanumeric with hyphens/underscores)</description></item>
    /// </list>
    /// <para>
    /// <strong>Optional fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>description:</strong> Detailed description of the attribute's purpose</description></item>
    /// <item><description><strong>dataType:</strong> Data type (Text, Number, Boolean, Date, List) - default: Text</description></item>
    /// <item><description><strong>isRequired:</strong> Whether the attribute is required for products - default: false</description></item>
    /// <item><description><strong>isVariantAttribute:</strong> Whether this defines product variants - default: false</description></item>
    /// <item><description><strong>isSearchable:</strong> Whether to index for search - default: false</description></item>
    /// <item><description><strong>isFilterable:</strong> Whether to enable filtering - default: false</description></item>
    /// <item><description><strong>validationPattern:</strong> Regex pattern for value validation</description></item>
    /// <item><description><strong>defaultValue:</strong> Default value for the attribute</description></item>
    /// <item><description><strong>displayOrder:</strong> Display order (0 or positive) - default: 0</description></item>
    /// </list>
    /// <para>
    /// <strong>Automatic fields (system-generated):</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>id:</strong> Automatically generated unique identifier (GUID)</description></item>
    /// <item><description><strong>createdAt:</strong> Automatically set to current UTC time</description></item>
    /// <item><description><strong>updatedAt:</strong> Automatically set to current UTC time</description></item>
    /// <item><description><strong>createdBy:</strong> Automatically set to authenticated user's ID</description></item>
    /// <item><description><strong>isDeleted:</strong> Defaults to false (active)</description></item>
    /// </list>
    /// <para>
    /// <strong>Validation rules:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Name must be 1-100 characters</description></item>
    /// <item><description>Code must be 1-50 characters, unique, alphanumeric with hyphens/underscores only</description></item>
    /// <item><description>Code format: ^[a-zA-Z0-9\-_]+$</description></item>
    /// <item><description>Display order must be 0 or positive</description></item>
    /// <item><description>Duplicate codes are rejected with 409 Conflict</description></item>
    /// </list>
    /// <para>
    /// <strong>Data types explained:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Text:</strong> Free-form text input (names, descriptions)</description></item>
    /// <item><description><strong>Number:</strong> Numeric values (weight, dimensions, capacity)</description></item>
    /// <item><description><strong>Boolean:</strong> Yes/No flags (waterproof, wireless, organic)</description></item>
    /// <item><description><strong>Date:</strong> Date values (expiration date, manufacture date)</description></item>
    /// <item><description><strong>List:</strong> Predefined list of values (size options, color choices)</description></item>
    /// </list>
    /// </remarks>
    /// <param name="attribute">
    /// The product attribute entity to create. Must include required fields (name, code).
    /// The ID, timestamps, and creator fields will be automatically generated by the system.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the created <see cref="ProductAttributeEntity"/> object
    /// with the assigned ID, timestamps, and all configuration details. The Location header contains
    /// the URI to retrieve this attribute.
    /// </returns>
    /// <response code="201">
    /// Created. Successfully created the product attribute. The Location header contains the URI
    /// of the newly created attribute resource. Returns the complete attribute object including
    /// the generated ID, timestamps, and creator ID.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid attribute data, missing required fields, or validation errors.
    /// Common causes: missing name/code, invalid code format, name/code exceeds length limits,
    /// negative display order, or null attribute object.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the required role (Admin or Manager)
    /// to create product attributes.
    /// </response>
    /// <response code="409">
    /// Conflict. An attribute with the specified code already exists. Attribute codes must be unique.
    /// Choose a different code or update the existing attribute.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while creating the attribute.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductAttributeEntity>> CreateAttribute(
        [FromBody] ProductAttributeEntity attribute,
        CancellationToken cancellationToken = default
    )
    {
        if (attribute == null)
        {
            _logger.LogWarning("Null product attribute data received");
            return BadRequest(new { Message = "Attribute data is required" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(attribute.Name) || attribute.Name.Length > 100)
        {
            return BadRequest(
                new { Message = "Valid attribute name is required (max 100 characters)" }
            );
        }

        if (string.IsNullOrWhiteSpace(attribute.Code) || attribute.Code.Length > 50)
        {
            return BadRequest(
                new { Message = "Valid attribute code is required (max 50 characters)" }
            );
        }

        if (!Regex.IsMatch(attribute.Code, @"^[a-zA-Z0-9\-_]+$"))
        {
            return BadRequest(
                new
                {
                    Message = "Invalid attribute code format. Use alphanumeric, hyphens, underscores only",
                }
            );
        }

        if (attribute.DisplayOrder < 0)
        {
            return BadRequest(new { Message = "Display order must be a positive number" });
        }

        try
        {
            // Check for duplicate code
            var duplicateCode = await _context.ProductAttributes.AnyAsync(
                a => a.Code == attribute.Code && !a.IsDeleted,
                cancellationToken
            );

            if (duplicateCode)
            {
                _logger.LogWarning(
                    "Duplicate product attribute code attempt: {Code}",
                    attribute.Code
                );
                return Conflict(new { Message = "Attribute code already exists" });
            }

            // Secure assignment
            var newAttribute = new ProductAttributeEntity
            {
                Id = Guid.NewGuid(),
                Name = attribute.Name,
                Code = attribute.Code,
                Description = attribute.Description,
                DataType = attribute.DataType,
                IsRequired = attribute.IsRequired,
                IsVariantAttribute = attribute.IsVariantAttribute,
                IsSearchable = attribute.IsSearchable,
                IsFilterable = attribute.IsFilterable,
                ValidationPattern = attribute.ValidationPattern,
                DefaultValue = attribute.DefaultValue,
                DisplayOrder = attribute.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty,
            };

            _context.ProductAttributes.Add(newAttribute);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Product attribute created: {AttributeId}, Code: {Code}, User: {UserId}",
                newAttribute.Id,
                newAttribute.Code,
                GetCurrentUserId()
            );

            return CreatedAtAction(
                nameof(GetAttributeById),
                new { id = newAttribute.Id },
                newAttribute
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product attribute");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Updates an existing product attribute
    /// </summary>
    /// <remarks>
    /// <para>
    /// Updates the configuration, flags, and validation rules for an existing product attribute.
    /// This endpoint is restricted to users with Admin or Manager roles. The attribute ID cannot be changed.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// PUT /api/v1/product-attributes/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "name": "Product Color Updated",
    ///   "code": "product-color",
    ///   "description": "Updated description",
    ///   "dataType": "List",
    ///   "isRequired": true,
    ///   "isVariantAttribute": true,
    ///   "isSearchable": true,
    ///   "isFilterable": true,
    ///   "validationPattern": null,
    ///   "defaultValue": "Blue",
    ///   "displayOrder": 15
    /// }
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin or Manager role
    /// </para>
    /// <para>
    /// <strong>Updatable fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>name:</strong> Attribute display name (1-100 characters)</description></item>
    /// <item><description><strong>code:</strong> Unique attribute code (must remain unique if changed)</description></item>
    /// <item><description><strong>description:</strong> Attribute description</description></item>
    /// <item><description><strong>dataType:</strong> Data type (Text, Number, Boolean, Date, List)</description></item>
    /// <item><description><strong>isRequired:</strong> Required flag</description></item>
    /// <item><description><strong>isVariantAttribute:</strong> Variant-defining flag</description></item>
    /// <item><description><strong>isSearchable:</strong> Search indexing flag</description></item>
    /// <item><description><strong>isFilterable:</strong> Filtering enablement flag</description></item>
    /// <item><description><strong>validationPattern:</strong> Regex validation pattern</description></item>
    /// <item><description><strong>defaultValue:</strong> Default value</description></item>
    /// <item><description><strong>displayOrder:</strong> Display order (0 or positive)</description></item>
    /// </list>
    /// <para>
    /// <strong>Non-updatable fields (system-managed):</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>id:</strong> Cannot be changed (must match route parameter)</description></item>
    /// <item><description><strong>createdAt:</strong> Original creation timestamp (preserved)</description></item>
    /// <item><description><strong>createdBy:</strong> Original creator ID (preserved)</description></item>
    /// <item><description><strong>updatedAt:</strong> Automatically set to current UTC time</description></item>
    /// <item><description><strong>updatedBy:</strong> Automatically set to authenticated user's ID</description></item>
    /// <item><description><strong>isDeleted:</strong> Use DELETE endpoint for soft deletion</description></item>
    /// </list>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Route ID must match the attribute ID in the request body</description></item>
    /// <item><description>Attribute must exist and not be soft-deleted</description></item>
    /// <item><description>Name must be 1-100 characters</description></item>
    /// <item><description>If code is changed, new code must be unique</description></item>
    /// <item><description>Display order must be 0 or positive</description></item>
    /// </list>
    /// <para>
    /// <strong>Concurrency handling:</strong>
    /// </para>
    /// <para>
    /// The endpoint handles database concurrency conflicts. If another user modified the attribute
    /// simultaneously, a 409 Conflict response is returned. The client should refresh the data
    /// and retry the update with the latest version.
    /// </para>
    /// <para>
    /// <strong>Impact considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Changing IsVariantAttribute may affect existing product variants</description></item>
    /// <item><description>Changing dataType may invalidate existing attribute values</description></item>
    /// <item><description>Changing validationPattern may cause existing values to fail validation</description></item>
    /// <item><description>Consider impact on existing products before making structural changes</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the product attribute to update.
    /// Must be a valid GUID that matches the ID in the request body.
    /// </param>
    /// <param name="attribute">
    /// The updated product attribute entity. The ID must match the route parameter.
    /// Include all fields that should be updated; unchanged fields should contain their current values.
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
    /// No Content. Successfully updated the product attribute. The response body is empty.
    /// The attribute's properties have been updated and the updatedAt timestamp has been recorded.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid data, missing required fields, validation errors, or ID mismatch.
    /// Common causes: route ID doesn't match body ID, null attribute object, invalid name length,
    /// or empty GUID.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the required role (Admin or Manager)
    /// to update product attributes.
    /// </response>
    /// <response code="404">
    /// Not found. The product attribute with the specified ID does not exist or has been soft-deleted.
    /// </response>
    /// <response code="409">
    /// Conflict. Either the new attribute code already exists (duplicate code violation),
    /// or a concurrency conflict occurred (another user modified the attribute simultaneously).
    /// For concurrency conflicts, refresh the data and retry the update.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while updating the attribute.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAttribute(
        Guid id,
        [FromBody] ProductAttributeEntity attribute,
        CancellationToken cancellationToken = default
    )
    {
        if (attribute == null)
        {
            _logger.LogWarning("Null product attribute data received for update");
            return BadRequest(new { Message = "Attribute data is required" });
        }

        if (id == Guid.Empty || id != attribute.Id)
        {
            _logger.LogWarning(
                "ID mismatch in attribute update. Route: {RouteId}, Body: {BodyId}",
                id,
                attribute.Id
            );
            return BadRequest(new { Message = "ID mismatch" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(attribute.Name) || attribute.Name.Length > 100)
        {
            return BadRequest(
                new { Message = "Valid attribute name is required (max 100 characters)" }
            );
        }

        try
        {
            var existingAttribute = await _context.ProductAttributes.FirstOrDefaultAsync(
                a => a.Id == id && !a.IsDeleted,
                cancellationToken
            );

            if (existingAttribute == null)
            {
                _logger.LogWarning("Product attribute not found for update: {AttributeId}", id);
                return NotFound(new { Message = "Product attribute not found" });
            }

            // Check for code conflict if code changed
            if (existingAttribute.Code != attribute.Code)
            {
                var duplicateCode = await _context.ProductAttributes.AnyAsync(
                    a => a.Code == attribute.Code && a.Id != id && !a.IsDeleted,
                    cancellationToken
                );

                if (duplicateCode)
                {
                    _logger.LogWarning(
                        "Duplicate attribute code in update: {Code}",
                        attribute.Code
                    );
                    return Conflict(new { Message = "Attribute code already exists" });
                }
            }

            // Selective update
            existingAttribute.Name = attribute.Name;
            existingAttribute.Code = attribute.Code;
            existingAttribute.Description = attribute.Description;
            existingAttribute.DataType = attribute.DataType;
            existingAttribute.IsRequired = attribute.IsRequired;
            existingAttribute.IsVariantAttribute = attribute.IsVariantAttribute;
            existingAttribute.IsSearchable = attribute.IsSearchable;
            existingAttribute.IsFilterable = attribute.IsFilterable;
            existingAttribute.ValidationPattern = attribute.ValidationPattern;
            existingAttribute.DefaultValue = attribute.DefaultValue;
            existingAttribute.DisplayOrder = attribute.DisplayOrder;
            existingAttribute.UpdatedAt = DateTime.UtcNow;
            existingAttribute.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Product attribute updated: {AttributeId}, User: {UserId}",
                id,
                GetCurrentUserId()
            );
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating attribute: {AttributeId}", id);
            return Conflict(
                new
                {
                    Message = "The attribute was modified by another user. Please refresh and try again",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product attribute: {AttributeId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Deletes a product attribute using soft delete
    /// </summary>
    /// <remarks>
    /// <para>
    /// Performs a soft delete on the specified product attribute by setting its isDeleted flag to true.
    /// The attribute remains in the database for referential integrity and audit purposes but is excluded
    /// from queries. This endpoint is restricted to Admin role only due to the potential impact.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// DELETE /api/v1/product-attributes/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin role only (not Manager)
    /// </para>
    /// <para>
    /// <strong>Soft Delete vs Hard Delete:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Soft Delete:</strong> Sets isDeleted flag to true; record remains in database</description></item>
    /// <item><description><strong>Benefits:</strong> Maintains referential integrity, enables data recovery, preserves audit trails</description></item>
    /// <item><description><strong>Visibility:</strong> Deleted attributes are automatically excluded from all public queries</description></item>
    /// <item><description><strong>Products:</strong> Existing products using this attribute retain their values</description></item>
    /// </list>
    /// <para>
    /// <strong>Impact considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Product Display:</strong> Attribute won't appear in new product forms or filters</description></item>
    /// <item><description><strong>Existing Products:</strong> Products with this attribute retain their data</description></item>
    /// <item><description><strong>Variants:</strong> If it's a variant attribute, existing variants remain but new variants cannot use it</description></item>
    /// <item><description><strong>Search/Filter:</strong> Attribute removed from search indexes and filter options</description></item>
    /// <item><description><strong>Reports:</strong> Historical data remains accessible for reporting</description></item>
    /// </list>
    /// <para>
    /// <strong>Before deletion, consider:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>How many products currently use this attribute?</description></item>
    /// <item><description>Is this a variant-defining attribute with active product variants?</description></item>
    /// <item><description>Are there any integrations or automations that reference this attribute code?</description></item>
    /// <item><description>Should products be updated to remove this attribute first?</description></item>
    /// </list>
    /// <para>
    /// <strong>Audit trail:</strong>
    /// </para>
    /// <para>
    /// Deletions are logged with warning level including the attribute ID, code, and deleting user's ID
    /// for security audit and compliance purposes.
    /// </para>
    /// <para>
    /// <strong>Recovery:</strong>
    /// </para>
    /// <para>
    /// Currently there is no API endpoint to undelete attributes. Recovery requires direct database access.
    /// Consider carefully before deleting attributes.
    /// </para>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the product attribute to delete.
    /// Must be a valid, non-empty GUID referencing an existing, non-deleted attribute.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content) on successful soft deletion.
    /// The response body is empty as per HTTP specification for successful DELETE operations.
    /// </returns>
    /// <response code="204">
    /// No Content. Successfully soft-deleted the product attribute. The response body is empty.
    /// The attribute is now marked as deleted and will not appear in future queries.
    /// The updatedAt timestamp and updatedBy field have been set.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid attribute ID format (empty GUID or malformed GUID).
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the Admin role required to delete
    /// product attributes. Managers are not authorized for this operation.
    /// </response>
    /// <response code="404">
    /// Not found. The product attribute with the specified ID does not exist or has already
    /// been soft-deleted. Attempting to delete an already-deleted attribute returns this status.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while deleting the attribute.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAttribute(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid product attribute GUID provided for deletion");
            return BadRequest(new { Message = "Invalid attribute ID" });
        }

        try
        {
            var attribute = await _context.ProductAttributes.FirstOrDefaultAsync(
                a => a.Id == id && !a.IsDeleted,
                cancellationToken
            );

            if (attribute == null)
            {
                _logger.LogWarning("Product attribute not found for deletion: {AttributeId}", id);
                return NotFound(new { Message = "Product attribute not found" });
            }

            attribute.IsDeleted = true;
            attribute.UpdatedAt = DateTime.UtcNow;
            attribute.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Product attribute deleted: {AttributeId}, Code: {Code}, User: {UserId}",
                id,
                attribute.Code,
                GetCurrentUserId()
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product attribute: {AttributeId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }
}
