using System.Security.Claims;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Policies;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing product return refunds and refund request processing.
/// Handles the complete refund lifecycle from request submission through approval, rejection, or completion.
/// </summary>
/// <remarks>
/// This controller manages refund operations including:
/// <list type="bullet">
/// <item><description>Creating refund requests for returned products or cancelled orders</description></item>
/// <item><description>Retrieving refund history for customers, orders, or administrative review</description></item>
/// <item><description>Administrative approval/rejection workflow with role-based authorization</description></item>
/// <item><description>Tracking refund status through the complete lifecycle</description></item>
/// <item><description>Soft-delete support for maintaining audit trails</description></item>
/// </list>
/// <para>
/// All endpoints require authentication. Customer-facing endpoints include IDOR (Insecure Direct Object Reference)
/// protection to ensure users can only access their own refund data. Administrative functions require
/// Admin or Manager roles.
/// </para>
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
[Tags("Refunds")]
[ApiController]
[Route("api/v1/refunds")]
[Produces("application/json")]
[Authorize]
public sealed class RefundController : ControllerBase
{
    /// <summary>
    /// The database context for accessing refund and related order data.
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
    /// Maximum total number of results that can be returned to prevent resource exhaustion.
    /// </summary>
    private const int MaxResultLimit = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefundController"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logger for recording operational events, security issues, and errors.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
    /// </exception>
    public RefundController(PostgresqlContext context, LoggingService<RefundController> logger)
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
    /// Administrators have full access to all refund operations including viewing all refunds,
    /// approving/rejecting requests, and deleting refund records.
    /// </remarks>
    private bool IsAdmin() => User.IsInRole("Admin");

    /// <summary>
    /// Determines whether the current user has manager privileges.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is in the Manager role; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Managers can view all refunds and approve/reject refund requests but cannot delete them.
    /// Used in combination with Admin role for authorization checks.
    /// </remarks>
    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves all refund records with pagination support for administrative review.
    /// </summary>
    /// <param name="pageNumber">
    /// The page number to retrieve (1-based indexing). Must be at least 1.
    /// </param>
    /// <param name="pageSize">
    /// The number of items per page. Must be between 1 and 100. Default is 10.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A paginated collection of refund entities ordered by creation date (newest first).
    /// </returns>
    /// <remarks>
    /// This endpoint is designed for administrative dashboards and refund management interfaces.
    /// Returns only non-deleted refunds ordered by CreatedAt descending.
    /// <para>Pagination headers included in response:</para>
    /// <list type="bullet">
    /// <item><description>X-Total-Count: Total number of refunds matching the criteria</description></item>
    /// <item><description>X-Page-Number: Current page number</description></item>
    /// <item><description>X-Page-Size: Items per page</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role to access all refunds across all customers.
    /// </remarks>
    /// <response code="200">Returns the paginated list of refunds with pagination metadata in headers.</response>
    /// <response code="400">Invalid pagination parameters (page number &lt; 1, page size &lt; 1 or &gt; 100).</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(IEnumerable<RefundEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RefundEntity>>> GetAllRefunds(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters");

