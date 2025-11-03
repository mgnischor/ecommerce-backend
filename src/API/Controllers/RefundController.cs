using System.Security.Claims;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Refund management endpoints
/// </summary>
[Tags("Refunds")]
[ApiController]
[Route("api/v1/refunds")]
[Produces("application/json")]
[Authorize]
public sealed class RefundController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<RefundController> _logger;
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private const int MaxResultLimit = 1000;

    public RefundController(PostgresqlContext context, ILogger<RefundController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdmin() => User.IsInRole("Admin");

    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves all refunds with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(IEnumerable<RefundEntity>), StatusCodes.Status200OK)]
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
    /// Retrieves a specific refund by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RefundEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Retrieves refunds for a specific order
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RefundEntity>), StatusCodes.Status200OK)]
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
    /// Retrieves refunds for a specific customer
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RefundEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                    currentUserId
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
    /// Creates a new refund request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RefundEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RefundEntity>> CreateRefund(
        [FromBody] RefundEntity refund,
        CancellationToken cancellationToken = default
    )
    {
        if (refund == null)
            return BadRequest("Refund data is required");

        refund.Id = Guid.NewGuid();
        refund.CreatedAt = DateTime.UtcNow;
        refund.UpdatedAt = DateTime.UtcNow;

        _context.Refunds.Add(refund);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetRefundById), new { id = refund.Id }, refund);
    }

    /// <summary>
    /// Updates an existing refund
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Approves a refund request
    /// </summary>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Rejects a refund request
    /// </summary>
    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                GetCurrentUserId(),
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
    /// Deletes a refund (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
