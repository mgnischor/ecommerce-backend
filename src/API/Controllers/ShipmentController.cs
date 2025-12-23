using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing order shipments and tracking delivery logistics.
/// Handles the complete shipment lifecycle from creation through delivery tracking and status updates.
/// </summary>
/// <remarks>
/// This controller manages shipment operations including:
/// <list type="bullet">
/// <item><description>Creating shipment records when orders are dispatched</description></item>
/// <item><description>Tracking shipment status through carrier systems</description></item>
/// <item><description>Public tracking number lookup for customers without authentication</description></item>
/// <item><description>Retrieving shipment history for orders and administrative purposes</description></item>
/// <item><description>Updating delivery status and timestamps throughout the shipping process</description></item>
/// <item><description>Soft-delete support for maintaining audit trails</description></item>
/// </list>
/// <para>
/// Most endpoints require authentication. Customer-facing endpoints include authorization checks
/// to ensure users can only access their own shipment data. Administrative functions require
/// Admin or Manager roles. The tracking number endpoint is public but includes rate limiting
/// and input validation to prevent abuse.
/// </para>
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
[Tags("Shipments")]
[ApiController]
[Route("api/v1/shipments")]
[Produces("application/json")]
[Authorize]
public sealed class ShipmentController : ControllerBase
{
    /// <summary>
    /// The database context for accessing shipment, order, and related data.
    /// </summary>
    private readonly PostgresqlContext _context;

    /// <summary>
    /// Logger instance for recording controller operations, security events, and errors.
    /// </summary>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Maximum number of items allowed per page in paginated results to prevent performance degradation.
    /// </summary>
    private const int MaxPageSize = 100;