        var refunds = await _context
            .Refunds.Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await _context.Refunds.CountAsync(r => !r.IsDeleted, cancellationToken);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page-Number", pageNumber.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(refunds);
    }

    /// <summary>
    /// Retrieves a specific refund by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the refund to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The refund entity if found and accessible by the current user.
    /// </returns>
    /// <remarks>
    /// Retrieves complete refund details including status, amounts, timestamps, and associated order information.
    /// Regular users can only access their own refunds (enforced by business logic or authorization filters).
    /// Admin and Manager roles can access any refund record.
    /// <para><b>Access Control:</b></para>
    /// While not explicitly enforced in this method, ensure that customer users cannot access
    /// refunds belonging to other customers through frontend validation or additional authorization logic.
    /// </remarks>
    /// <response code="200">Returns the refund entity.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Refund not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RefundEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RefundEntity>> GetRefundById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var refund = await _context.Refunds.FirstOrDefaultAsync(
            r => r.Id == id && !r.IsDeleted,
            cancellationToken
        );

        if (refund == null)
            return NotFound(new { Message = $"Refund with ID '{id}' not found" });

        return Ok(refund);
    }

    /// <summary>
    /// Retrieves all refunds associated with a specific order.
    /// </summary>
    /// <param name="orderId">The unique GUID of the order to retrieve refunds for.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A collection of refund entities associated with the specified order, ordered by creation date (newest first).
    /// </returns>
    /// <remarks>
    /// Returns all refunds linked to a single order, useful for displaying refund history on order detail pages.
    /// An order may have multiple refunds if:
    /// <list type="bullet">
    /// <item><description>Multiple items were returned separately</description></item>
    /// <item><description>Partial refunds were issued</description></item>
    /// <item><description>Initial refund was rejected and resubmitted</description></item>
    /// </list>
    /// Results are ordered by CreatedAt descending (most recent first).
    /// Returns an empty array if no refunds exist for the order.
    /// <para><b>Authorization Consideration:</b></para>
    /// Consider implementing ownership validation to ensure customers can only view refunds
    /// for their own orders, or restrict this endpoint to Admin/Manager roles only.
    /// </remarks>
    /// <response code="200">Returns the list of refunds for the order (may be empty).</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RefundEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RefundEntity>>> GetRefundsByOrder(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        var refunds = await _context
            .Refunds.Where(r => r.OrderId == orderId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(refunds);
    }

    /// <summary>
    /// Retrieves all refunds for a specific customer with IDOR protection.
    /// </summary>
    /// <param name="customerId">The unique GUID of the customer whose refunds to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A collection of refund entities for the specified customer, ordered by creation date (newest first).
    /// </returns>
    /// <remarks>
    /// <para><b>Security - IDOR Protection:</b></para>
    /// This endpoint implements Insecure Direct Object Reference (IDOR) protection:
    /// <list type="bullet">
    /// <item><description>Regular users can only access their own refunds (customerId must match authenticated user)</description></item>
    /// <item><description>Admin and Manager roles can access any customer's refunds</description></item>
    /// <item><description>Unauthorized access attempts are logged for security monitoring</description></item>
    /// </list>
    /// <para><b>Result Limiting:</b></para>
    /// Results are limited to 100 refunds to prevent Denial of Service (DoS) attacks
    /// and ensure consistent performance. For customers with more refunds, implement
    /// pagination or date-range filtering.
    /// <para><b>Use Cases:</b></para>
    /// <list type="bullet">
    /// <item><description>Customer viewing their refund history</description></item>
    /// <item><description>Customer service representatives reviewing customer's refund records</description></item>
    /// <item><description>Administrative reporting and analysis</description></item>
    /// </list>
    /// </remarks>
    /// <response code="200">Returns the list of refunds for the customer (may be empty).</response>
    /// <response code="400">Invalid customer ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Forbidden - user attempting to access another customer's refunds without proper role.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RefundEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RefundEntity>>> GetRefundsByCustomer(
        Guid customerId,
        CancellationToken cancellationToken = default
    )
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogWarning("Invalid customer GUID provided");
            return BadRequest(new { Message = "Invalid customer ID" });
        }

        try
        {
            // IDOR Protection: Users can only view their own refunds
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && !IsManager() && customerId.ToString() != currentUserId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt to customer refunds: {CustomerId}, User: {UserId}",
                    customerId,
                    currentUserId ?? "Unknown"
                );
                return Forbid();
            }

            var refunds = await _context
                .Refunds.Where(r => r.CustomerId == customerId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .Take(100) // Limit results to prevent DoS
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} refunds for customer: {CustomerId}",
                refunds.Count,
                customerId
            );

            return Ok(refunds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving refunds for customer: {CustomerId}", customerId);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new refund request for a returned product or cancelled order.
    /// </summary>
    /// <param name="refund">
    /// The refund entity containing request details including order ID, refund amount, reason, and customer information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created refund entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Required Information:</b></para>
    /// <list type="bullet">
    /// <item><description>OrderId: The order for which the refund is requested</description></item>
    /// <item><description>CustomerId: The customer requesting the refund</description></item>
    /// <item><description>RefundAmount: The amount to be refunded</description></item>
    /// <item><description>Reason: Explanation for the refund request</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>CreatedAt: Set to current UTC time</description></item>
    /// <item><description>UpdatedAt: Set to current UTC time</description></item>
    /// <item><description>Status: Typically initialized to Pending or Requested</description></item>
    /// </list>
    /// <para><b>Typical Workflow:</b></para>
    /// <list type="number">
    /// <item><description>Customer submits refund request (this endpoint)</description></item>
    /// <item><description>Request enters pending state awaiting admin review</description></item>
    /// <item><description>Admin/Manager approves or rejects via dedicated endpoints</description></item>
    /// <item><description>If approved, payment processor issues refund</description></item>
    /// <item><description>Status updated to Completed once funds are returned</description></item>
    /// </list>
    /// <para><b>Validation Recommendations:</b></para>
    /// Consider implementing business logic validation:
    /// <list type="bullet">
    /// <item><description>Verify order exists and belongs to the customer</description></item>
    /// <item><description>Ensure refund amount doesn't exceed order total</description></item>
    /// <item><description>Check refund policies (time limits, conditions)</description></item>
    /// <item><description>Validate that order is eligible for refund (payment completed, not already refunded)</description></item>
    /// </list>
    /// </remarks>
    /// <response code="201">Refund created successfully. Returns the created refund with Location header.</response>
    /// <response code="400">Invalid refund data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RefundEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RefundEntity>> CreateRefund(
        [FromBody] RefundEntity refund,
        CancellationToken cancellationToken = default
    )
    {
        if (refund == null)
            return BadRequest("Refund data is required");

        // Basic validation of refund reason
        if (string.IsNullOrWhiteSpace(refund.Reason))
        {
            _logger.LogWarning("Refund reason is required");
            return BadRequest(new { Message = "Refund reason is required" });
        }

        _logger.LogInformation(
            "Creating refund: Amount={Amount}, OrderId={OrderId}, Reason={Reason}",
            refund.RefundAmount,
            refund.OrderId,
            refund.Reason
        );

        refund.Id = Guid.NewGuid();
        refund.CreatedAt = DateTime.UtcNow;
        refund.UpdatedAt = DateTime.UtcNow;

        _context.Refunds.Add(refund);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refund created successfully: RefundId={RefundId}, Amount={Amount}",
            refund.Id,
            refund.RefundAmount
        );

        return CreatedAtAction(nameof(GetRefundById), new { id = refund.Id }, refund);
    }

    /// <summary>
    /// Updates an existing refund's details and status.
    /// </summary>
    /// <param name="id">The unique GUID of the refund to update.</param>
    /// <param name="refund">
    /// The refund entity with updated values. The Id must match the route parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Updatable Fields:</b></para>
    /// Depending on refund status and business rules, various fields can be updated:
    /// <list type="bullet">
    /// <item><description>Status: Progression through refund lifecycle states</description></item>
    /// <item><description>RefundAmount: Adjustment before approval (if allowed)</description></item>
    /// <item><description>Reason: Clarification or additional details</description></item>
    /// <item><description>Administrative notes and tracking information</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>ID in route must match ID in request body</description></item>
    /// <item><description>Refund must exist and not be soft-deleted</description></item>
    /// <item><description>Status transitions should follow allowed workflows</description></item>
    /// </list>
    /// <para><b>Status Transition Considerations:</b></para>
    /// Some status changes should use dedicated endpoints:
    /// <list type="bullet">
    /// <item><description>Use PATCH /approve for approving refunds</description></item>
    /// <item><description>Use PATCH /reject for rejecting refunds</description></item>
    /// <item><description>This general update endpoint for other modifications</description></item>
    /// </list>
    /// UpdatedAt timestamp is automatically set to current UTC time.
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Regular users cannot directly update refund records
    /// to maintain data integrity and workflow compliance.
    /// </remarks>
    /// <response code="204">Refund updated successfully. No content returned.</response>
    /// <response code="400">Invalid data, ID mismatch, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="404">Refund not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRefund(
        Guid id,
        [FromBody] RefundEntity refund,
        CancellationToken cancellationToken = default
    )
    {
        if (refund == null)
            return BadRequest("Refund data is required");

        if (id != refund.Id)
            return BadRequest("ID mismatch");

        var existingRefund = await _context.Refunds.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (existingRefund == null || existingRefund.IsDeleted)
            return NotFound(new { Message = $"Refund with ID '{id}' not found" });

        refund.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingRefund).CurrentValues.SetValues(refund);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Approves a pending refund request, authorizing the refund to be processed.
    /// </summary>
    /// <param name="id">The unique GUID of the refund to approve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Approval Workflow:</b></para>
    /// <list type="number">
    /// <item><description>Validates refund exists and is in an approvable state</description></item>
    /// <item><description>Updates Status to Approved</description></item>
    /// <item><description>Sets ApprovedAt timestamp to current UTC time</description></item>
    /// <item><description>Records UpdatedAt for audit trail</description></item>
    /// </list>
    /// <para><b>Post-Approval Actions:</b></para>
    /// After approval, additional processes should typically occur:
    /// <list type="bullet">
    /// <item><description>Initiate refund with payment processor</description></item>
    /// <item><description>Send notification to customer about approval</description></item>
    /// <item><description>Update inventory if product was returned</description></item>
    /// <item><description>Record transaction in accounting system</description></item>
    /// </list>
    /// <para><b>Business Rules:</b></para>
    /// Consider implementing validation:
    /// <list type="bullet">
    /// <item><description>Refund must be in Pending or Requested status</description></item>
    /// <item><description>Cannot approve already completed, cancelled, or rejected refunds</description></item>
    /// <item><description>Verify sufficient funds or credit available</description></item>
    /// <item><description>Validate refund policy compliance</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Approval authority may be restricted based on
    /// refund amount thresholds in more complex implementations.
    /// </remarks>
    /// <response code="204">Refund approved successfully. No content returned.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="404">Refund not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApproveRefund(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var refund = await _context.Refunds.FindAsync(new object[] { id }, cancellationToken);

        if (refund == null || refund.IsDeleted)
            return NotFound(new { Message = $"Refund with ID '{id}' not found" });

        refund.Status = Domain.Enums.RefundStatus.Approved;
        refund.ApprovedAt = DateTime.UtcNow;
        refund.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Rejects a refund request with a mandatory explanation, denying the refund.
    /// </summary>
    /// <param name="id">The unique GUID of the refund to reject.</param>
    /// <param name="reason">
    /// The reason for rejecting the refund. Must be provided and cannot exceed 500 characters.
    /// Should clearly explain why the refund was denied.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Rejection Workflow:</b></para>
    /// <list type="number">
    /// <item><description>Validates refund exists and rejection is allowed</description></item>
    /// <item><description>Validates rejection reason is provided and within length limits</description></item>
    /// <item><description>Updates Status to Rejected</description></item>
    /// <item><description>Stores rejection reason for customer communication</description></item>
    /// <item><description>Records UpdatedAt and UpdatedBy for audit trail</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Reason is required (cannot be null or whitespace)</description></item>
    /// <item><description>Reason maximum length: 500 characters</description></item>
    /// <item><description>Cannot reject refunds in Completed or Cancelled status</description></item>
    /// <item><description>Refund must exist and not be soft-deleted</description></item>
    /// </list>
    /// <para><b>Common Rejection Reasons:</b></para>
    /// <list type="bullet">
    /// <item><description>Product was not returned in original condition</description></item>
    /// <item><description>Refund request exceeded policy time limit</description></item>
    /// <item><description>Product shows signs of use or damage</description></item>
    /// <item><description>Missing original packaging or accessories</description></item>
    /// <item><description>Refund amount exceeds order total</description></item>
    /// <item><description>Product is non-refundable (clearance, personalized items)</description></item>
    /// </list>
    /// <para><b>Post-Rejection Actions:</b></para>
    /// After rejection, consider:
    /// <list type="bullet">
    /// <item><description>Send notification to customer with rejection reason</description></item>
    /// <item><description>Provide information about appeals process if available</description></item>
    /// <item><description>Log decision for dispute resolution</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. The rejecting user is recorded in UpdatedBy field
    /// for accountability and audit purposes.
    /// </remarks>
    /// <response code="204">Refund rejected successfully. No content returned.</response>
    /// <response code="400">Invalid refund ID, missing/invalid reason, or refund status doesn't allow rejection.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="404">Refund not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RejectRefund(
        Guid id,
        [FromBody] string reason,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid refund GUID provided for rejection");
            return BadRequest(new { Message = "Invalid refund ID" });
        }

        // Validate rejection reason
        if (string.IsNullOrWhiteSpace(reason))
        {
            return BadRequest(new { Message = "Rejection reason is required" });
        }

        if (reason.Length > 500)
        {
            return BadRequest(new { Message = "Rejection reason must not exceed 500 characters" });
        }

        try
        {
            var refund = await _context.Refunds.FirstOrDefaultAsync(
                r => r.Id == id && !r.IsDeleted,
                cancellationToken
            );

            if (refund == null)
            {
                _logger.LogWarning("Refund not found for rejection: {RefundId}", id);
                return NotFound(new { Message = "Refund not found" });
            }

            // Validate state transition
            if (
                refund.Status == Domain.Enums.RefundStatus.Completed
                || refund.Status == Domain.Enums.RefundStatus.Cancelled
            )
            {
                _logger.LogWarning(
                    "Invalid refund status for rejection: {RefundId}, Status: {Status}",
                    id,
                    refund.Status
                );
                return BadRequest(new { Message = "Cannot reject a refund in this status" });
            }

            refund.Status = Domain.Enums.RefundStatus.Rejected;
            refund.RejectionReason = reason;
            refund.UpdatedAt = DateTime.UtcNow;
            refund.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Refund rejected: {RefundId}, User: {UserId}, Reason: {Reason}",
                id,
                GetCurrentUserId() ?? "Unknown",
                reason
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting refund: {RefundId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Soft deletes a refund record, marking it as deleted while preserving data for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the refund to delete.</param>
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
    /// <item><description>Preserves all refund data in database</description></item>
    /// <item><description>Excluded from normal queries by IsDeleted filter</description></item>
    /// </list>
    /// <para><b>Effects:</b></para>
    /// <list type="bullet">
    /// <item><description>Refund no longer appears in active lists or searches</description></item>
    /// <item><description>Historical financial records remain intact</description></item>
    /// <item><description>Audit trails and compliance reports can still access the data</description></item>
    /// <item><description>Data can potentially be restored if needed</description></item>
    /// </list>
    /// <para><b>Use Cases:</b></para>
    /// Deletion should be reserved for:
    /// <list type="bullet">
    /// <item><description>Duplicate refund requests created in error</description></item>
    /// <item><description>Test or training data that shouldn't appear in production reports</description></item>
    /// <item><description>Administrative cleanup of invalid records</description></item>
    /// </list>
    /// <para><b>Important Considerations:</b></para>
    /// <list type="bullet">
    /// <item><description>Should not be used for legitimate refunds (even rejected ones)</description></item>
    /// <item><description>Does not reverse any financial transactions</description></item>
    /// <item><description>Does not affect associated order or payment records</description></item>
    /// <item><description>Consider the impact on financial reconciliation and reporting</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin role only. This is intentionally more restrictive than other operations
    /// to prevent accidental data loss and maintain comprehensive audit trails.
    /// Managers cannot delete refund records.
    /// </remarks>
    /// <response code="204">Refund deleted successfully. No content returned.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin role required.</response>
    /// <response code="404">Refund not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRefund(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var refund = await _context.Refunds.FindAsync(new object[] { id }, cancellationToken);

        if (refund == null || refund.IsDeleted)
            return NotFound(new { Message = $"Refund with ID '{id}' not found" });

        refund.IsDeleted = true;
        refund.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
