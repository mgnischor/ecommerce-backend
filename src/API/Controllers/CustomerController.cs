using Comex.API.Constants;
using Comex.Application.Interfaces;
using Comex.Application.Services;
using Comex.Domain.Entities;
using Comex.Domain.Policies;
using Comex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comex.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing customers and computing lifecycle segmentation.
/// Supports creating, retrieving, updating, segmenting, and deleting customers with role-based access control.
/// </summary>
/// <remarks>
/// This controller handles all customer operations including:
/// <list type="bullet">
/// <item><description>Managing customer spending and order history</description></item>
/// <item><description>Computing customer segments (VIP, Regular, New, Occasional)</description></item>
/// <item><description>Evaluating churn risk and dormancy for lifecycle management</description></item>
/// <item><description>Soft-delete support for maintaining historical records</description></item>
/// </list>
/// All business rules are enforced via <see cref="CustomerSegmentationPolicy"/>.
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="CustomerController"/> class.
/// </remarks>
/// <param name="context">The database context for data access operations.</param>
/// <param name="logger">The logger for recording operational events and errors.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
/// </exception>
[Tags("Customers")]
[ApiController]
[Route("api/v1/customers")]
[Produces("application/json")]
public sealed class CustomerController(
    PostgresqlContext context,
    LoggingService<CustomerController> logger
) : ControllerBase
{
    /// <summary>
    /// The database context for accessing customer data.
    /// </summary>
    private readonly PostgresqlContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Logger instance for recording controller operations and errors.
    /// </summary>
    private readonly ILoggingService _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves a specific customer by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the customer to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The customer entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Retrieves complete customer details including spending, order history, and activity dates.
    /// Excludes soft-deleted customers.
    /// </remarks>
    /// <response code="200">Returns the customer entity.</response>
    /// <response code="400">Invalid customer ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Customer not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CustomerEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerEntity>> GetCustomerById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid customer GUID provided");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var customer = await _context
                .Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

            if (customer == null)
            {
                _logger.LogInformation("Customer not found: {CustomerId}", id);
                return NotFound(new { Message = ErrorMessages.CustomerNotFound });
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer: {CustomerId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Retrieves a customer by the associated user account identifier.
    /// </summary>
    /// <param name="userId">The unique GUID of the user account to look up.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The customer entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Each user account maps to at most one customer record. Useful for linking
    /// authenticated users to their customer profile.
    /// </remarks>
    /// <response code="200">Returns the customer entity.</response>
    /// <response code="400">Invalid user ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Customer not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CustomerEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerEntity>> GetCustomerByUserId(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user GUID provided");
            return BadRequest(new { Message = ErrorMessages.CustomerUserIdRequired });
        }

        try
        {
            var customer = await _context
                .Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

            if (customer == null)
            {
                _logger.LogInformation("Customer not found for user: {UserId}", userId);
                return NotFound(new { Message = ErrorMessages.CustomerNotFound });
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer for user: {UserId}", userId);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Creates a new customer record linked to a user account.
    /// </summary>
    /// <param name="customer">
    /// The customer entity containing the user ID and initial history.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created customer entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>User ID must be provided</description></item>
    /// <item><description>Each user can have only one customer record</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// </list>
    /// </remarks>
    /// <response code="201">Customer created successfully. Returns the created entity with Location header.</response>
    /// <response code="400">Invalid input data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="409">A customer record already exists for this user.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(CustomerEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerEntity>> CreateCustomer(
        [FromBody] CustomerEntity customer,
        CancellationToken cancellationToken = default
    )
    {
        if (customer == null)
        {
            _logger.LogWarning("Null customer data received");
            return BadRequest(new { Message = ErrorMessages.CustomerDataRequired });
        }

        if (customer.UserId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID provided for customer");
            return BadRequest(new { Message = ErrorMessages.CustomerUserIdRequired });
        }

        try
        {
            var existingCustomer = await _context.Customers.AnyAsync(
                c => c.UserId == customer.UserId && !c.IsDeleted,
                cancellationToken
            );

            if (existingCustomer)
            {
                _logger.LogWarning(
                    "Duplicate customer attempt for user: {UserId}",
                    customer.UserId
                );
                return Conflict(new { Message = "A customer record already exists for this user" });
            }

            var newCustomer = new CustomerEntity
            {
                Id = Guid.NewGuid(),
                UserId = customer.UserId,
                TotalSpending = customer.TotalSpending,
                TotalOrders = customer.TotalOrders,
                LastOrderDate = customer.LastOrderDate,
                LastLoginAt = customer.LastLoginAt,
                HistoricalAverageOrderDays = customer.HistoricalAverageOrderDays,
                IsActive = customer.IsActive,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Customer created: {CustomerId}, User: {UserId}",
                newCustomer.Id,
                newCustomer.UserId
            );

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = newCustomer.Id },
                newCustomer
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer for user: {UserId}", customer.UserId);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Updates an existing customer's spending, order history, and activity data.
    /// </summary>
    /// <param name="id">The unique GUID of the customer to update.</param>
    /// <param name="customer">
    /// The customer entity with updated values. The Id must match the route parameter.
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
    /// <item><description>TotalSpending, TotalOrders</description></item>
    /// <item><description>LastOrderDate, LastLoginAt, HistoricalAverageOrderDays</description></item>
    /// <item><description>IsActive</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id, UserId: Cannot be changed</description></item>
    /// <item><description>CreatedAt: Audit field remains unchanged</description></item>
    /// </list>
    /// </remarks>
    /// <response code="204">Customer updated successfully. No content returned.</response>
    /// <response code="400">Invalid input data or ID mismatch.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Customer not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] CustomerEntity customer,
        CancellationToken cancellationToken = default
    )
    {
        if (customer == null)
        {
            _logger.LogWarning("Null customer data received for update");
            return BadRequest(new { Message = ErrorMessages.CustomerDataRequired });
        }

        if (id == Guid.Empty || id != customer.Id)
        {
            _logger.LogWarning(
                "ID mismatch in customer update. Route: {RouteId}, Body: {BodyId}",
                id,
                customer.Id
            );
            return BadRequest(new { Message = ErrorMessages.IdMismatch });
        }

        try
        {
            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(
                c => c.Id == id && !c.IsDeleted,
                cancellationToken
            );

            if (existingCustomer == null)
            {
                _logger.LogWarning("Customer not found for update: {CustomerId}", id);
                return NotFound(new { Message = ErrorMessages.CustomerNotFound });
            }

            existingCustomer.TotalSpending = customer.TotalSpending;
            existingCustomer.TotalOrders = customer.TotalOrders;
            existingCustomer.LastOrderDate = customer.LastOrderDate;
            existingCustomer.LastLoginAt = customer.LastLoginAt;
            existingCustomer.HistoricalAverageOrderDays = customer.HistoricalAverageOrderDays;
            existingCustomer.IsActive = customer.IsActive;
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer updated: {CustomerId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Computes the customer segmentation and lifecycle analysis for a customer.
    /// </summary>
    /// <param name="id">The unique GUID of the customer to analyze.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// An object containing the customer segment, churn risk, and lifecycle flags.
    /// </returns>
    /// <remarks>
    /// <para><b>Computed Values:</b></para>
    /// <list type="bullet">
    /// <item><description>Segment: VIP, Regular, New, or Occasional via <see cref="CustomerSegmentationPolicy.GetCustomerSegment"/></description></item>
    /// <item><description>ChurnRisk: Critical, High, Medium, or Low via <see cref="CustomerSegmentationPolicy.GetChurnRiskLevel"/></description></item>
    /// <item><description>AtRisk: True when inactivity exceeds twice the historical order cadence via <see cref="CustomerSegmentationPolicy.IsAtRiskCustomer"/></description></item>
    /// <item><description>Churned: True when no order within the churn threshold via <see cref="CustomerSegmentationPolicy.IsChurnedCustomer"/></description></item>
    /// <item><description>Dormant: True when no login within the dormant threshold via <see cref="CustomerSegmentationPolicy.IsDormantCustomer"/></description></item>
    /// <item><description>HighValue: True when total spending meets the high value threshold via <see cref="CustomerSegmentationPolicy.IsHighValueCustomer"/></description></item>
    /// </list>
    /// </remarks>
    /// <response code="200">Returns the customer segmentation analysis.</response>
    /// <response code="400">Invalid customer ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Customer not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}/segment")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerSegment(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid customer GUID provided for segmentation");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var customer = await _context
                .Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

            if (customer == null)
            {
                _logger.LogInformation("Customer not found for segmentation: {CustomerId}", id);
                return NotFound(new { Message = ErrorMessages.CustomerNotFound });
            }

            var now = DateTime.UtcNow;
            var daysSinceLastOrder = customer.LastOrderDate.HasValue
                ? Math.Max((int)(now - customer.LastOrderDate.Value).TotalDays, 0)
                : int.MaxValue;
            var daysSinceLastLogin = customer.LastLoginAt.HasValue
                ? Math.Max((int)(now - customer.LastLoginAt.Value).TotalDays, 0)
                : int.MaxValue;

            return Ok(
                new
                {
                    customer.Id,
                    customer.UserId,
                    customer.TotalSpending,
                    customer.TotalOrders,
                    Segment = CustomerSegmentationPolicy.GetCustomerSegment(
                        customer.TotalSpending,
                        customer.TotalOrders
                    ),
                    ChurnRisk = CustomerSegmentationPolicy.GetChurnRiskLevel(
                        daysSinceLastOrder,
                        customer.TotalOrders
                    ),
                    IsAtRisk = CustomerSegmentationPolicy.IsAtRiskCustomer(
                        daysSinceLastOrder,
                        customer.HistoricalAverageOrderDays
                    ),
                    IsChurned = CustomerSegmentationPolicy.IsChurnedCustomer(daysSinceLastOrder),
                    IsDormant = CustomerSegmentationPolicy.IsDormantCustomer(daysSinceLastLogin),
                    IsHighValue = CustomerSegmentationPolicy.IsHighValueCustomer(
                        customer.TotalSpending
                    ),
                    IsNewCustomer = CustomerSegmentationPolicy.IsNewCustomer(customer.TotalOrders),
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing customer segmentation: {CustomerId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Soft deletes a customer, marking it as deleted while preserving the record for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the customer to delete.</param>
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
    /// <item><description>Preserves all customer history for reporting and audit trails</description></item>
    /// </list>
    /// <para>
    /// A deleted customer no longer appears in lookups or segmentation analysis.
    /// </para>
    /// </remarks>
    /// <response code="204">Customer deleted successfully. No content returned.</response>
    /// <response code="400">Invalid customer ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Customer not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCustomer(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid customer GUID provided for deletion");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(
                c => c.Id == id && !c.IsDeleted,
                cancellationToken
            );

            if (customer == null)
            {
                _logger.LogWarning("Customer not found for deletion: {CustomerId}", id);
                return NotFound(new { Message = ErrorMessages.CustomerNotFound });
            }

            customer.IsDeleted = true;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Customer deleted: {CustomerId}, User: {UserId}",
                id,
                customer.UserId
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }
}