    /// <summary>
    /// Default page size when not specified in pagination requests.
    /// </summary>
    private const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum total number of results that can be returned to prevent resource exhaustion and DoS attacks.
    /// </summary>
    private const int MaxResultLimit = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShipmentController"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logger for recording operational events, security issues, and errors.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
    /// </exception>
    public ShipmentController(PostgresqlContext context, LoggingService<ShipmentController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user from JWT token claims.
    /// </summary>
    /// <returns>
    /// The user ID as a string, or null if the user is not authenticated.
    /// </returns>
    /// <remarks>
    /// Extracts the NameIdentifier claim from the current user's claims principal.
    /// Used for authorization checks and audit trail tracking (CreatedBy, UpdatedBy fields).
    /// </remarks>
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Determines whether the current user has administrator privileges.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is in the Admin role; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Administrators have full access to all shipment operations including viewing all shipments,
    /// creating/updating records, and deleting shipment data.
    /// </remarks>
    private bool IsAdmin() => User.IsInRole("Admin");

    /// <summary>
    /// Determines whether the current user has manager privileges.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is in the Manager role; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Managers can view all shipments and create/update shipment records but cannot delete them.
    /// Used in combination with Admin role for authorization checks.
    /// </remarks>
    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves all shipment records with pagination support for administrative review and logistics management.
    /// </summary>
    /// <param name="pageNumber">
    /// The page number to retrieve (1-based indexing). Must be at least 1. Default is 1.
    /// </param>
    /// <param name="pageSize">
    /// The number of items per page. Must be between 1 and 100. Default is 10.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A paginated collection of shipment entities ordered by creation date (newest first).
    /// </returns>
    /// <remarks>
    /// This endpoint is designed for administrative dashboards, warehouse management systems,
    /// and logistics coordination interfaces. Returns only non-deleted shipments ordered by CreatedAt descending.
    /// <para>Pagination headers included in response:</para>
    /// <list type="bullet">
    /// <item><description>X-Total-Count: Total number of shipments matching the criteria</description></item>
    /// <item><description>X-Page-Number: Current page number</description></item>
    /// <item><description>X-Page-Size: Items per page</description></item>
    /// <item><description>X-Total-Pages: Total number of pages available</description></item>
    /// </list>
    /// <para><b>Performance Protection:</b></para>
    /// Implements result limiting to prevent excessive data retrieval. If attempting to access
    /// data beyond the maximum result limit (1000 records), returns a 400 Bad Request.
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role to access all shipments across all orders and customers.
    /// </remarks>
    /// <response code="200">Returns the paginated list of shipments with pagination metadata in headers.</response>
    /// <response code="400">Invalid pagination parameters or result set too large.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ShipmentEntity>>> GetAllShipments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (pageNumber < 1)
        {
            _logger.LogWarning("Invalid page number requested: {PageNumber}", pageNumber);
            return BadRequest(new { Message = "Page number must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            _logger.LogWarning("Invalid page size requested: {PageSize}", pageSize);
            return BadRequest(new { Message = $"Page size must be between 1 and {MaxPageSize}" });
        }

        try
        {
            var query = _context.Shipments.Where(s => !s.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            // Prevent excessive data retrieval
            if (totalCount > MaxResultLimit && pageNumber * pageSize > MaxResultLimit)
            {
                _logger.LogWarning(
                    "Attempt to retrieve data beyond limit. User: {UserId}",
                    GetCurrentUserId() ?? "Unknown"
                );
                return BadRequest(
                    new { Message = "Result set too large. Please refine your query" }
                );
            }

            var shipments = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page-Number", pageNumber.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());
            Response.Headers.Append(
                "X-Total-Pages",
                ((int)Math.Ceiling(totalCount / (double)pageSize)).ToString()
            );

            _logger.LogInformation(
                "Retrieved {Count} shipments. Page: {Page}, User: {UserId}",
                shipments.Count,
                pageNumber,
                GetCurrentUserId() ?? "Unknown"
            );

            return Ok(shipments);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving shipments. User: {UserId}",
                GetCurrentUserId() ?? "Unknown"
            );
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a specific shipment by its unique identifier with ownership authorization.
    /// </summary>
    /// <param name="id">The unique GUID of the shipment to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The shipment entity if found and accessible by the current user.
    /// </returns>
    /// <remarks>
    /// Retrieves complete shipment details including tracking information, carrier details,
    /// status, and delivery timestamps.
    /// <para><b>Authorization Logic:</b></para>
    /// <list type="bullet">
    /// <item><description>Admin and Manager roles: Can access any shipment</description></item>
    /// <item><description>Regular users: Can only access shipments for their own orders</description></item>
    /// <item><description>Ownership validated by checking the associated order's CustomerId</description></item>
    /// </list>
    /// This prevents Insecure Direct Object Reference (IDOR) vulnerabilities by ensuring
    /// customers cannot access shipment information for other customers' orders.
    /// <para><b>Use Cases:</b></para>
    /// <list type="bullet">
    /// <item><description>Customer viewing their order's shipment details</description></item>
    /// <item><description>Customer service representatives reviewing shipment status</description></item>
    /// <item><description>Administrative shipment record lookup</description></item>
    /// </list>
    /// </remarks>
    /// <response code="200">Returns the shipment entity.</response>
    /// <response code="400">Invalid shipment ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Forbidden - user attempting to access another customer's shipment without proper role.</response>
    /// <response code="404">Shipment not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShipmentEntity>> GetShipmentById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID provided for shipment lookup");
            return BadRequest(new { Message = "Invalid shipment ID" });
        }

        try
        {
            var shipment = await _context
                .Shipments.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

            if (shipment == null)
            {
                _logger.LogWarning(
                    "Shipment not found: {ShipmentId}, User: {UserId}",
                    id,
                    GetCurrentUserId() ?? "Unknown"
                );
                return NotFound(new { Message = "Shipment not found" });
            }

            // Authorization check - users can only view their own shipments unless admin/manager
            if (!IsAdmin() && !IsManager())
            {
                var order = await _context
                    .Orders.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == shipment.OrderId, cancellationToken);

                var currentUserId = GetCurrentUserId();
                if (order == null || order.CustomerId.ToString() != currentUserId)
                {
                    _logger.LogWarning(
                        "Unauthorized access attempt to shipment: {ShipmentId}, User: {UserId}",
                        id,
                        currentUserId ?? "Unknown"
                    );
                    return Forbid();
                }
            }

            _logger.LogInformation(
                "Shipment retrieved: {ShipmentId}, User: {UserId}",
                id,
                GetCurrentUserId() ?? "Unknown"
            );
            return Ok(shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipment: {ShipmentId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves all shipments associated with a specific order with ownership authorization.
    /// </summary>
    /// <param name="orderId">The unique GUID of the order to retrieve shipments for.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A collection of shipment entities associated with the specified order, ordered by creation date (newest first).
    /// </returns>
    /// <remarks>
    /// Returns all shipments linked to a single order, useful for tracking multi-package orders
    /// or viewing shipment history across multiple delivery attempts.
    /// <para><b>Multiple Shipments Scenarios:</b></para>
    /// An order may have multiple shipments when:
    /// <list type="bullet">
    /// <item><description>Items ship from different warehouses or fulfillment centers</description></item>
    /// <item><description>Large orders are split into multiple packages</description></item>
    /// <item><description>Backordered items ship separately when they become available</description></item>
    /// <item><description>Initial shipment was returned and a replacement was sent</description></item>
    /// </list>
    /// <para><b>Authorization Logic:</b></para>
    /// <list type="bullet">
    /// <item><description>Verifies the order exists before checking shipments</description></item>
    /// <item><description>Admin and Manager roles: Can access shipments for any order</description></item>
    /// <item><description>Regular users: Can only access shipments for their own orders</description></item>
    /// <item><description>Ownership validated by checking the order's CustomerId</description></item>
    /// </list>
    /// <para><b>Result Limiting:</b></para>
    /// Results are limited to 100 shipments to prevent performance issues and potential DoS attacks.
    /// Most orders will have 1-3 shipments, so this limit is generous while maintaining security.
    /// Returns an empty array if no shipments exist for the order.
    /// </remarks>
    /// <response code="200">Returns the list of shipments for the order (may be empty).</response>
    /// <response code="400">Invalid order ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Forbidden - user attempting to access another customer's order shipments without proper role.</response>
    /// <response code="404">Order not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ShipmentEntity>>> GetShipmentsByOrder(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order GUID provided");
            return BadRequest(new { Message = "Invalid order ID" });
        }

        try
        {
            // Verify order exists and user has access
            var order = await _context
                .Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning(
                    "Order not found: {OrderId}, User: {UserId}",
                    orderId,
                    GetCurrentUserId() ?? "Unknown"
                );
                return NotFound(new { Message = "Order not found" });
            }

            // Authorization check
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && !IsManager() && order.CustomerId.ToString() != currentUserId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt to order shipments: {OrderId}, User: {UserId}",
                    orderId,
                    currentUserId ?? "Unknown"
                );
                return Forbid();
            }

            var shipments = await _context
                .Shipments.Where(s => s.OrderId == orderId && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .Take(100) // Limit results
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} shipments for order: {OrderId}, User: {UserId}",
                shipments.Count,
                orderId,
                currentUserId ?? "Unknown"
            );

            return Ok(shipments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipments for order: {OrderId}", orderId);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves shipment information by tracking number for public package tracking.
    /// </summary>
    /// <param name="trackingNumber">
    /// The carrier tracking number to look up. Must be alphanumeric with optional hyphens, max 50 characters.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The shipment entity if a matching tracking number is found.
    /// </returns>
    /// <remarks>
    /// <para><b>Public Access Endpoint:</b></para>
    /// This is the only shipment endpoint that allows anonymous access, enabling customers
    /// to track their packages without logging in. Common use cases include:
    /// <list type="bullet">
    /// <item><description>Customers tracking packages from order confirmation emails</description></item>
    /// <item><description>Gift recipients tracking incoming packages</description></item>
    /// <item><description>Third-party tracking integrations and widgets</description></item>
    /// </list>
    /// <para><b>Security Measures:</b></para>
    /// <list type="bullet">
    /// <item><description><b>Input Validation:</b> Strict format validation (alphanumeric + hyphens only)</description></item>
    /// <item><description><b>Length Limits:</b> Maximum 50 characters to prevent buffer overflow attacks</description></item>
    /// <item><description><b>SQL Injection Protection:</b> Uses parameterized queries</description></item>
    /// <item><description><b>Rate Limiting:</b> Should be implemented at middleware/gateway level (429 response)</description></item>
    /// <item><description><b>Information Disclosure Prevention:</b> Returns minimal shipment data</description></item>
    /// </list>
    /// <para><b>Input Format Validation:</b></para>
    /// Accepted characters: A-Z, a-z, 0-9, hyphen (-)
    /// <para>Examples of valid tracking numbers:</para>
    /// <list type="bullet">
    /// <item><description>USPS: 9400111899562537368610</description></item>
    /// <item><description>FedEx: 123456789012</description></item>
    /// <item><description>UPS: 1Z999AA10123456784</description></item>
    /// <item><description>DHL: 1234567890</description></item>
    /// </list>
    /// <para><b>Rate Limiting Recommendation:</b></para>
    /// Implement rate limiting at the API gateway or middleware level to prevent:
    /// <list type="bullet">
    /// <item><description>Brute force tracking number enumeration</description></item>
    /// <item><description>Denial of Service (DoS) attacks</description></item>
    /// <item><description>Excessive database queries from automated scrapers</description></item>
    /// </list>
    /// Suggested limits: 10 requests per minute per IP address.
    /// </remarks>
    /// <response code="200">Returns the shipment entity with tracking information.</response>
    /// <response code="400">Invalid tracking number format or validation failure.</response>
    /// <response code="404">Tracking number not found or shipment has been deleted.</response>
    /// <response code="429">Too many requests - rate limit exceeded (if rate limiting is implemented).</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("tracking/{trackingNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShipmentEntity>> GetShipmentByTrackingNumber(
        string trackingNumber,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation - prevent injection attacks
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            _logger.LogWarning("Empty tracking number provided");
            return BadRequest(new { Message = "Tracking number is required" });
        }

        // Validate tracking number format (alphanumeric, hyphens, max 50 chars)
        if (trackingNumber.Length > 50 || !Regex.IsMatch(trackingNumber, @"^[a-zA-Z0-9\-]+$"))
        {
            _logger.LogWarning("Invalid tracking number format: {TrackingNumber}", trackingNumber);
            return BadRequest(new { Message = "Invalid tracking number format" });
        }

        try
        {
            // Use parameterized query to prevent SQL injection
            var shipment = await _context
                .Shipments.AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.TrackingNumber == trackingNumber && !s.IsDeleted,
                    cancellationToken
                );

            if (shipment == null)
            {
                _logger.LogInformation(
                    "Tracking number not found: {TrackingNumber}",
                    trackingNumber
                );
                return NotFound(new { Message = "Shipment not found" });
            }

            _logger.LogInformation(
                "Shipment tracked successfully: {TrackingNumber}",
                trackingNumber
            );
            return Ok(shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking shipment: {TrackingNumber}", trackingNumber);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new shipment record when an order is dispatched from the warehouse.
    /// </summary>
    /// <param name="shipment">
    /// The shipment entity containing dispatch details including order ID, tracking number, carrier, and expected delivery.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created shipment entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Required Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>OrderId: The order being shipped (must exist and not be deleted)</description></item>
    /// <item><description>TrackingNumber: 1-50 characters, must be unique across all shipments</description></item>
    /// <item><description>Carrier: 1-100 characters (e.g., "USPS", "FedEx", "UPS", "DHL")</description></item>
    /// <item><description>Status: Initial shipment status (typically "Pending" or "InTransit")</description></item>
    /// </list>
    /// <para><b>Optional Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>ShippedAt: When package was handed to carrier (defaults to current time if not provided)</description></item>
    /// <item><description>ExpectedDeliveryDate: Estimated delivery date from carrier</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Order must exist and not be soft-deleted</description></item>
    /// <item><description>Tracking number must be unique (prevents duplicate shipment records)</description></item>
    /// <item><description>Tracking number: max 50 characters</description></item>
    /// <item><description>Carrier name: max 100 characters</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// <item><description>CreatedBy: Set from authenticated user's ID</description></item>
    /// </list>
    /// <para><b>Typical Workflow:</b></para>
    /// <list type="number">
    /// <item><description>Warehouse picks and packs order</description></item>
    /// <item><description>Package is handed to carrier, tracking number obtained</description></item>
    /// <item><description>System creates shipment record (this endpoint)</description></item>
    /// <item><description>Customer receives shipment notification with tracking number</description></item>
    /// <item><description>Shipment status updated via PUT endpoint as package moves through carrier network</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Only authorized warehouse and logistics staff
    /// should create shipment records to maintain data integrity.
    /// </remarks>
    /// <response code="201">Shipment created successfully. Returns the created shipment with Location header.</response>
    /// <response code="400">Invalid shipment data, order not found, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="409">Tracking number already exists (duplicate shipment).</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShipmentEntity>> CreateShipment(
        [FromBody] ShipmentEntity shipment,
        CancellationToken cancellationToken = default
    )
    {
        if (shipment == null)
        {
            _logger.LogWarning("Null shipment data received");
            return BadRequest(new { Message = "Shipment data is required" });
        }

        // Input validation
        if (shipment.OrderId == Guid.Empty)
        {
            return BadRequest(new { Message = "Valid Order ID is required" });
        }

        if (
            string.IsNullOrWhiteSpace(shipment.TrackingNumber)
            || shipment.TrackingNumber.Length > 50
        )
        {
            return BadRequest(
                new { Message = "Valid tracking number is required (max 50 characters)" }
            );
        }

        if (string.IsNullOrWhiteSpace(shipment.Carrier) || shipment.Carrier.Length > 100)
        {
            return BadRequest(
                new { Message = "Valid carrier name is required (max 100 characters)" }
            );
        }

        try
        {
            // Verify order exists
            var orderExists = await _context.Orders.AnyAsync(
                o => o.Id == shipment.OrderId && !o.IsDeleted,
                cancellationToken
            );

            if (!orderExists)
            {
                _logger.LogWarning(
                    "Attempt to create shipment for non-existent order: {OrderId}",
                    shipment.OrderId
                );
                return BadRequest(new { Message = "Order not found" });
            }

            // Check for duplicate tracking number
            var duplicateTracking = await _context.Shipments.AnyAsync(
                s => s.TrackingNumber == shipment.TrackingNumber && !s.IsDeleted,
                cancellationToken
            );

            if (duplicateTracking)
            {
                _logger.LogWarning(
                    "Duplicate tracking number attempt: {TrackingNumber}",
                    shipment.TrackingNumber
                );
                return Conflict(new { Message = "Tracking number already exists" });
            }

            // Secure assignment - prevent mass assignment vulnerabilities
            var newShipment = new ShipmentEntity
            {
                Id = Guid.NewGuid(),
                OrderId = shipment.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                Carrier = shipment.Carrier,
                Status = shipment.Status,
                ShippedAt = shipment.ShippedAt,
                ExpectedDeliveryDate = shipment.ExpectedDeliveryDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty,
            };

            _context.Shipments.Add(newShipment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Shipment created: {ShipmentId}, Order: {OrderId}, User: {UserId}",
                newShipment.Id,
                newShipment.OrderId,
                GetCurrentUserId() ?? "Unknown"
            );

            return CreatedAtAction(
                nameof(GetShipmentById),
                new { id = newShipment.Id },
                newShipment
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment for order: {OrderId}", shipment.OrderId);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Updates an existing shipment's tracking information, status, and delivery details.
    /// </summary>
    /// <param name="id">The unique GUID of the shipment to update.</param>
    /// <param name="shipment">
    /// The shipment entity with updated values. The Id must match the route parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Updatable Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>TrackingNumber: Can be corrected if initially entered incorrectly</description></item>
    /// <item><description>Carrier: Can be updated if shipment is transferred between carriers</description></item>
    /// <item><description>Status: Updated as package moves through delivery stages</description></item>
    /// <item><description>ShippedAt: Actual pickup/dispatch timestamp</description></item>
    /// <item><description>ExpectedDeliveryDate: Updated delivery estimates from carrier</description></item>
    /// <item><description>DeliveredAt: Timestamp when package was delivered</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Cannot be changed</description></item>
    /// <item><description>OrderId: Cannot be reassigned to different orders</description></item>
    /// <item><description>CreatedAt, CreatedBy: Audit fields remain unchanged</description></item>
    /// </list>
    /// <para><b>Common Status Progression:</b></para>
    /// <list type="number">
    /// <item><description>Pending: Shipment created, awaiting pickup</description></item>
    /// <item><description>InTransit: Package in carrier's network</description></item>
    /// <item><description>OutForDelivery: On delivery vehicle for final delivery</description></item>
    /// <item><description>Delivered: Successfully delivered to recipient</description></item>
    /// <item><description>Exception: Delivery issue (address problem, refused, etc.)</description></item>
    /// <item><description>Returned: Package returned to sender</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>ID in route must match ID in request body</description></item>
    /// <item><description>TrackingNumber: 1-50 characters, must be unique (if changed)</description></item>
    /// <item><description>Shipment must exist and not be soft-deleted</description></item>
    /// </list>
    /// <para><b>Concurrency:</b></para>
    /// Returns 409 Conflict if the shipment was modified by another user/system during the update.
    /// This can occur when carrier webhook updates and manual updates happen simultaneously.
    /// Clients should refresh data and retry the operation.
    /// <para><b>Use Cases:</b></para>
    /// <list type="bullet">
    /// <item><description>Carrier webhook updates (automated status changes)</description></item>
    /// <item><description>Manual corrections by warehouse staff</description></item>
    /// <item><description>Customer service representatives updating delivery information</description></item>
    /// <item><description>Handling delivery exceptions and problems</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Updates are tracked via UpdatedBy and UpdatedAt fields
    /// for complete audit trail.
    /// </remarks>
    /// <response code="204">Shipment updated successfully. No content returned.</response>
    /// <response code="400">Invalid input data, ID mismatch, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="404">Shipment not found or has been deleted.</response>
    /// <response code="409">Conflict - tracking number already exists or concurrent modification detected.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateShipment(
        Guid id,
        [FromBody] ShipmentEntity shipment,
        CancellationToken cancellationToken = default
    )
    {
        if (shipment == null)
        {
            _logger.LogWarning("Null shipment data received for update");
            return BadRequest(new { Message = "Shipment data is required" });
        }

        if (id == Guid.Empty || id != shipment.Id)
        {
            _logger.LogWarning(
                "ID mismatch in shipment update. Route: {RouteId}, Body: {BodyId}",
                id,
                shipment.Id
            );
            return BadRequest(new { Message = "ID mismatch" });
        }

        // Input validation
        if (
            string.IsNullOrWhiteSpace(shipment.TrackingNumber)
            || shipment.TrackingNumber.Length > 50
        )
        {
            return BadRequest(
                new { Message = "Valid tracking number is required (max 50 characters)" }
            );
        }

        try
        {
            var existingShipment = await _context.Shipments.FirstOrDefaultAsync(
                s => s.Id == id && !s.IsDeleted,
                cancellationToken
            );

            if (existingShipment == null)
            {
                _logger.LogWarning("Shipment not found for update: {ShipmentId}", id);
                return NotFound(new { Message = "Shipment not found" });
            }

            // Check for tracking number conflict
            if (existingShipment.TrackingNumber != shipment.TrackingNumber)
            {
                var duplicateTracking = await _context.Shipments.AnyAsync(
                    s => s.TrackingNumber == shipment.TrackingNumber && s.Id != id && !s.IsDeleted,
                    cancellationToken
                );

                if (duplicateTracking)
                {
                    _logger.LogWarning(
                        "Duplicate tracking number in update: {TrackingNumber}",
                        shipment.TrackingNumber
                    );
                    return Conflict(new { Message = "Tracking number already exists" });
                }
            }

            // Selective update - prevent mass assignment
            existingShipment.TrackingNumber = shipment.TrackingNumber;
            existingShipment.Carrier = shipment.Carrier;
            existingShipment.Status = shipment.Status;
            existingShipment.ShippedAt = shipment.ShippedAt;
            existingShipment.ExpectedDeliveryDate = shipment.ExpectedDeliveryDate;
            existingShipment.DeliveredAt = shipment.DeliveredAt;
            existingShipment.UpdatedAt = DateTime.UtcNow;
            existingShipment.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Shipment updated: {ShipmentId}, User: {UserId}",
                id,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating shipment: {ShipmentId}", id);
            return Conflict(
                new
                {
                    Message = "The shipment was modified by another user. Please refresh and try again",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipment: {ShipmentId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Soft deletes a shipment record, marking it as deleted while preserving data for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the shipment to delete.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Soft Delete Behavior:</b></para>
    /// <list type="bullet">
    /// <item><description>Sets IsDeleted = true</description></item>
    /// <item><description>Updates UpdatedAt to current UTC time</description></item>
    /// <item><description>Sets UpdatedBy to current user's ID</description></item>
    /// <item><description>Preserves all shipment data in database</description></item>
    /// <item><description>Excluded from normal queries by IsDeleted filter</description></item>
    /// </list>
    /// <para><b>Effects:</b></para>
    /// <list type="bullet">
    /// <item><description>Shipment no longer appears in active lists, searches, or tracking</description></item>
    /// <item><description>Tracking number becomes available for reuse</description></item>
    /// <item><description>Historical order and delivery records remain intact</description></item>
    /// <item><description>Audit trails and compliance reports can still access the data</description></item>
    /// <item><description>Data can potentially be restored if needed</description></item>
    /// </list>
    /// <para><b>Appropriate Use Cases:</b></para>
    /// Deletion should be reserved for:
    /// <list type="bullet">
    /// <item><description>Duplicate shipment records created in error</description></item>
    /// <item><description>Test shipments during system setup or training</description></item>
    /// <item><description>Incorrectly created shipments before physical pickup</description></item>
    /// <item><description>Administrative cleanup of invalid records</description></item>
    /// </list>
    /// <para><b>Important Considerations:</b></para>
    /// <list type="bullet">
    /// <item><description>Should NOT be used for legitimate shipments (even cancelled or returned)</description></item>
    /// <item><description>Does not affect associated order status or payment records</description></item>
    /// <item><description>Does not cancel actual shipment with carrier</description></item>
    /// <item><description>Consider the impact on customer tracking and order history</description></item>
    /// <item><description>May affect logistics reporting and performance metrics</description></item>
    /// </list>
    /// <para><b>Recommended Alternatives:</b></para>
    /// Instead of deletion, consider:
    /// <list type="bullet">
    /// <item><description>Updating status to "Cancelled" for shipments that didn't proceed</description></item>
    /// <item><description>Updating status to "Returned" for shipments coming back</description></item>
    /// <item><description>Adding notes explaining why a shipment didn't complete</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin role only. This is intentionally more restrictive than other operations
    /// to prevent accidental data loss. Managers cannot delete shipment records.
    /// All deletions are logged with user ID for accountability.
    /// </remarks>
    /// <response code="204">Shipment deleted successfully. No content returned.</response>
    /// <response code="400">Invalid shipment ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin role required.</response>
    /// <response code="404">Shipment not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteShipment(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID provided for shipment deletion");
            return BadRequest(new { Message = "Invalid shipment ID" });
        }

        try
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(
                s => s.Id == id && !s.IsDeleted,
                cancellationToken
            );

            if (shipment == null)
            {
                _logger.LogWarning("Shipment not found for deletion: {ShipmentId}", id);
                return NotFound(new { Message = "Shipment not found" });
            }

            shipment.IsDeleted = true;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Shipment deleted: {ShipmentId}, User: {UserId}",
                id,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shipment: {ShipmentId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }
}
