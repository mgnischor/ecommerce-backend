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

    public RefundController(PostgresqlContext context, ILogger<RefundController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
    public async Task<ActionResult<IEnumerable<RefundEntity>>> GetRefundsByCustomer(
        Guid customerId,
        CancellationToken cancellationToken = default
    )
    {
        var refunds = await _context
            .Refunds.Where(r => r.CustomerId == customerId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(refunds);
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRefund(
        Guid id,
        [FromBody] string reason,
        CancellationToken cancellationToken = default
    )
    {
        var refund = await _context.Refunds.FindAsync(new object[] { id }, cancellationToken);

        if (refund == null || refund.IsDeleted)
            return NotFound(new { Message = $"Refund with ID '{id}' not found" });

        refund.Status = Domain.Enums.RefundStatus.Rejected;
        refund.RejectionReason = reason;
        refund.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
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
